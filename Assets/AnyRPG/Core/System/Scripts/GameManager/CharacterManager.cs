using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterManager : ConfiguredMonoBehaviour {

        // keep track of which request spawned something
        private int clientSpawnRequestId;
        private int serverSpawnRequestId;

        private List<UnitController> localUnits = new List<UnitController>();
        private List<UnitController> networkUnownedUnits = new List<UnitController>();
        private List<UnitController> networkOwnedUnits = new List<UnitController>();
        private List<UnitController> serverOwnedUnits = new List<UnitController>();

        // keep track of spawn requests so that they can be configured after spawning
        private Dictionary<int, CharacterRequestData> unitSpawnRequests = new Dictionary<int, CharacterRequestData>();
        private Dictionary<UnitController, CharacterRequestData> modelSpawnRequests = new Dictionary<UnitController, CharacterRequestData>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private NetworkManagerClient networkManagerClient = null;
        private PlayerManager playerManager = null;
        private NetworkManagerServer networkManagerServer = null;
        private PlayerManagerServer playerManagerServer = null;

        public List<UnitController> LocalUnits { get => localUnits; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            playerManager = systemGameManager.PlayerManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public int GetSpawnRequestId() {
            return clientSpawnRequestId++;
        }

        public int GetServerSpawnRequestId() {
            return serverSpawnRequestId++;
        }

        private void SetupUnitSpawnRequest(CharacterRequestData characterRequestData) {
            Debug.Log($"CharacterManager.SetupUnitSpawnRequest({characterRequestData.characterConfigurationRequest.unitProfile.resourceName})");

            characterRequestData.spawnRequestId = GetSpawnRequestId();
            AddUnitSpawnRequest(characterRequestData.spawnRequestId, characterRequestData);

        }

        public void SpawnLobbyGamePlayer(int gameId, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnLobbyGamePlayer({gameId}, {position}, {forward})");

            SetupUnitSpawnRequest(characterRequestData);

            networkManagerClient.SpawnLobbyGamePlayer(gameId, characterRequestData, parentTransform, position, forward);

        }

        public void SpawnPlayer(PlayerCharacterSaveData playerCharacterSaveData, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {

            SetupUnitSpawnRequest(characterRequestData);

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.SpawnPlayer(playerCharacterSaveData.PlayerCharacterId, characterRequestData, parentTransform);
                return;
            }

            // game mode is local, spawn locally
            SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }

        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            Debug.Log($"CharacterManager.SpawnUnitPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {scene.name})");

            SetupUnitSpawnRequest(characterRequestData);

            return networkManagerServer.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.SpawnUnitPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            SetupUnitSpawnRequest(characterRequestData);

            return SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }


        public void ProcessStopNetworkUnit(UnitController unitController) {
            Debug.Log($"CharacterManager.ProcessStopClient({unitController.gameObject.name})");
            if (unitController.IsOwner == true && networkOwnedUnits.Contains(unitController)) {
                //HandleNetworkOwnedUnitDespawn(unitController);
                unitController.Despawn(0f, false, true);
            }
            if (unitController.IsOwner == false && networkUnownedUnits.Contains(unitController)) {
                //HandleNetworkUnownedUnitDespawn(unitController);
                unitController.Despawn(0f, false, true);
            }
        }

        public void CompleteCharacterRequest(GameObject characterGameObject, int spawnRequestId, bool isOwner) {
            Debug.Log($"CharacterManager.CompleteCharacterRequest({characterGameObject.name}, {spawnRequestId}, {isOwner})");

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true) {
                CompleteCharacterRequest(characterGameObject, unitSpawnRequests[spawnRequestId], isOwner);
            }
        }

        public UnitController CompleteCharacterRequest(GameObject characterGameObject, CharacterRequestData characterRequestData, bool isOwner) {
            Debug.Log($"CharacterManager.CompleteCharacterRequest({characterGameObject.name}, {characterRequestData.isServerOwned}, {isOwner})");

            UnitController unitController = ConfigureUnitController(characterRequestData, characterGameObject, isOwner);
            if (unitController == null) {
                return null;
            }
            /*
            if (unitController.IsOwner && characterRequestData.characterConfigurationRequest.unitControllerMode == UnitControllerMode.Player) {
                playerManager.SetUnitController(unitController);
            }
            */

            if (characterRequestData.requestMode == GameMode.Network) {
                // if this is being spawned over the network, the model is not spawned yet, so return and wait for it to spawn
                return null;
            }

            CompleteModelRequest(characterRequestData.spawnRequestId, unitController, isOwner, false);

            return unitController;
        }

        public void CompleteNetworkModelRequest(int spawnRequestId, UnitController unitController, GameObject unitModel, bool isOwner, bool isServerOwner) {
            Debug.Log($"CharacterManager.CompleteNetworkModelRequest({spawnRequestId}, {unitController.gameObject.name}, {isOwner}, {isServerOwner})");

            unitController.UnitModelController.SetUnitModel(unitModel);
            CompleteModelRequest(spawnRequestId, unitController, isOwner, isServerOwner);
        }

        public void CompleteModelRequest(int spawnRequestId, UnitController unitController, bool isOwner, bool isServerOwner) {
            Debug.Log($"CharacterManager.CompleteModelRequest({spawnRequestId}, {unitController.gameObject.name}, {isOwner}, {isServerOwner})");

            CharacterRequestData characterRequestData;
            if (isServerOwner && networkManagerServer.ServerModeActive == true && unitSpawnRequests.ContainsKey(spawnRequestId)) {
                characterRequestData = unitSpawnRequests[spawnRequestId];
            } else if (isOwner && unitSpawnRequests.ContainsKey(spawnRequestId)) {
                characterRequestData = unitSpawnRequests[spawnRequestId];
            } else {
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitController.UnitProfile);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Preview; // does not matter since it's unused to the CompleteModelRequest() process
                characterRequestData = new CharacterRequestData(null,
                                GameMode.Network,
                                characterConfigurationRequest);
                characterRequestData.spawnRequestId = clientSpawnRequestId;
            }

            unitController.UnitModelController.SetInitialSavedAppearance();

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true && (isOwner || isServerOwner)) {
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) unitSpawnRequests contains the key");
                characterRequestData.characterRequestor.ConfigureSpawnedCharacter(unitController, characterRequestData);
            }

            unitController.Init();

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true && (isOwner || isServerOwner)) {
                characterRequestData.characterRequestor.PostInit(unitController, characterRequestData);
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) removing character request id {characterRequestData.spawnRequestId}");
                unitSpawnRequests.Remove(spawnRequestId);
            }

        }

        public UnitController ConfigureUnitController(CharacterRequestData characterRequestData, GameObject prefabObject, bool isOwner) {
            Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name})");

            characterRequestData.isOwner = isOwner;

            UnitController unitController = null;

            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {
                    characterRequestData.unitController = unitController;
                    //Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name}) adding {unitController.gameObject.name} to modelSpawnRequests");
                    modelSpawnRequests.Add(unitController, characterRequestData);

                    // give this unit a unique name
                    //Debug.Log($"CharacterManager.ConfigureUnitController({unitProfile.ResourceName}, {prefabObject.name}) renaming gameobject from {unitController.gameObject.name}");
                    unitController.gameObject.name = characterRequestData.characterConfigurationRequest.unitProfile.ResourceName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                    unitController.Configure(systemGameManager);

                    if (characterRequestData.requestMode == GameMode.Local) {
                        Debug.Log($"adding {unitController.gameObject.name} to local owned units");
                        localUnits.Add(unitController);
                        SubscribeToLocalOwnedUnitsEvents(unitController);
                    } else {
                        if (isOwner) {
                            networkOwnedUnits.Add(unitController);
                            unitController.UnitEventController.OnDespawn += HandleNetworkOwnedUnitDespawn;
                        } else if (characterRequestData.isServerOwned) {
                            Debug.Log($"adding {unitController.gameObject.name} to server owned units");
                            serverOwnedUnits.Add(unitController);
                            unitController.UnitEventController.OnDespawn += HandleServerOwnedUnitDespawn;
                        } else {
                            networkUnownedUnits.Add(unitController);
                            unitController.UnitEventController.OnDespawn += HandleNetworkUnownedUnitDespawn;
                        }
                    }

                    if (characterRequestData.characterConfigurationRequest.unitControllerMode == UnitControllerMode.Player) {
                        if (isOwner) {
                            playerManager.SetUnitController(unitController);
                            playerManagerServer.AddActivePlayer(0, unitController);
                            playerManagerServer.MonitorPlayer(unitController);
                        } else if (networkManagerServer.ServerModeActive) {
                            playerManagerServer.MonitorPlayer(unitController);
                        }
                    }
                    unitController.SetCharacterConfiguration(characterRequestData);

                }
            }

            return unitController;
        }

        private void SubscribeToLocalOwnedUnitsEvents(UnitController unitController) {
            
            unitController.UnitEventController.OnDespawn += HandleLocalUnitDespawn;
            unitController.UnitEventController.OnAfterDie += HandleAfterDie;
        }

        public void HandleAfterDie(CharacterStats deadCharacterStats) {
            if (deadCharacterStats.UnitController.GetCurrentInteractables().Count == 0) {
                deadCharacterStats.UnitController.OutlineController.TurnOffOutline();
            }
        }

        private void HandleLocalUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleLocalUnitDespawn;
            unitController.UnitEventController.OnAfterDie -= HandleAfterDie;
            localUnits.Remove(unitController);
        }

        private void HandleNetworkOwnedUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleNetworkOwnedUnitDespawn;
            networkOwnedUnits.Remove(unitController);
        }

        private void HandleNetworkUnownedUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleNetworkUnownedUnitDespawn;
            networkUnownedUnits.Remove(unitController);
        }

        private void HandleServerOwnedUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleServerOwnedUnitDespawn;
            serverOwnedUnits.Remove(unitController);
        }

        private GameObject LocalSpawnPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.LocalSpawnPrefab({spawnPrefab.name})");

            if (spawnPrefab == null) {
                return null;
            }

            GameObject prefabObject = objectPooler.GetPooledObject(spawnPrefab, position, (forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward)), parentTransform);

            return prefabObject;
        }

        private UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.SpawnCharacterPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            GameObject prefabObject = LocalSpawnPrefab(characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.UnitPrefab, parentTransform, position, forward);
            UnitController unitController = null;
            if (characterRequestData.requestMode == GameMode.Local) {
                // this should always be true in this function because it's only called if not network mode
                unitController = CompleteCharacterRequest(prefabObject, characterRequestData, true);
            }
            return unitController;
        }

        public void AddUnitSpawnRequest(int spawnRequestId, CharacterRequestData characterRequestData) {
            Debug.Log($"CharacterManager.AddUnitSpawnRequest({spawnRequestId}, {characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            unitSpawnRequests.Add(spawnRequestId, characterRequestData);
        }
        
        public bool HasUnitSpawnRequest(int spawnRequestId) {
            return unitSpawnRequests.ContainsKey(spawnRequestId);
        }

        private GameObject SpawnModelPrefab(int spawnRequestId, GameMode spawnMode, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.SpawnModelPrefab({spawnRequestId}, {spawnMode}, {spawnPrefab.name}, {parentTransform.gameObject.name})");

            if (spawnMode == GameMode.Network) {
                if (networkManagerServer.ServerModeActive == true) {
                    return networkManagerServer.SpawnModelPrefab(spawnRequestId, spawnPrefab, parentTransform, position, forward);
                } else {
                    return networkManagerClient.SpawnModelPrefab(spawnRequestId, spawnPrefab, parentTransform, position, forward);
                }
            }
            return LocalSpawnPrefab(spawnPrefab, parentTransform, position, forward);
        }


        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public GameObject SpawnModelPrefab(UnitController unitController, UnitProfile unitProfile, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}, {parentTransform.gameObject.name})");

            if (modelSpawnRequests.ContainsKey(unitController) == false) {
                Debug.Log($"CharacterManager.SpawnModelPrefab() return null not in spawn requests");
                return null;
            }

            if (networkUnownedUnits.Contains(unitController)) {
                Debug.Log($"CharacterManager.SpawnModelPrefab() network unowned unit");
                return null;
            }

            int usedSpawnRequestId = modelSpawnRequests[unitController].spawnRequestId;
            //Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}) removing unitController from modelSpawnRequests");
            modelSpawnRequests.Remove(unitController);

            if (localUnits.Contains(unitController)) {
                Debug.Log("in local units");
                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Local, unitProfile.UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
            }

            if (networkOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            if (serverOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            return null;
        }

        public void PoolUnitController(UnitController unitController) {
            if (localUnits.Contains(unitController)) {
                objectPooler.ReturnObjectToPool(unitController.gameObject);
                return;
            }
            if (networkManagerServer.ServerModeActive == true) {
                // this is happening on the server, return the object to the pool
                networkManagerServer.ReturnObjectToPool(unitController.gameObject);
            } else {
                // this is happening on the client
                if (localUnits.Contains(unitController)) {
                    // this unit was requested in a local game, pool it
                    objectPooler.ReturnObjectToPool(unitController.gameObject);
                } else {
                    // this unit was requested in a network game, deactivate it and let it wait for the network pooler to claim it
                    unitController.gameObject.SetActive(false);
                }
            }
        }
    }

}
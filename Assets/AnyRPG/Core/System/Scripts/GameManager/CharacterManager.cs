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
        private int clientSpawnRequestIdCounter;
        //private int serverSpawnRequestId;

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

        public int GetClientSpawnRequestId() {
            int returnValue = clientSpawnRequestIdCounter;
            clientSpawnRequestIdCounter++;
            return returnValue;
        }

        /*
        public int GetServerSpawnRequestId() {
            return serverSpawnRequestId++;
        }
        */

        private void SetupUnitSpawnRequest(CharacterRequestData characterRequestData) {
            //Debug.Log($"CharacterManager.SetupUnitSpawnRequest({characterRequestData.characterConfigurationRequest.unitProfile.resourceName})");

            characterRequestData.clientSpawnRequestId = GetClientSpawnRequestId();
            characterRequestData.serverSpawnRequestId = characterRequestData.clientSpawnRequestId;
            AddUnitSpawnRequest(characterRequestData.clientSpawnRequestId, characterRequestData);
        }

        public void RequestSpawnLobbyGamePlayer(int gameId, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnLobbyGamePlayer({gameId}, {position}, {forward})");

            SetupUnitSpawnRequest(characterRequestData);

            networkManagerClient.RequestSpawnLobbyGamePlayer(gameId, characterRequestData, parentTransform, position, forward, SceneManager.GetActiveScene().name);

        }

        public void SpawnPlayer(PlayerCharacterSaveData playerCharacterSaveData, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {

            SetupUnitSpawnRequest(characterRequestData);

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.SpawnPlayer(playerCharacterSaveData.PlayerCharacterId, characterRequestData, parentTransform, SceneManager.GetActiveScene().name);
                return;
            }

            // game mode is local, spawn locally
            SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }

        // on the network
        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            //Debug.Log($"CharacterManager.SpawnUnitPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {position}, {forward}, {scene.name})");

            SetupUnitSpawnRequest(characterRequestData);

            return networkManagerServer.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        // locally
        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnUnitPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            SetupUnitSpawnRequest(characterRequestData);

            return SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }


        public void ProcessStopNetworkUnit(UnitController unitController) {
            //Debug.Log($"CharacterManager.ProcessStopClient({unitController.gameObject.name})");

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
            //Debug.Log($"CharacterManager.CompleteCharacterRequest({characterGameObject.name}, {spawnRequestId}, {isOwner})");

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true) {
                CompleteCharacterRequest(characterGameObject, unitSpawnRequests[spawnRequestId], isOwner);
            }
        }

        public UnitController CompleteCharacterRequest(GameObject characterGameObject, CharacterRequestData characterRequestData, bool isOwner) {
            //Debug.Log($"CharacterManager.CompleteCharacterRequest({characterGameObject.name}, {characterRequestData.isServerOwned}, {isOwner})");

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

            CompleteModelRequest(characterRequestData.clientSpawnRequestId, characterRequestData.serverSpawnRequestId, unitController, isOwner, false);

            return unitController;
        }

        public void CompleteNetworkModelRequest(int clientSpawnRequestId, int serverSpawnRequestId, UnitController unitController, GameObject unitModel, bool isOwner, bool isServerOwner) {
            //Debug.Log($"CharacterManager.CompleteNetworkModelRequest({clientSpawnRequestId}, {serverSpawnRequestId}, {unitController.gameObject.name}, {isOwner}, {isServerOwner})");

            unitController.UnitModelController.SetUnitModel(unitModel);
            CompleteModelRequest(clientSpawnRequestId, serverSpawnRequestId, unitController, isOwner, isServerOwner);
        }

        public void CompleteModelRequest(int clientSpawnRequestId, int serverSpawnRequestId, UnitController unitController, bool isOwner, bool isServerOwner) {
            //Debug.Log($"CharacterManager.CompleteModelRequest({clientSpawnRequestId}, {serverSpawnRequestId}, {unitController.gameObject.name}, {isOwner}, {isServerOwner})");

            CharacterRequestData characterRequestData;
            int usedSpawnRequestId = -1;
            if (networkManagerServer.ServerModeActive == true) {
                if (unitSpawnRequests.ContainsKey(serverSpawnRequestId)) {
                    //Debug.Log($"we are on the server, and there is a request, so use the server's request data");
                    characterRequestData = unitSpawnRequests[serverSpawnRequestId];
                    usedSpawnRequestId = serverSpawnRequestId;
                } else {
                    Debug.Log($"we are on the server, but there is no request, this should not happen!");
                    return;
                }
            } else {
                if (isOwner == true && unitSpawnRequests.ContainsKey(clientSpawnRequestId)) {
                    //Debug.Log($"we are on the client this request was made by, so use the client's request data");
                    characterRequestData = unitSpawnRequests[clientSpawnRequestId];
                    usedSpawnRequestId = clientSpawnRequestId;
                } else {
                    // we on the client and this request was made by the server, or another client
                    //Debug.Log($"we on the client and this request was made by the server, or another client");
                    CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitController.UnitProfile);
                    characterConfigurationRequest.unitControllerMode = UnitControllerMode.Preview; // does not matter since it's unused to the CompleteModelRequest() process
                    characterRequestData = new CharacterRequestData(null,
                                    GameMode.Network,
                                    characterConfigurationRequest);
                    characterRequestData.clientSpawnRequestId = clientSpawnRequestId;
                }
            }

            unitController.UnitModelController.SetInitialSavedAppearance();

            if (characterRequestData.characterRequestor != null) {
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) unitSpawnRequests contains the key");
                characterRequestData.characterRequestor.ConfigureSpawnedCharacter(unitController, characterRequestData);
            }

            unitController.Init();

            if (unitSpawnRequests.ContainsKey(usedSpawnRequestId) == true && characterRequestData.characterRequestor != null) {
                characterRequestData.characterRequestor.PostInit(unitController, characterRequestData);
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) removing character request id {characterRequestData.spawnRequestId}");
                unitSpawnRequests.Remove(usedSpawnRequestId);
            }

        }

        public UnitController ConfigureUnitController(CharacterRequestData characterRequestData, GameObject prefabObject, bool isOwner) {
            //Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name})");

            characterRequestData.isOwner = isOwner;

            UnitController unitController = null;

            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {
                    //Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name}) adding {unitController.gameObject.name} to modelSpawnRequests");
                    modelSpawnRequests.Add(unitController, characterRequestData);

                    // give this unit a unique name
                    //Debug.Log($"CharacterManager.ConfigureUnitController({unitProfile.ResourceName}, {prefabObject.name}) renaming gameobject from {unitController.gameObject.name}");
                    unitController.gameObject.name = characterRequestData.characterConfigurationRequest.unitProfile.ResourceName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                    unitController.Configure(systemGameManager);

                    if (characterRequestData.requestMode == GameMode.Local) {
                        //Debug.Log($"adding {unitController.gameObject.name} to local owned units");
                        localUnits.Add(unitController);
                        SubscribeToLocalOwnedUnitsEvents(unitController);
                    } else {
                        if (isOwner) {
                            networkOwnedUnits.Add(unitController);
                            unitController.UnitEventController.OnDespawn += HandleNetworkOwnedUnitDespawn;
                        } else if (characterRequestData.isServerOwned) {
                            //Debug.Log($"adding {unitController.gameObject.name} to server owned units");
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
            if (deadCharacterStats.UnitController.GetCurrentInteractables(playerManager.UnitController).Count == 0) {
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
            //Debug.Log($"CharacterManager.LocalSpawnPrefab({spawnPrefab.name})");

            if (spawnPrefab == null) {
                return null;
            }

            GameObject prefabObject = objectPooler.GetPooledObject(spawnPrefab, position, (forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward)), parentTransform);

            return prefabObject;
        }

        private UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnCharacterPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            GameObject prefabObject = LocalSpawnPrefab(characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.UnitPrefab, parentTransform, position, forward);
            UnitController unitController = null;
            if (characterRequestData.requestMode == GameMode.Local) {
                // this should always be true in this function because it's only called if not network mode
                unitController = CompleteCharacterRequest(prefabObject, characterRequestData, true);
            }
            return unitController;
        }

        public void AddUnitSpawnRequest(int spawnRequestId, CharacterRequestData characterRequestData) {
            //Debug.Log($"CharacterManager.AddUnitSpawnRequest({spawnRequestId}, {characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            unitSpawnRequests.Add(spawnRequestId, characterRequestData);
        }
        
        public bool HasUnitSpawnRequest(int spawnRequestId) {
            return unitSpawnRequests.ContainsKey(spawnRequestId);
        }

        private GameObject SpawnModelPrefab(int clientSpawnRequestId, int serverSpawnRequestId, GameMode spawnMode, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnModelPrefab({spawnRequestId}, {spawnMode}, {spawnPrefab.name}, {parentTransform.gameObject.name})");

            if (spawnMode == GameMode.Network) {
                if (networkManagerServer.ServerModeActive == true) {
                    return networkManagerServer.SpawnModelPrefab(clientSpawnRequestId, serverSpawnRequestId, spawnPrefab, parentTransform, position, forward);
                } else {
                    return networkManagerClient.SpawnModelPrefab(clientSpawnRequestId, serverSpawnRequestId, spawnPrefab, parentTransform, position, forward);
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
            //Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}, {parentTransform.gameObject.name})");

            if (modelSpawnRequests.ContainsKey(unitController) == false) {
                //Debug.Log($"CharacterManager.SpawnModelPrefab() return null not in spawn requests");
                return null;
            }

            if (networkUnownedUnits.Contains(unitController)) {
                //Debug.Log($"CharacterManager.SpawnModelPrefab() network unowned unit");
                return null;
            }

            int clientSpawnRequestId = modelSpawnRequests[unitController].clientSpawnRequestId;
            int serverSpawnRequestId = modelSpawnRequests[unitController].serverSpawnRequestId;
            //Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}) removing unitController from modelSpawnRequests");
            modelSpawnRequests.Remove(unitController);

            if (localUnits.Contains(unitController)) {
                return SpawnModelPrefab(clientSpawnRequestId, serverSpawnRequestId, GameMode.Local, unitProfile.UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
            }

            if (networkOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(clientSpawnRequestId, serverSpawnRequestId, GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            if (serverOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(clientSpawnRequestId, serverSpawnRequestId, GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
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
                // disabled because crashing.  On server, network objects are automatically despawned when level unloads
                //networkManagerServer.ReturnObjectToPool(unitController.gameObject);
                unitController.gameObject.SetActive(false);
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

        public void AddServerSpawnRequestId(int clientSpawnRequestId, int serverSpawnRequestId) {
            //Debug.Log($"CharacterManager.AddServerSpawnRequestId({clientSpawnRequestId}, {serverSpawnRequestId})");
            if (unitSpawnRequests.ContainsKey(clientSpawnRequestId) == true) {
                unitSpawnRequests[clientSpawnRequestId].serverSpawnRequestId = serverSpawnRequestId;
            } else {
                //Debug.LogError($"CharacterManager.AddServerSpawnRequestId() client spawn request id {clientSpawnRequestId} not found in unit spawn requests");
            }
        }

    }

}
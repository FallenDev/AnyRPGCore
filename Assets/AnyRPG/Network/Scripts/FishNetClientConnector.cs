using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class FishNetClientConnector : ConfiguredNetworkBehaviour {

        private FishNet.Managing.NetworkManager fishNetNetworkManager;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private CharacterManager characterManager = null;
        private NetworkManagerServer networkManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer.OnLoadCharacterList += HandleLoadCharacterList;
            networkManagerServer.OnDeletePlayerCharacter += HandleDeletePlayerCharacter;
            networkManagerServer.OnCreatePlayerCharacter += HandleCreatePlayerCharacter;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            characterManager = systemGameManager.CharacterManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            saveManager = systemGameManager.SaveManager;
        }

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.fishNetNetworkManager = networkManager;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayer(int clientSpawnRequestId, int playerCharacterId, Transform parentTransform, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {playerCharacterId})");

            // check if character is already spawned
            if (networkManagerServer.PlayerCharacterIsActive(playerCharacterId)) {
                int otherClientId = networkManagerServer.GetPlayerCharacterClientId(playerCharacterId);
                if (otherClientId == -1) {
                    Debug.LogError($"FishNetNetworkConnector.SpawnPlayer({ clientSpawnRequestId}, { playerCharacterId}) got invalid clientId for an active player character");
                    return;
                }
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(otherClientId)) {
                    // manually stop monitoring here because the actual despawn of the unit won't happen until later after we attempt to spawn the new unit
                    networkManagerServer.StopMonitoringPlayerUnit(playerCharacterId);
                    fishNetNetworkManager.ServerManager.Clients[otherClientId].Kick(FishNet.Managing.Server.KickReason.Unset);
                }
            }

            PlayerCharacterSaveData playerCharacterSaveData = networkManagerServer.GetPlayerCharacterSaveData(networkConnection.ClientId, playerCharacterId);
            if (playerCharacterSaveData == null) {
                Debug.LogWarning($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {playerCharacterId}) could not find playerCharacterId");
                return;
            }

            UnitControllerMode unitControllerMode = UnitControllerMode.Player;
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(playerCharacterSaveData.SaveData.unitProfileName);
            if (unitProfile == null) {
                return;
            }
            NetworkObject networkPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {unitProfile.UnitPrefabProps.NetworkUnitPrefab.name}");
                return;
            }
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            /*
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = playerCharacterSaveData.SaveData.PlayerLevel;
            characterConfigurationRequest.unitControllerMode = unitControllerMode;
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
            */
            //characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);
            Vector3 position = new Vector3(
                playerCharacterSaveData.SaveData.PlayerLocationX,
                playerCharacterSaveData.SaveData.PlayerLocationY,
                playerCharacterSaveData.SaveData.PlayerLocationZ);
            Vector3 forward = new Vector3(
                playerCharacterSaveData.SaveData.PlayerRotationX,
                playerCharacterSaveData.SaveData.PlayerRotationY,
                playerCharacterSaveData.SaveData.PlayerRotationZ);
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, unitProfile.UnitPrefabProps.NetworkUnitPrefab, parentTransform, position, forward);
            // update syncvars
            NetworkCharacterUnit networkCharacterUnit = nob.gameObject.GetComponent<NetworkCharacterUnit>();
            if (networkCharacterUnit != null) {
                networkCharacterUnit.unitProfileName.Value = playerCharacterSaveData.SaveData.unitProfileName;
                networkCharacterUnit.unitControllerMode.Value = unitControllerMode;
                networkCharacterUnit.unitLevel.Value = playerCharacterSaveData.SaveData.PlayerLevel;
                networkCharacterUnit.serverRequestId.Value = serverSpawnRequestId;
                networkCharacterUnit.characterAppearanceData.Value = new CharacterAppearanceData(playerCharacterSaveData.SaveData);
            }

            UnitController unitController = nob.gameObject.GetComponent<UnitController>();
            if (unitController != null) {
                networkManagerServer.MonitorPlayerUnit(networkConnection.ClientId, playerCharacterSaveData, unitController);
            }

            SpawnPrefab(nob, networkConnection);
            if (nob == null) {
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnLobbyGamePlayer(int clientSpawnRequestId, int gameId, Transform parentTransform, Vector3 position, Vector3 forward, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.SpawnLobbyGamePlayer({clientSpawnRequestId}, {gameId})");

            if (networkManagerServer.LobbyGames.ContainsKey(gameId) == false || networkManagerServer.LobbyGames[gameId].PlayerList.ContainsKey(networkConnection.ClientId) == false) {
                // game not running or client not in game
                return;
            }

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(networkManagerServer.LobbyGames[gameId].PlayerList[networkConnection.ClientId].unitProfileName);
            if (unitProfile == null) {
                return;
            }
            NetworkObject networkPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {unitProfile.UnitPrefabProps.NetworkUnitPrefab.name}");
                return;
            }
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            /*
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = playerCharacterSaveData.SaveData.PlayerLevel;
            characterConfigurationRequest.unitControllerMode = unitControllerMode;
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
            */
            //characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);
            position = new Vector3(position.x + UnityEngine.Random.Range(-2f, 2f), position.y, position.z + UnityEngine.Random.Range(-2f, 2f));
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, unitProfile.UnitPrefabProps.NetworkUnitPrefab, parentTransform, position, forward);
            // update syncvars
            NetworkCharacterUnit networkCharacterUnit = nob.gameObject.GetComponent<NetworkCharacterUnit>();
            if (networkCharacterUnit != null) {
                networkCharacterUnit.unitProfileName.Value = unitProfile.ResourceName;
                networkCharacterUnit.unitControllerMode.Value = UnitControllerMode.Player;
                networkCharacterUnit.unitLevel.Value = 1;
                networkCharacterUnit.serverRequestId.Value = serverSpawnRequestId;
            }

            PlayerCharacterSaveData playerCharacterSaveData = saveManager.CreateSaveData();
            playerCharacterSaveData.PlayerCharacterId = networkConnection.ClientId;
            playerCharacterSaveData.SaveData.playerName = networkManagerServer.LobbyGames[gameId].PlayerList[networkConnection.ClientId].userName;
            playerCharacterSaveData.SaveData.unitProfileName = unitProfile.ResourceName;
            playerCharacterSaveData.SaveData.CurrentScene = networkManagerServer.LobbyGames[gameId].sceneName;

            UnitController unitController = nob.gameObject.GetComponent<UnitController>();
            if (unitController != null) {
                networkManagerServer.MonitorPlayerUnit(networkConnection.ClientId, playerCharacterSaveData, unitController);
            }
            

            SpawnPrefab(nob, networkConnection);
            if (nob == null) {
                return;
            }
        }

        // currently unused 
        [ServerRpc(RequireOwnership = false)]
        public void SpawnCharacterUnit(int clientSpawnRequestId, string unitProfileName, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {unitProfileName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile == null) {
                return;
            }
            NetworkObject networkPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {unitProfile.UnitPrefabProps.NetworkUnitPrefab.name}");
                return;
            }
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = unitLevel;
            characterConfigurationRequest.unitControllerMode = unitControllerMode;
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
            //characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, unitProfile.UnitPrefabProps.NetworkUnitPrefab, parentTransform, position, forward);
            // update syncvars
            NetworkCharacterUnit networkCharacterUnit = nob.gameObject.GetComponent<NetworkCharacterUnit>();
            if (networkCharacterUnit != null) {
                networkCharacterUnit.unitProfileName.Value = unitProfileName;
                networkCharacterUnit.unitControllerMode.Value = unitControllerMode;
                networkCharacterUnit.unitLevel.Value = unitLevel;
                networkCharacterUnit.serverRequestId.Value = serverSpawnRequestId;
            }

            SpawnPrefab(nob, networkConnection);
            if (nob == null) {
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnModelPrefab(int clientSpawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.SpawnModelPrefab({clientSpawnRequestId}, {parentTransform.gameObject.name})");

            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            //NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, prefab, parentTransform, position, forward);
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, prefab, parentTransform, position, forward);
            SpawnPrefab(nob, networkConnection);
        }


        private NetworkObject GetSpawnablePrefab(NetworkConnection networkConnection, int clientSpawnRequestId, int serverSpawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkConnector.SpawnPrefab({clientSpawnRequestId}, {prefab.name}, {position}, {forward})");

            NetworkObject networkPrefab = prefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {prefab.name}");
                return null;
            }

            //NetworkObject nob = fishNetNetworkManager.GetPooledInstantiated(networkPrefab, true);

            NetworkObject nob = fishNetNetworkManager.GetPooledInstantiated(networkPrefab, position, Quaternion.LookRotation(forward), true);
            //NetworkObject nob = fishNetNetworkManager.GetPooledInstantiated(networkPrefab, position, Quaternion.identity, parentTransform, true);
            //nob.transform.SetPositionAndRotation(position, rotation);
            
            if (parentTransform != null) {
                NetworkObject nob2 = parentTransform.GetComponent<NetworkObject>();
                if (nob2 == null) {
                    //Debug.Log($"FishNetNetworkConnector.SpawnPrefab() could not find network object on {parentTransform.gameObject.name}");
                } else {
                    //Debug.Log($"FishNetNetworkConnector.SpawnPrefab() found a network object on {parentTransform.gameObject.name}");
                    nob.SetParent(nob2);
                }
            }
            
            //nob.transform.position = position;
            //nob.transform.forward = forward;

            SpawnedNetworkObject spawnedNetworkObject = nob.gameObject.GetComponent<SpawnedNetworkObject>();
            if (spawnedNetworkObject != null) {
                //Debug.Log($"FishNetNetworkConnector.SpawnPrefab({clientSpawnRequestId}, {prefab.name}) setting spawnRequestId on gameobject");
                spawnedNetworkObject.clientSpawnRequestId.Value = clientSpawnRequestId;
                spawnedNetworkObject.serverRequestId.Value = serverSpawnRequestId;
            }

            return nob;
        }

        private void SpawnPrefab(NetworkObject nob, NetworkConnection networkConnection) {
            //Debug.Log($"FishNetNetworkController.SpawnPlayer() Spawning player at {position}");
            fishNetNetworkManager.ServerManager.Spawn(nob, networkConnection);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LoadSceneServer(NetworkConnection networkConnection, string sceneName) {
            //Debug.Log($"FishNetNetworkConnector.LoadSceneServer({networkConnection.ClientId}, {sceneName})");

            SceneLoadData sceneLoadData = new SceneLoadData(sceneName);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
            //sceneLoadData.PreferredActiveScene = sceneLoadData.SceneLookupDatas[0];
            sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneName));
            fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.CreatePlayerCharacter(AnyRPGSaveData)");

            networkManagerServer.CreatePlayerCharacter(networkConnection.ClientId, anyRPGSaveData);
        }

        public void HandleCreatePlayerCharacter(int clientId) {
            Debug.Log($"FishNetNetworkConnector.HandleCreatePlayerCharacter({clientId})");

            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetNetworkConnector.HandleCreatePlayerCharacter() could not find client id {clientId}");
                return;
            }

            //LoadCharacterList(networkManager.ServerManager.Clients[clientId]);
            networkManagerServer.LoadCharacterList(clientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void DeletePlayerCharacter(int playerCharacterId, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.DeletePlayerCharacter({playerCharacterId})");

            networkManagerServer.DeletePlayerCharacter(networkConnection.ClientId, playerCharacterId);

            // now that character is deleted, just load the character list
            //LoadCharacterList(networkConnection);
        }

        public void HandleDeletePlayerCharacter(int clientId) {
            Debug.Log($"FishNetNetworkConnector.HandleDeletePlayerCharacter({clientId})");

            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetNetworkConnector.HandleDeletePlayerCharacter() could not find client id {clientId}");
                return;
            }

            //LoadCharacterList(networkManager.ServerManager.Clients[clientId]);
            networkManagerServer.LoadCharacterList(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ToggleLobbyGameReadyStatus(int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.ToggleLobbyGameReadyStatus(gameId, networkConnection.ClientId);
        }


        /*
        [TargetRpc]
        public void LoadCharacterList(NetworkConnection networkConnection, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            Debug.Log($"FishNetNetworkConnector.SetCharacterList({playerCharacterSaveDataList.Count})");

            systemGameManager.LoadGameManager.SetCharacterList(playerCharacterSaveDataList);
        }
        */

        [ServerRpc(RequireOwnership = false)]
        public void LoadCharacterList(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetNetworkConnector.LoadCharacterList()");

            networkManagerServer.LoadCharacterList(networkConnection.ClientId);
            //List<PlayerCharacterSaveData> playerCharacterSaveDataList = networkManagerServer.LoadCharacterList(networkConnection.ClientId);

            //Debug.Log($"FishNetNetworkConnector.LoadCharacterList() list size: {playerCharacterSaveDataList.Count}");
            //SetCharacterList(networkConnection, playerCharacterSaveDataList);
        }

        public void HandleLoadCharacterList(int clientId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            //Debug.Log($"FishNetNetworkConnector.HandleLoadCharacterList({clientId})");

            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                foreach (int client in fishNetNetworkManager.ServerManager.Clients.Keys) {
                    Debug.Log($"FishNetNetworkConnector.LoadCharacterList() found client id {client}");
                }
                Debug.Log($"FishNetNetworkConnector.LoadCharacterList() could not find client id {clientId}");
                return;
            }

            SetCharacterList(fishNetNetworkManager.ServerManager.Clients[clientId], playerCharacterSaveDataList);
        }

        [TargetRpc]
        public void SetCharacterList(NetworkConnection networkConnection, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            //Debug.Log($"FishNetNetworkConnector.SetCharacterList({playerCharacterSaveDataList.Count})");

            systemGameManager.LoadGameManager.SetCharacterList(playerCharacterSaveDataList);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"FishNetNetworkConnector.OnStartClient() ClientId: {networkManager.ClientManager.Connection.ClientId}");

            //systemGameManager.NetworkManager.ProcessLoginSuccess();
            systemGameManager.UIManager.ProcessLoginSuccess();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLobbyGameList(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestLobbyGameList(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLobbyPlayerList(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestLobbyPlayerList(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendLobbyChatMessage(string messageText, NetworkConnection networkConnection = null) {
            networkManagerServer.SendLobbyChatMessage(messageText, networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendLobbyGameChatMessage(string messageText, int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.SendLobbyGameChatMessage(messageText, networkConnection.ClientId, gameId);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SendSceneChatMessage(string messageText, NetworkConnection networkConnection = null) {
            networkManagerServer.SendSceneChatMessage(messageText, networkConnection.ClientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void ChooseLobbyGameCharacter(string unitProfileName, int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.ChooseLobbyGameCharacter(gameId, networkConnection.ClientId, unitProfileName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.StartLobbyGame(gameId, networkConnection.ClientId);
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            //Debug.Log($"FishNetNetworkConnector.OnStartNetwork()");

            FishNetNetworkController fishNetNetworkController = GameObject.FindAnyObjectByType<FishNetNetworkController>();
            fishNetNetworkController.RegisterConnector(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreateLobbyGame(string sceneName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetNetworkConnector.CreateLobbyGame()");

            networkManagerServer.CreateLobbyGame(sceneName, networkConnection.ClientId);
        }


        [ObserversRpc]
        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            networkManagerClient.AdvertiseCreateLobbyGame(lobbyGame);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CancelLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetNetworkConnector.CancelLobbyGame()");

            networkManagerServer.CancelLobbyGame(networkConnection.ClientId, gameId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetNetworkConnector.JoinLobbyGame()");

            networkManagerServer.JoinLobbyGame(gameId, networkConnection.ClientId);
        }

        [ObserversRpc]
        public void AdvertiseLobbyLogin(int clientId, string userName) {
            networkManagerClient.AdvertiseLobbyLogin(clientId, userName);
        }

        public void SendLobbyGameList(int clientId, List<LobbyGame> lobbyGames) {
            SetLobbyGameList(fishNetNetworkManager.ServerManager.Clients[clientId], lobbyGames);
        }

        [TargetRpc]
        public void SetLobbyGameList(NetworkConnection networkConnection, List<LobbyGame> lobbyGames) {
            networkManagerClient.SetLobbyGameList(lobbyGames);
        }

        public void SendLobbyPlayerList(int clientId, Dictionary<int, string> lobbyPlayers) {
            SetLobbyPlayerList(fishNetNetworkManager.ServerManager.Clients[clientId], lobbyPlayers);
        }

        [TargetRpc]
        public void SetLobbyPlayerList(NetworkConnection networkConnection, Dictionary<int, string> lobbyPlayers) {
            networkManagerClient.SetLobbyPlayerList(lobbyPlayers);
        }

        [ObserversRpc]
        public void AdvertiseStartLobbyGame(int gameId, string sceneName) {
            networkManagerClient.AdvertiseStartLobbyGame(gameId, sceneName);
        }

        [ObserversRpc]
        public void AdvertiseChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName) {
            networkManagerClient.AdvertiseChooseLobbyGameCharacter(gameId, clientId, unitProfileName);
        }

        [ObserversRpc]
        public void AdvertiseLobbyLogout(int clientId) {
            networkManagerClient.AdvertiseLobbyLogout(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LeaveLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetNetworkConnector.LeaveLobbyGame()");

            networkManagerServer.LeaveLobbyGame(gameId, networkConnection.ClientId);
        }

        [ObserversRpc]
        public void AdvertiseCancelLobbyGame(int gameId) {
            networkManagerClient.AdvertiseCancelLobbyGame(gameId);
        }

        [ObserversRpc]
        public void AdvertiseClientJoinLobbyGame(int gameId, int clientId, string userName) {
            networkManagerClient.AdvertiseClientJoinLobbyGame(gameId, clientId, userName);
        }

        [ObserversRpc]
        public void AdvertiseClientLeaveLobbyGame(int gameId, int clientId) {
            networkManagerClient.AdvertiseClientLeaveLobbyGame(gameId, clientId);
        }

        [ObserversRpc]
        public void AdvertiseSendLobbyChatMessage(string messageText) {
            networkManagerClient.AdvertiseSendLobbyChatMessage(messageText);
        }

        [ObserversRpc]
        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            networkManagerClient.AdvertiseSendLobbyGameChatMessage(messageText, gameId);
        }

        [ObserversRpc]
        public void AdvertiseSendSceneChatMessage(string messageText, int clientId) {
            networkManagerClient.AdvertiseSendSceneChatMessage(messageText, clientId);
        }

        [ObserversRpc]
        public void AdvertiseSetLobbyGameReadyStatus(int gameId, int clientId, bool ready) {
            networkManagerClient.AdvertiseSetLobbyGameReadyStatus(gameId, clientId, ready);
        }

        public void AdvertiseLoadSceneServer(string sceneName, int clientId) {
            AdvertiseLoadSceneClient(fishNetNetworkManager.ServerManager.Clients[clientId], sceneName);
        }

        [TargetRpc]
        public void AdvertiseLoadSceneClient(NetworkConnection networkConnection, string sceneName) {
            networkManagerClient.AdvertiseLoadSceneClient(sceneName);
        }

        public void ReturnObjectToPool(GameObject returnedObject) {
            fishNetNetworkManager.ServerManager.Despawn(returnedObject);
        }

        public void AdvertiseInteractWithQuestGiver(NetworkInteractable networkInteractable, int optionIndex, int clientId) {
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId)) {
                AdvertiseInteractWithQuestGiverClient(fishNetNetworkManager.ServerManager.Clients[clientId], networkInteractable, optionIndex);
            }
        }

        [TargetRpc]
        public void AdvertiseInteractWithQuestGiverClient(NetworkConnection networkConnection, NetworkInteractable networkInteractable, int optionIndex) {

            networkManagerClient.AdvertiseInteractWithQuestGiver(networkInteractable.Interactable, optionIndex);
        }

        public void InteractWithOptionClient(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex) {
            NetworkCharacterUnit networkCharacterUnit = null;
            if (sourceUnitController != null) {
                networkCharacterUnit = sourceUnitController.GetComponent<NetworkCharacterUnit>();
            }
            NetworkInteractable networkInteractable = null;
            if (targetInteractable != null) {
                networkInteractable = targetInteractable.GetComponent<NetworkInteractable>();
            }
            InteractWithOptionServer(networkCharacterUnit, networkInteractable, componentIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteractWithOptionServer(NetworkCharacterUnit sourceNetworkCharacterUnit, NetworkInteractable targetNetworkInteractable, int componentIndex) {
            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit != null) {
                sourceUnitController = sourceNetworkCharacterUnit.UnitController;
            }
            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.InteractWithOption(sourceUnitController, interactable, componentIndex);
        }

        public void AdvertiseAddSpawnRequestServer(int clientId, LoadSceneRequest loadSceneRequest) {
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId)) {
                AdvertiseAddSpawnRequestClient(fishNetNetworkManager.ServerManager.Clients[clientId], loadSceneRequest);
            }
        }

        [TargetRpc]
        public void AdvertiseAddSpawnRequestClient(NetworkConnection networkConnection, LoadSceneRequest loadSceneRequest) {
            networkManagerClient.AdvertiseAddSpawnRequest(loadSceneRequest);
        }

        /*
        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"FishNetNetworkConnector.OnStartServer()");

            // on server gameMode should always bet set to network
            //Debug.Log($"FishNetNetworkConnector.OnStartServer(): setting gameMode to network");
            systemGameManager.SetGameMode(GameMode.Network);
            networkManagerServer.ActivateServerMode();
        }

        public override void OnStopServer() {
            base.OnStopServer();
            Debug.Log($"FishNetNetworkConnector.OnStopServer()");

            systemGameManager.SetGameMode(GameMode.Local);
            networkManagerServer.DeactivateServerMode();
        }
        */
    }
}

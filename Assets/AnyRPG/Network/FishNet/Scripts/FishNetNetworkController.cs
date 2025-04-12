using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class FishNetNetworkController : NetworkController {

        private FishNet.Managing.NetworkManager fishNetNetworkManager;
        private FishNetClientConnector clientConnector;
        
        [SerializeField]
        private GameObject networkConnectorSpawnPrefab = null;
        private GameObject networkConnectorSpawnReference = null;

        /// <summary>
        /// Current state of client socket.
        /// </summary>
        private LocalConnectionState clientState = LocalConnectionState.Stopped;

        /// <summary>
        /// Current state of server socket.
        /// </summary>
        private LocalConnectionState serverState = LocalConnectionState.Stopped;

        // game manager references
        private LevelManager levelManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("FishNetNetworkController.Configure()");

            base.Configure(systemGameManager);
            fishNetNetworkManager = InstanceFinder.NetworkManager;
            if (fishNetNetworkManager != null) {
                //Debug.Log("FishNetNetworkController.Configure() Found FishNet NetworkManager");

                fishNetNetworkManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
                //fishNetNetworkManager.SceneManager.OnClientLoadedStartScenes += HandleClientLoadedStartScenes;
                fishNetNetworkManager.ServerManager.OnServerConnectionState += HandleServerConnectionState;
                
                // stuff that was previously done only on active connection
                fishNetNetworkManager.SceneManager.OnActiveSceneSet += HandleActiveSceneSet;
                fishNetNetworkManager.SceneManager.OnUnloadStart += HandleUnloadStart;

            } else {
                Debug.Log("FishNetNetworkController.Configure() Could not find FishNet NetworkManager");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        private void HandleServerConnectionState(ServerConnectionStateArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleServerConnectionState() {obj.ConnectionState.ToString()}");

            serverState = obj.ConnectionState;
            if (serverState == LocalConnectionState.Started) {
                //Debug.Log("FishNetNetworkController.HandleServerConnectionState() Server connection started.  Activating Server Mode.");
                systemGameManager.SetGameMode(GameMode.Network);
                networkManagerServer.ActivateServerMode();
                SubscribeToServerEvents();
                InstantiateNetworkConnector();
            } else if (serverState == LocalConnectionState.Stopping) {
                Debug.Log("FishNetNetworkController.HandleServerConnectionState() Stopping");
            } else if (serverState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.HandleServerConnectionState() Stopped");
                systemGameManager.SetGameMode(GameMode.Local);
                networkManagerServer.DeactivateServerMode();
                UnsubscribeFromServerEvents();
            }
        }

        public void SubscribeToServerEvents() {
            //Debug.Log($"FishNetNetworkController.SubscribeToServerEvents()");

            fishNetNetworkManager.SceneManager.OnLoadEnd += HandleSceneLoadEndServer;
            fishNetNetworkManager.ServerManager.OnClientKick += HandleClientKick;
            fishNetNetworkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
            fishNetNetworkManager.SceneManager.OnUnloadEnd += HandleUnloadEndServer;
        }

        public void UnsubscribeFromServerEvents() {
            Debug.Log($"FishNetNetworkController.UnsubscribeFromServerEvents()");

            fishNetNetworkManager.SceneManager.OnLoadEnd -= HandleSceneLoadEndServer;
            fishNetNetworkManager.ServerManager.OnClientKick -= HandleClientKick;
            fishNetNetworkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
            fishNetNetworkManager.SceneManager.OnUnloadEnd -= HandleUnloadEndServer;
        }

        private void HandleClientConnectionState(ClientConnectionStateArgs obj) {
            //Debug.Log($"HandleClientConnectionState() {obj.ConnectionState.ToString()}");

            clientState = obj.ConnectionState;
            if (clientState == LocalConnectionState.Starting) {
                SubscribeToClientEvents();
            } else if (clientState == LocalConnectionState.Started) {
                //Debug.Log("FishNetNetworkController.OnClientConnectionState() Connection Successful. Setting mode to network");
                systemGameManager.SetGameMode(GameMode.Network);
            } else if (clientState == LocalConnectionState.Stopping) {
                Debug.Log("FishNetNetworkController.OnClientConnectionState() Disconnected from server. Stopping");
            } else if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.OnClientConnectionState() Disconnected from server. Setting mode to local");
                systemGameManager.NetworkManagerClient.ProcessStopConnection();
                UnsubscribeFromClientEvents();
            }
        }

        public void SubscribeToClientEvents() {
            fishNetNetworkManager.ClientManager.OnAuthenticated += HandleClientAuthenticated;
            fishNetNetworkManager.SceneManager.OnLoadStart += HandleLoadStartClient;
            fishNetNetworkManager.SceneManager.OnLoadPercentChange += HandleLoadPercentChangeClient;
            fishNetNetworkManager.SceneManager.OnLoadEnd += HandleLoadEndClient;
        }

        public void UnsubscribeFromClientEvents() {
            fishNetNetworkManager.ClientManager.OnAuthenticated -= HandleClientAuthenticated;
            fishNetNetworkManager.SceneManager.OnLoadStart -= HandleLoadStartClient;
            fishNetNetworkManager.SceneManager.OnLoadPercentChange -= HandleLoadPercentChangeClient;
            fishNetNetworkManager.SceneManager.OnLoadEnd -= HandleLoadEndClient;
        }

        #region client functions

        public override bool Login(string username, string password, string server) {
            //Debug.Log($"FishNetNetworkController.Login({username}, {password})");

            if (fishNetNetworkManager == null) {
                return false;
            }

            bool customPort = false;
            int port = 0;
            string serverAddress = server;
            if (string.IsNullOrEmpty(server)) {
                server = "localhost";
            } else {
                if (server.Contains(':')) {
                    string[] splitList = server.Split(":");
                    serverAddress = splitList[0];
                    if (int.TryParse(splitList[1], out port)) {
                        customPort = true;
                    }
                }
            }

            if (clientState != LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already connected to the server!");
                return false;
            }

            bool connectionResult;
            if (customPort) {
                connectionResult = fishNetNetworkManager.ClientManager.StartConnection(server, (ushort)port);
            } else {
                connectionResult = fishNetNetworkManager.ClientManager.StartConnection(server);
            }
            
            //Debug.Log($"FishNetNetworkController.Login() Result of connection attempt: {connectionResult}");

            return connectionResult;
        }

        public override void Logout() {
            if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already disconnected from the server!");
                return;
            }

            bool connectionResult = fishNetNetworkManager.ClientManager.StopConnection();
            Debug.Log($"FishNetNetworkController.Login() Result of disconnection attempt: {connectionResult}");
        }
       

        private void HandleRemoteConnectionState(NetworkConnection networkConnection, RemoteConnectionStateArgs args) {
            //Debug.Log($"FishNetNetworkController.HandleRemoteConnectionState({args.ConnectionState.ToString()})");

            if (args.ConnectionState == RemoteConnectionState.Stopped) {
                networkManagerServer.ProcessClientDisconnect(networkConnection.ClientId);
            }
        }

        private void HandleClientAuthenticated() {
            //Debug.Log($"FishNetNetworkController.HandleClientAuthenticated({fishNetNetworkManager.ClientManager.Connection.ClientId})");

            networkManagerClient.SetClientId(fishNetNetworkManager.ClientManager.Connection.ClientId);
        }

        private void HandleClientKick(NetworkConnection arg1, int arg2, KickReason kickReason) {
            Debug.Log($"FishNetNetworkController.HandleClientKick({kickReason.ToString()})");
        }

        /*
        private void HandleClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer) {
            //Debug.Log("FishNetNetworkController.HandleClientLoadedStartScenes()");
            //networkManager.SceneManager.AddConnectionToScene(networkConnection, UnityEngine.SceneManagement.SceneManager.GetSceneByName("DontDestroyOnLoad"));
            //networkManager.SceneManager.AddConnectionToScene(networkConnection, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        */

        public void HandleLoadStartClient(SceneLoadStartEventArgs args) {
            networkManagerClient.HandleSceneLoadStart(args.QueueData.SceneLoadData.GetFirstLookupScene().name);
        }

        public void HandleLoadPercentChangeClient(SceneLoadPercentEventArgs args) {
            networkManagerClient.HandleSceneLoadPercentageChange(args.Percent);
        }

        private void HandleLoadEndClient(SceneLoadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadEnd() AsServer: {obj.QueueData.AsServer}");

            if (systemGameManager.GameMode == GameMode.Network) {
                levelManager.ProcessLevelLoad();
            }

        }

        private void HandleActiveSceneSet(bool userInitiated) {
            //Debug.Log($"FishNetNetworkController.HandleActiveSceneSet({userInitiated}) current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            //if (systemGameManager.GameMode == GameMode.Network) {
            //    levelManager.ProcessLevelLoad();
            //}
        }

        private void HandleUnloadEndServer(SceneUnloadEndEventArgs obj) {
            Debug.Log($"FishNetNetworkController.HandleUnloadEnd()");

            foreach (UnloadedScene scene in obj.UnloadedScenesV2) {
                Debug.Log($"FishNetNetworkController.HandleUnloadEnd() {scene.Name}");
                networkManagerServer.HandleSceneUnloadEnd(scene.Name);
            }
        }

        private void HandleUnloadStart(SceneUnloadStartEventArgs obj) {
            Debug.Log($"FishNetNetworkController.HandleUnloadStart({obj.QueueData.SceneUnloadData.SceneLookupDatas[0].Name})");

            //foreach (SceneLookupData sceneLookupData in obj.QueueData.SceneUnloadData.SceneLookupDatas) {
            //    Debug.Log($"FishNetNetworkController.HandleUnloadStart() {sceneLookupData.Name}");
            //}
        }


        public void RegisterConnector(FishNetClientConnector clientConnector) {
            this.clientConnector = clientConnector;
            if (clientConnector != null) {
                clientConnector.Configure(systemGameManager);
                clientConnector.SetNetworkManager(fishNetNetworkManager);
            }
        }

        
        /// <summary>
        /// Instantiate the network connector on the server that will spawn on every client, allowing them to issue requests to the server
        /// </summary>
        public void InstantiateNetworkConnector() {
            //Debug.Log("FishNetNetworkController.InstantiateNetworkConnector()");

            networkConnectorSpawnReference = GameObject.Instantiate(networkConnectorSpawnPrefab);
            clientConnector = networkConnectorSpawnReference.gameObject.GetComponentInChildren<FishNetClientConnector>();
            if (clientConnector != null) {
                clientConnector.Configure(systemGameManager);
                clientConnector.SetNetworkManager(fishNetNetworkManager);
            }

            NetworkObject networkPrefab = networkConnectorSpawnPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {networkConnectorSpawnPrefab.name}");
                return;
            }

            NetworkObject nob = fishNetNetworkManager.GetPooledInstantiated(networkPrefab, true);
            fishNetNetworkManager.ServerManager.Spawn(nob);

        }


        public override void SpawnPlayer(int playerCharacterId, CharacterRequestData characterRequestData, Transform parentTransform) {
            Debug.Log($"FishNetNetworkController.SpawnPlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            clientConnector.SpawnPlayer(characterRequestData.spawnRequestId, playerCharacterId, parentTransform);
            //return null;
        }

        public override void SpawnLobbyGamePlayer(int gameId, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkController.SpawnLobbyGamePlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            clientConnector.SpawnLobbyGamePlayer(characterRequestData.spawnRequestId, gameId, parentTransform, position, forward);
            //return null;
        }


        public override GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkController.SpawnModelPrefab({spawnRequestId}, {parentTransform.gameObject.name})");

            clientConnector.SpawnModelPrefab(spawnRequestId, prefab, parentTransform, position, forward);
            return null;
        }

        public override void LoadScene(string sceneName) {
            //Debug.Log($"FishNetNetworkController.LoadScene({sceneName})");

            clientConnector.LoadSceneServer(fishNetNetworkManager.ClientManager.Connection, sceneName);
        }

        public override bool CanSpawnCharacterOverNetwork() {
            //Debug.Log($"FishNetNetworkController.CanSpawnCharacterOverNetwork() isClient: {networkManager.IsClient}");
            return fishNetNetworkManager.IsClientStarted;
        }

        public override bool OwnPlayer(UnitController unitController) {
            NetworkBehaviour networkBehaviour = unitController.gameObject.GetComponent<NetworkBehaviour>();
            if (networkBehaviour != null && networkBehaviour.IsOwner == true) {
                return true;
            }
            return false;
        }

        public override void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"FishNetNetworkController.CreatePlayerCharacter(AnyRPGSaveData)");

            clientConnector.CreatePlayerCharacter(anyRPGSaveData);
        }

        public override void DeletePlayerCharacter(int playerCharacterId) {
            Debug.Log($"FishNetNetworkController.DeletePlayerCharacter({playerCharacterId})");

            clientConnector.DeletePlayerCharacter(playerCharacterId);
        }

        public override void LoadCharacterList() {
            //Debug.Log($"FishNetNetworkController.LoadCharacterList()");

            if (clientConnector == null) {
                Debug.LogWarning($"FishNetNetworkController.LoadCharacterList(): networkConnector is null");
                return;
            }
            clientConnector.LoadCharacterList();
        }

        public override void CreateLobbyGame(string sceneName) {
            clientConnector.CreateLobbyGame(sceneName);
        }

        public override void CancelLobbyGame(int gameId) {
            clientConnector.CancelLobbyGame(gameId);
        }

        public override void JoinLobbyGame(int gameId) {
            clientConnector.JoinLobbyGame(gameId);
        }

        public override void LeaveLobbyGame(int gameId) {
            clientConnector.LeaveLobbyGame(gameId);
        }

        public override int GetClientId() {
            return fishNetNetworkManager.ClientManager.Connection.ClientId;
        }

        public override void SendLobbyChatMessage(string messageText) {
            clientConnector.SendLobbyChatMessage(messageText);
        }

        public override void SendLobbyGameChatMessage(string messageText, int gameId) {
            clientConnector.SendLobbyGameChatMessage(messageText, gameId);
        }

        public override void SendSceneChatMessage(string messageText) {
            clientConnector.SendSceneChatMessage(messageText);
        }

        public override void RequestLobbyGameList() {
            clientConnector.RequestLobbyGameList();
        }

        public override void RequestLobbyPlayerList() {
            clientConnector.RequestLobbyPlayerList();
        }

        public override void ChooseLobbyGameCharacter(string unitProfileName, int gameId) {
            clientConnector.ChooseLobbyGameCharacter(unitProfileName, gameId);
        }

        public override void StartLobbyGame(int gameId) {
            clientConnector.StartLobbyGame(gameId);
        }

        public override void ToggleLobbyGameReadyStatus(int gameId) {
            clientConnector.ToggleLobbyGameReadyStatus(gameId);
        }

        public override void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            clientConnector.InteractWithOptionClient(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
        }

        public override void SetPlayerCharacterClass(string className) {
            clientConnector.SetPlayerCharacterClass(className);
        }

        public override void LearnSkill(string skillName) {
            clientConnector.LearnSkill(skillName);
        }

        public override void AcceptQuest(string questName) {
            clientConnector.AcceptQuest(questName);
        }

        public override void CompleteQuest(string questName, QuestRewardChoices questRewardChoices) {
            clientConnector.CompleteQuest(questName, questRewardChoices);
        }

        public override void SellVendorItem(Interactable interactable, int componentIndex, int itemInstanceId) {
            clientConnector.SellVendorItemClient(interactable, componentIndex, itemInstanceId);
        }

        public override void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName) {
            clientConnector.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName);
        }

        public override void TakeAllLoot() {
            clientConnector.TakeAllLoot();
        }

        public override void RequestTakeLoot(int lootDropId) {
            clientConnector.RequestTakeLoot(lootDropId);
        }

        #endregion

        #region server functions

        private void HandleSceneLoadEndServer(SceneLoadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadEndServer()");

            if (obj.SkippedSceneNames.Length > 0 && obj.LoadedScenes.Length == 0) {
                return;
            }
            foreach (Scene scene in obj.LoadedScenes) {
                //Debug.Log($"FishNetNetworkController.HandleLoadEnd() {scene.name}");
                networkManagerServer.HandleSceneLoadEnd(scene);
            }
            //Debug.Log($"FishNetNetworkController.HandleLoadEnd() skipped: {string.Join(',', obj.SkippedSceneNames.ToList())}");

        }

        public override void StartServer() {
            //Debug.Log($"FishNetNetworkController.StartServer()");

            fishNetNetworkManager.ServerManager.StartConnection();
        }

        public override void StopServer() {
            fishNetNetworkManager.ServerManager.StopConnection(true);
        }

        public override void KickPlayer(int clientId) {
            fishNetNetworkManager.ServerManager.Kick(clientId, KickReason.Unset);
        }

        public override string GetClientIPAddress(int clientId) {
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return "ClientId not found";
            }

            return fishNetNetworkManager.ServerManager.Clients[clientId].GetAddress();
        }

        public override void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            clientConnector.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public override void AdvertiseCancelLobbyGame(int gameId) {
            clientConnector.AdvertiseCancelLobbyGame(gameId);
        }

        public override void AdvertiseClientJoinLobbyGame(int gameId, int clientId, string userName) {
            clientConnector.AdvertiseClientJoinLobbyGame(gameId, clientId, userName);
        }

        public override void AdvertiseClientLeaveLobbyGame(int gameId, int clientId) {
            clientConnector.AdvertiseClientLeaveLobbyGame(gameId, clientId);
        }

        public override void AdvertiseSendLobbyChatMessage(string messageText) {
            clientConnector.AdvertiseSendLobbyChatMessage(messageText);
        }

        public override void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            clientConnector.AdvertiseSendLobbyGameChatMessage(messageText, gameId);
        }

        public override void AdvertiseSendSceneChatMessage(string messageText, int clientId) {
            clientConnector.AdvertiseSendSceneChatMessage(messageText, clientId);
        }

        public override void AdvertiseLobbyLogin(int clientId, string userName) {
            clientConnector.AdvertiseLobbyLogin(clientId, userName);
        }

        public override void AdvertiseLobbyLogout(int clientId) {
            clientConnector.AdvertiseLobbyLogout(clientId);
        }

        public override void SetLobbyGameList(int clientId, List<LobbyGame> lobbyGames) {
            clientConnector.SendLobbyGameList(clientId, lobbyGames);
        }

        public override void SetLobbyPlayerList(int clientId, Dictionary<int, string> lobbyPlayers) {
            clientConnector.SendLobbyPlayerList(clientId, lobbyPlayers);
        }

        public override void AdvertiseChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName) {
            clientConnector.AdvertiseChooseLobbyGameCharacter(gameId, clientId, unitProfileName);
        }

        public override void AdvertiseStartLobbyGame(int gameId, string sceneName) {
            clientConnector.AdvertiseStartLobbyGame(gameId, sceneName);
        }

        public override void AdvertiseSetLobbyGameReadyStatus(int gameId, int clientId, bool ready) {
            clientConnector.AdvertiseSetLobbyGameReadyStatus(gameId, clientId, ready);
        }

        public override int GetServerPort() {
            return fishNetNetworkManager.TransportManager.Transport.GetPort();
        }

        public override void AdvertiseLoadScene(string sceneName, int clientId) {
            clientConnector.AdvertiseLoadSceneServer(sceneName, clientId);
        }

        public override void ReturnObjectToPool(GameObject returnedObject) {
            clientConnector.ReturnObjectToPool(returnedObject);
        }

        /*
        public override void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex, int clientId) {

            NetworkInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<NetworkInteractable>();
            }
            clientConnector.AdvertiseInteractWithQuestGiver(networkInteractable, optionIndex, clientId);
        }
        */

        public override void AdvertiseAddSpawnRequest(int clientId, LoadSceneRequest loadSceneRequest) {
            clientConnector.AdvertiseAddSpawnRequestServer(clientId, loadSceneRequest);
        }

        /*
        public override void AdvertiseInteractWithClassChangeComponentServer(int clientId, Interactable interactable, int optionIndex) {
            clientConnector.AdvertiseInteractWithClassChangeComponentServer(clientId, interactable, optionIndex);
        }

        public override void AdvertiseInteractWithSkillTrainerComponentServer(int clientId, Interactable interactable, int optionIndex) {
            clientConnector.AdvertiseInteractWithSkillTrainerComponentServer(clientId, interactable, optionIndex);
        }

        public override void AdvertiseInteractWithAnimatedObjectComponentServer(int clientId, Interactable interactable, int optionIndex) {
            clientConnector.AdvertiseInteractWithAnimatedObjectComponentServer(clientId, interactable, optionIndex);
        }
        */

        public override UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return clientConnector.SpawnCharacterUnit(characterRequestData, parentTransform, position, forward, scene);
        }

        public override GameObject SpawnModelPrefabServer(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkController.SpawnModelPrefabServer({spawnRequestId}, {parentTransform.gameObject.name})");

            clientConnector.SpawnModelPrefabServer(spawnRequestId, prefab, parentTransform, position, forward);
            return null;
        }

        public override void AdvertiseMessageFeedMessage(int clientId, string message) {
            clientConnector.AdvertiseMessageFeedMessage(clientId, message);
        }

        public override void AdvertiseSystemMessage(int clientId, string message) {
            clientConnector.AdvertiseSystemMessage(clientId, message);
        }

        public override void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, int clientId, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            clientConnector.AdvertiseAddToBuyBackCollection(sourceUnitController, clientId, interactable, componentIndex, newInstantiatedItem);
        }

        public override void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            //Debug.Log($"FishNetNetworkController.AdvertiseSellItemToPlayer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            
            clientConnector.AdvertiseSellItemToPlayer(sourceUnitController, interactable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        public override void AddAvailableDroppedLoot(int clientId, List<LootDrop> items) {
            //Debug.Log($"FishNetNetworkController.AddAvailableDroppedLoot({clientId}, {items.Count})");

            clientConnector.AddAvailableDroppedLoot(clientId, items);
        }

        public override void AddLootDrop(int clientId, int lootDropId, int itemId) {
            clientConnector.AddDroppedLoot(clientId, lootDropId, itemId);
        }

        public override void AdvertiseTakeLoot(int clientId, int lootDropId) {
            clientConnector.AdvertiseTakeLoot(clientId, lootDropId);
        }

        #endregion

    }
}

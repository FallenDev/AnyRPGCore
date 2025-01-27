using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class NetworkManagerServer : ConfiguredMonoBehaviour {

        public event Action<int, bool, bool> OnAuthenticationResult = delegate { };
        public event Action<int, List<PlayerCharacterSaveData>> OnLoadCharacterList = delegate { };
        public event Action<int> OnDeletePlayerCharacter = delegate { };
        public event Action<int> OnCreatePlayerCharacter = delegate { };
        public event Action OnStartServer = delegate { };
        public event Action OnStopServer = delegate { };
        public event Action<int> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };

        // jwt for each client so the server can make API calls to the api server on their behalf
        //private Dictionary<int, string> clientTokens = new Dictionary<int, string>();

        // cached list of player character save data from client lookups used for loading games
        private Dictionary<int, Dictionary<int, PlayerCharacterSaveData>> playerCharacterDataDict = new Dictionary<int, Dictionary<int, PlayerCharacterSaveData>>();

        // playerCharacterId
        private Dictionary<int, PlayerCharacterMonitor> activePlayerCharacters = new Dictionary<int, PlayerCharacterMonitor>();
        private Dictionary<int, PlayerCharacterMonitor> activePlayerCharactersByClient = new Dictionary<int, PlayerCharacterMonitor>();

        // mapping of client Ids to account information
        private Dictionary<int, LoggedInAccount> loggedInAccounts = new Dictionary<int, LoggedInAccount>();
        private Dictionary<int, string> loginRequests = new Dictionary<int, string>();

        // list of lobby games
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();
        private int lobbyGameCounter = 0;
        private int maxLobbyChatTextSize = 64000;

        // lobby chat
        private string lobbyChatText = string.Empty;
        private Dictionary<int, string> lobbyGameChatText = new Dictionary<int, string>();


        private GameServerClient gameServerClient = null;
        private Coroutine monitorPlayerCharactersCoroutine = null;
        private bool serverModeActive = false;
        private NetworkClientMode clientMode = NetworkClientMode.Lobby;

        // game manager references
        private SaveManager saveManager = null;
        private ChatCommandManager chatCommandManager = null;
        private LogManager logManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private InteractionManager interactionManager = null;
        private LevelManagerServer levelManagerServer = null;
        private SystemDataFactory systemDataFactory = null;

        [SerializeField]
        private NetworkController networkController = null;

        public bool ServerModeActive { get => serverModeActive; }
        public NetworkClientMode ClientMode { get => clientMode; set => clientMode = value; }
        public Dictionary<int, LoggedInAccount> LoggedInAccounts { get => loggedInAccounts; }
        public Dictionary<int, LobbyGame> LobbyGames { get => lobbyGames; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            chatCommandManager = systemGameManager.ChatCommandManager;
            logManager = systemGameManager.LogManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterManager = systemGameManager.CharacterManager;
            interactionManager = systemGameManager.InteractionManager;
            levelManagerServer = systemGameManager.LevelManagerServer;
            systemDataFactory = systemGameManager.SystemDataFactory;

            networkController?.Configure(systemGameManager);

        }

        public void SetClientToken(int clientId, string token) {
            //Debug.Log($"NetworkManagerServer.SetClientToken({clientId}, {token})");

            if (loginRequests.ContainsKey(clientId)) {
                loggedInAccounts.Add(clientId, new LoggedInAccount(clientId, loginRequests[clientId], token, GetClientIPAddress(clientId)));
            }
        }

        public void OnSetGameMode(GameMode gameMode) {
            //Debug.Log($"NetworkManagerServer.OnSetGameMode({gameMode})");
            
            if (gameMode == GameMode.Network) {
                // create instance of GameServerClient
                gameServerClient = new GameServerClient(systemGameManager, systemConfigurationManager.ApiServerAddress);
                if (monitorPlayerCharactersCoroutine == null) {
                    monitorPlayerCharactersCoroutine = StartCoroutine(MonitorPlayerCharacters());
                }
                return;
            }

            // local mode
            if (monitorPlayerCharactersCoroutine != null) {
                StopCoroutine(monitorPlayerCharactersCoroutine);
            }
        }

        public IEnumerator MonitorPlayerCharacters() {
            while (systemGameManager.GameMode == GameMode.Network) {
                foreach (PlayerCharacterMonitor playerCharacterMonitor in activePlayerCharacters.Values) {
                    SavePlayerCharacter(playerCharacterMonitor);
                }
                yield return new WaitForSeconds(10);
            }
        }

        private void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            playerCharacterMonitor.SavePlayerLocation();
            if (playerCharacterMonitor.saveDataDirty == true) {
                if (loggedInAccounts.ContainsKey(playerCharacterMonitor.clientId) == false) {
                    // can't do anything without a token
                    return;
                }
                if (clientMode == NetworkClientMode.MMO) {
                    gameServerClient.SavePlayerCharacter(
                        playerCharacterMonitor.clientId,
                        loggedInAccounts[playerCharacterMonitor.clientId].token,
                        playerCharacterMonitor.playerCharacterSaveData.PlayerCharacterId,
                        playerCharacterMonitor.playerCharacterSaveData.SaveData);
                }
            }
        }

        public void GetLoginToken(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.GetLoginToken({clientId}, {username}, {password})");

            loginRequests.Add(clientId, username);
            //(bool correctPassword, string token) = gameServerClient.Login(clientId, username, password);
            if (clientMode == NetworkClientMode.MMO) {
                gameServerClient.Login(clientId, username, password);
            } else {
                // fow now the lobby mode accept any password to avoid having to deal with an account system
                ProcessLoginResponse(clientId, true, string.Empty);
            }
        }

        public void ProcessLoginResponse(int clientId, bool correctPassword, string token) {
            //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {correctPassword}, {token})");

            if (correctPassword == true) {
                SetClientToken(clientId, token);
            }
            loginRequests.Remove(clientId);
            OnAuthenticationResult(clientId, true, correctPassword);
            
            if (correctPassword == false) {
                return;
            }

            //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {correctPassword}, {token}) {loggedInAccounts[clientId].username} logged in.");

            OnLobbyLogin(clientId);
            networkController.AdvertiseLobbyLogin(clientId, loggedInAccounts[clientId].username);
        }

        public void CreatePlayerCharacter(int clientId, AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerServer.CreatePlayerCharacter(AnyRPGSaveData)");
            if (loggedInAccounts.ContainsKey(clientId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.CreatePlayerCharacter(clientId, loggedInAccounts[clientId].token, anyRPGSaveData);
        }

        public void ProcessCreatePlayerCharacterResponse(int clientId) {
            Debug.Log($"NetworkManagerServer.ProcessCreatePlayerCharacterResponse({clientId})");

            OnCreatePlayerCharacter(clientId);
        }


        public void DeletePlayerCharacter(int clientId, int playerCharacterId) {
            Debug.Log($"NetworkManagerServer.DeletePlayerCharacter({playerCharacterId})");

            if (loggedInAccounts.ContainsKey(clientId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.DeletePlayerCharacter(clientId, loggedInAccounts[clientId].token, playerCharacterId);
        }

        public void ProcessStopNetworkUnitServer(UnitController unitController) {
            Debug.Log($"NetworkManagerServer.ProcessStopServer({unitController.gameObject.name})");

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            characterManager.ProcessStopNetworkUnit(unitController);

            foreach (int playerCharacterId in activePlayerCharacters.Keys) {
                if (activePlayerCharacters[playerCharacterId].unitController == unitController) {
                    StopMonitoringPlayerUnit(playerCharacterId);
                    //unitController.Despawn(0f, false, true);
                    break;
                }
            }
        }

        public void ProcessDeletePlayerCharacterResponse(int clientId) {
            Debug.Log($"NetworkManagerServer.ProcessDeletePlayerCharacterResponse({clientId})");

            OnDeletePlayerCharacter(clientId);
        }

        public void LoadCharacterList(int clientId) {
            Debug.Log($"NetworkManagerServer.LoadCharacterList({clientId})");
            if (loggedInAccounts.ContainsKey(clientId) == false) {
                // can't do anything without a token
                //return new List<PlayerCharacterSaveData>();
                return;
            }
            gameServerClient.LoadCharacterList(clientId, loggedInAccounts[clientId].token);
            //List<PlayerCharacterData> playerCharacterDataList = gameServerClient.LoadCharacterList(clientId, clientTokens[clientId]);

            //Debug.Log($"NetworkManagerServer.LoadCharacterListServer({clientId}) list size: {playerCharacterDataList.Count}");

            //List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            //foreach (PlayerCharacterData playerCharacterData in playerCharacterDataList) {
            //    playerCharacterSaveDataList.Add(new PlayerCharacterSaveData() {
            //        PlayerCharacterId = playerCharacterData.id,
            //        SaveData = saveManager.LoadSaveDataFromString(playerCharacterData.saveData)
            //    });
            //}
            //if (playerCharacterDataDict.ContainsKey(clientId)) {
            //    playerCharacterDataDict[clientId] = playerCharacterSaveDataList;
            //} else {
            //    playerCharacterDataDict.Add(clientId, playerCharacterSaveDataList);
            //}

            //return playerCharacterSaveDataList;
        }

        public bool PlayerCharacterIsActive(int playerCharacterId) {
            return activePlayerCharacters.ContainsKey(playerCharacterId);
        }

        public int GetPlayerCharacterClientId(int playerCharacterId) {
            if (activePlayerCharacters.ContainsKey(playerCharacterId)) {
                return activePlayerCharacters[playerCharacterId].clientId;
            }
            return -1;
        }

        public void MonitorPlayerUnit(int clientId,  PlayerCharacterSaveData playerCharacterSaveData, UnitController unitController) {
            PlayerCharacterMonitor playerCharacterMonitor = new PlayerCharacterMonitor(
                systemGameManager,
                clientId,
                playerCharacterSaveData,
                unitController
            );
            activePlayerCharacters.Add(playerCharacterSaveData.PlayerCharacterId, playerCharacterMonitor);
            activePlayerCharactersByClient.Add(clientId, playerCharacterMonitor);
            playerManagerServer.AddActivePlayer(clientId, unitController);
        }

        public void StopMonitoringPlayerUnit(int playerCharacterId) {
            //Debug.Log($"NetworkManagerServer.StopMonitoringPlayerUnit({playerCharacterId})");

            if (activePlayerCharacters.ContainsKey(playerCharacterId)) {
                playerManagerServer.RemoveActivePlayer(activePlayerCharacters[playerCharacterId].clientId);

                activePlayerCharacters[playerCharacterId].StopMonitoring();
                // flush data to database before stop monitoring
                SavePlayerCharacter(activePlayerCharacters[playerCharacterId]);
                activePlayerCharactersByClient.Remove(activePlayerCharacters[playerCharacterId].clientId);
                activePlayerCharacters.Remove(playerCharacterId);
            }

        }

        public void ProcessLoadCharacterListResponse(int clientId, List<PlayerCharacterData> playerCharacters) {
            //Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({clientId})");

            List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            foreach (PlayerCharacterData playerCharacterData in playerCharacters) {
                playerCharacterSaveDataList.Add(new PlayerCharacterSaveData() {
                    PlayerCharacterId = playerCharacterData.id,
                    SaveData = saveManager.LoadSaveDataFromString(playerCharacterData.saveData)
                });
            }
            Dictionary<int, PlayerCharacterSaveData> playerCharacterSaveDataDict = new Dictionary<int, PlayerCharacterSaveData>();
            foreach (PlayerCharacterSaveData playerCharacterSaveData in playerCharacterSaveDataList) {
                playerCharacterSaveDataDict.Add(playerCharacterSaveData.PlayerCharacterId, playerCharacterSaveData);
            }
            if (playerCharacterDataDict.ContainsKey(clientId)) {
                playerCharacterDataDict[clientId] = playerCharacterSaveDataDict;
            } else {
                playerCharacterDataDict.Add(clientId, playerCharacterSaveDataDict);
            }

            OnLoadCharacterList(clientId, playerCharacterSaveDataList);
        }

        public PlayerCharacterSaveData GetPlayerCharacterSaveData(int clientId, int playerCharacterId) {
            if (playerCharacterDataDict.ContainsKey(clientId) == false) {
                return null;
            }
            if (playerCharacterDataDict[clientId].ContainsKey(playerCharacterId) == false) {
                return null;
            }
            return playerCharacterDataDict[clientId][playerCharacterId];
        }

        public string GetClientToken(int clientId) {
            Debug.Log($"NetworkManagerServer.GetClientToken({clientId})");

            if (loggedInAccounts.ContainsKey(clientId)) {
                return loggedInAccounts[clientId].token;
            }
            return string.Empty;
        }

        public void ProcessClientDisconnect(int clientId) {
            Debug.Log($"NetworkManagerServer.ProcessClientDisconnect({clientId})");

            if (loggedInAccounts.ContainsKey(clientId)) {
                loggedInAccounts.Remove(clientId);
            }
            OnLobbyLogout(clientId);
            foreach (LobbyGame lobbyGame in lobbyGames.Values) {
                if (lobbyGame.leaderClientId == clientId) {
                    CancelLobbyGame(clientId, lobbyGame.gameId);
                    break;
                }
            }
            networkController?.AdvertiseLobbyLogout(clientId);
        }

        public void ActivateServerMode() {
            //Debug.Log($"NetworkManagerServer.ActivateServerMode()");

            serverModeActive = true;
            OnStartServer();
        }

        public void DeactivateServerMode() {
            Debug.Log($"NetworkManagerServer.DeactivateServerMode()");

            serverModeActive = false;

            loggedInAccounts.Clear();
            lobbyGames.Clear();
            lobbyGameChatText.Clear();

            OnStopServer();
        }


        public void StartServer() {
            //Debug.Log($"NetworkManagerServer.StartServer()");

            if (serverModeActive == true) {
                return;
            }

            networkController?.StartServer();
        }

        public void StopServer() {
            Debug.Log($"NetworkManagerServer.StartServer()");

            if (serverModeActive == false) {
                return;
            }

            networkController?.StopServer();
        }

        public void KickPlayer(int clientId) {
            networkController?.KickPlayer(clientId);
        }

        public string GetClientIPAddress(int clientId) {
            return networkController?.GetClientIPAddress(clientId);
        }

        public void CreateLobbyGame(string sceneName, int clientId) {
            LobbyGame lobbyGame = new LobbyGame(clientId, lobbyGameCounter, sceneName, loggedInAccounts[clientId].username);
            lobbyGameCounter++;
            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            lobbyGameChatText.Add(lobbyGame.gameId, string.Empty);
            networkController.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int clientId, int gameId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderClientId != clientId) {
                // game not found, or requesting client is not leader
                return;
            }
            lobbyGames.Remove(gameId);
            lobbyGameChatText.Remove(gameId);
            networkController.AdvertiseCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId, int clientId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(clientId) == false) {
                // game or client doesn't exist
                return;
            }
            lobbyGames[gameId].AddPlayer(clientId, loggedInAccounts[clientId].username);
            networkController.AdvertiseClientJoinLobbyGame(gameId, clientId, loggedInAccounts[clientId].username);
        }

        public void RequestLobbyGameList(int clientId) {
            networkController.SetLobbyGameList(clientId, lobbyGames.Values.ToList<LobbyGame>());
        }

        public void RequestLobbyPlayerList(int clientId) {
            Dictionary<int, string> lobbyPlayerList = new Dictionary<int, string>();
            foreach (int loggedInClientId in loggedInAccounts.Keys) {
                lobbyPlayerList.Add(loggedInClientId, loggedInAccounts[loggedInClientId].username);
            }
            networkController.SetLobbyPlayerList(clientId, lobbyPlayerList);
        }

        public void ChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(clientId) == false) {
                // game or client doesn't exist
                return;
            }
            if (lobbyGames[gameId].PlayerList.ContainsKey(clientId) == false) {
                // client isn't part of lobby game
                return;
            }
            lobbyGames[gameId].PlayerList[clientId].unitProfileName = unitProfileName;
            networkController.AdvertiseChooseLobbyGameCharacter(gameId, clientId, unitProfileName);
        }

        public void ToggleLobbyGameReadyStatus(int gameId, int clientId) {
            //Debug.Log($"NetworkManagerClient.ToggleLobbyGameReadyStatus({gameId}, {clientId})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(clientId) == false) {
                // game did not exist or client was not in game
                return;
            }

            lobbyGames[gameId].PlayerList[clientId].ready = !lobbyGames[gameId].PlayerList[clientId].ready;
            networkController.AdvertiseSetLobbyGameReadyStatus(gameId, clientId, lobbyGames[gameId].PlayerList[clientId].ready);
        }

        public void StartLobbyGame(int gameId, int clientId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderClientId != clientId || lobbyGames[gameId].inProgress == true) {
                // game did not exist, non leader tried to start, or already in progress, nothing to do
                return;
            }
            lobbyGames[gameId].inProgress = true;
            networkController.AdvertiseStartLobbyGame(gameId, lobbyGames[gameId].sceneName);
        }

        public void LeaveLobbyGame(int gameId, int clientId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(clientId) == false) {
                // game or client doesn't exist
                return;
            }
            if (lobbyGames[gameId].leaderClientId == clientId) {
                CancelLobbyGame(clientId, gameId);
            } else {
                lobbyGames[gameId].RemovePlayer(clientId);
                networkController.AdvertiseClientLeaveLobbyGame(gameId, clientId);
            }
        }

        public void SendLobbyChatMessage(string messageText, int clientId) {
            if (loggedInAccounts.ContainsKey(clientId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[clientId].username}: {messageText}\n";
            lobbyChatText += addedText;
            lobbyChatText = ShortenStringOnNewline(lobbyChatText, maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyChatMessage(addedText);
        }

        public void SendLobbyGameChatMessage(string messageText, int clientId, int gameId) {
            if (loggedInAccounts.ContainsKey(clientId) == false) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(clientId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[clientId].username}: {messageText}\n";
            lobbyGameChatText[gameId] += addedText;
            lobbyGameChatText[gameId] = ShortenStringOnNewline(lobbyGameChatText[gameId], maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyGameChatMessage(addedText, gameId);
        }

        public void SendSceneChatMessage(string messageText, int clientId) {
            if (loggedInAccounts.ContainsKey(clientId) == false) {
                return;
            }
            logManager.WriteChatMessageServer(clientId, messageText);
        }

        public void AdvertiseSceneChatMessage(string messageText, int clientId) {
            networkController.AdvertiseSendSceneChatMessage(messageText, clientId);

            if (activePlayerCharactersByClient.ContainsKey(clientId) == false) {
                // no unit logged in
                return;
            }
            string addedText = $"{loggedInAccounts[clientId].username}: {messageText}\n";

            activePlayerCharactersByClient[clientId].unitController.UnitEventController.NotifyOnBeginChatMessage(addedText);
        }

        public void AdvertiseLoadScene(string sceneName, int clientId) {
            Debug.Log($"NetworkManagerServer.AdvertiseLoadScene({sceneName}, {clientId})");
            
            playerManagerServer.DespawnPlayerUnit(clientId);
            networkController.AdvertiseLoadScene(sceneName, clientId);
        }

        public static string ShortenStringOnNewline(string message, int messageLength) {
            // if the chat text is greater than the max size, keep splitting it on newlines until reaches an acceptable size
            while (message.Length > messageLength && message.Contains("\n")) {
                message = message.Split("\n", 1)[1];
            }
            return message;
        }

        public int GetServerPort() {
            return networkController.GetServerPort();
        }

        public void AdvertiseTeleport(int clientId, TeleportEffectProperties teleportEffectProperties) {
            playerManagerServer.DespawnPlayerUnit(clientId);
            networkController.AdvertiseLoadScene(teleportEffectProperties.LevelName, clientId);
        }

        public void ReturnObjectToPool(GameObject returnedObject) {
            networkController.ReturnObjectToPool(returnedObject);
        }

        public void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex, UnitController sourceUnitController) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                networkController.AdvertiseInteractWithQuestGiver(interactable, optionIndex, playerManagerServer.ActivePlayerLookup[sourceUnitController]);
            }
        }

        public void InteractWithOption(UnitController sourceUnitController, Interactable interactable, int componentIndex) {
            interactionManager.InteractWithOptionServer(sourceUnitController, interactable, componentIndex);
        }

        public void AdvertiseAddSpawnRequest(int clientId, LoadSceneRequest loadSceneRequest) {
            networkController.AdvertiseAddSpawnRequest(clientId, loadSceneRequest);
        }

        public void InteractWithClassChangeComponent(int clientId, Interactable interactable, int optionIndex) {
            networkController.InteractWithClassChangeComponentServer(clientId, interactable, optionIndex);
        }

        public void HandleSceneLoadEnd(Scene scene) {
            levelManagerServer.AddLoadedScene(scene);
            levelManagerServer.ProcessLevelLoad(scene.name);
        }

        public void HandleSceneUnloadEnd(string sceneName) {
            levelManagerServer.RemoveLoadedScene(sceneName);
        }

        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return networkController.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        public GameObject SpawnModelPrefab(int spawnRequestId, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerServer.SpawnModelPrefab({spawnRequestId})");

            return networkController.SpawnModelPrefabServer(spawnRequestId, spawnPrefab, parentTransform, position, forward);
        }

        public void SetPlayerCharacterClass(string className, int clientId) {
            CharacterClass characterClass = systemDataFactory.GetResource<CharacterClass>(className);
            if (characterClass == null) {
                return;
            }
            playerManagerServer.SetPlayerCharacterClass(characterClass, clientId);
        }
    }

}
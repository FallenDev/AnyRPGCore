using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class NetworkManagerServer : ConfiguredMonoBehaviour, ICharacterRequestor {

        public event Action<int, int, bool, bool> OnAuthenticationResult = delegate { };
        public event Action<int, List<PlayerCharacterSaveData>> OnLoadCharacterList = delegate { };
        public event Action<int> OnDeletePlayerCharacter = delegate { };
        public event Action<int> OnCreatePlayerCharacter = delegate { };
        public event Action OnStartServer = delegate { };
        public event Action OnStopServer = delegate { };
        public event Action<int> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int> OnStartLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };

        [SerializeField]
        private NetworkController networkController = null;

        // jwt for each client so the server can make API calls to the api server on their behalf
        //private Dictionary<int, string> clientTokens = new Dictionary<int, string>();

        // cached list of player character save data from client lookups used for loading games
        /// <summary>
        /// accountId, playerCharacterId, playerCharacterSaveData
        /// </summary>
        private Dictionary<int, Dictionary<int, PlayerCharacterSaveData>> playerCharacterDataDict = new Dictionary<int, Dictionary<int, PlayerCharacterSaveData>>();

        /// <summary>
        /// accountId, playerCharacterMonitor
        /// </summary>
        private Dictionary<int, PlayerCharacterMonitor> activePlayerCharacters = new Dictionary<int, PlayerCharacterMonitor>();

        /// <summary>
        /// accountId, playerCharacterMonitor
        /// </summary>
        //private Dictionary<int, PlayerCharacterMonitor> activePlayerCharactersByAccount = new Dictionary<int, PlayerCharacterMonitor>();

        /// <summary>
        /// clientId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccountsByClient = new Dictionary<int, LoggedInAccount>();

        /// <summary>
        /// accountId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccounts = new Dictionary<int, LoggedInAccount>();


        /// <summary>
        /// clientId, username
        /// </summary>
        private Dictionary<int, string> loginRequests = new Dictionary<int, string>();

        /// <summary>
        /// username, temporarylobbyaccount
        /// </summary>
        private Dictionary<string, TemporaryLobbyAccount> lobbyAccounts = new Dictionary<string, TemporaryLobbyAccount>();

        // list of lobby games
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();

        /// <summary>
        /// accountId, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameAccountLookup = new Dictionary<int, int>();

        /// <summary>
        /// hashcode, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameLoadRequestHashCodes = new Dictionary<int, int>();

        /// <summary>
        /// gameId, sceneFileName, sceneHandle
        /// </summary>
        private Dictionary<int, Dictionary<string, int>> lobbyGameSceneHandles = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>
        /// sceneHandle, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameSceneHandleLookup = new Dictionary<int, int>();

        private int lobbyGameCounter = 0;
        private int lobbyGameAccountCounter = 0;
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
        private VendorManager vendorManager = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;
        private CraftingManager craftingManager = null;
        private UnitSpawnManager unitSpawnManager = null;

        public bool ServerModeActive { get => serverModeActive; }
        public NetworkClientMode ClientMode { get => clientMode; set => clientMode = value; }
        public Dictionary<int, LoggedInAccount> LoggedInAccounts { get => loggedInAccounts; }
        public Dictionary<int, LoggedInAccount> LoggedInAccountsByClient { get => loggedInAccountsByClient; }
        public Dictionary<int, LobbyGame> LobbyGames { get => lobbyGames; }
        public Dictionary<int, Dictionary<string, int>> LobbyGameSceneHandles { get => lobbyGameSceneHandles; }
        public Dictionary<int, int> LobbyGameAccountLookup { get => lobbyGameAccountLookup; set => lobbyGameAccountLookup = value; }
        public Dictionary<int, PlayerCharacterMonitor> ActivePlayerCharacters { get => activePlayerCharacters; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkController?.Configure(systemGameManager);
        }

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
            vendorManager = systemGameManager.VendorManager;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
            craftingManager = systemGameManager.CraftingManager;
            unitSpawnManager = systemGameManager.UnitSpawnManager;
        }

        public void AddLoggedInAccount(int clientId, int accountId, string token) {
            Debug.Log($"NetworkManagerServer.AddLoggedInAccount({clientId}, {accountId}, {token})");

            if (loginRequests.ContainsKey(clientId)) {
                if (loggedInAccounts.ContainsKey(accountId)) {
                    int oldClientId = loggedInAccounts[accountId].clientId;
                    loggedInAccounts[accountId].clientId = clientId;
                    loggedInAccounts[accountId].token = token;
                    loggedInAccounts[accountId].ipAddress = GetClientIPAddress(clientId);
                    loggedInAccountsByClient.Remove(oldClientId);
                    loggedInAccountsByClient.Add(clientId, loggedInAccounts[accountId]);
                } else {
                    LoggedInAccount loggedInAccount = new LoggedInAccount(clientId, accountId, loginRequests[clientId], token, GetClientIPAddress(clientId));
                    loggedInAccounts.Add(accountId, loggedInAccount);
                    loggedInAccountsByClient.Add(clientId, loggedInAccount);
                }
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
                    if (playerCharacterMonitor.unitController != null) {
                        SavePlayerCharacter(playerCharacterMonitor);
                    }
                }
                yield return new WaitForSeconds(10);
            }
        }

        private void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            if (playerCharacterMonitor.unitController != null) {
                playerCharacterMonitor.SavePlayerLocation();
            }
            if (playerCharacterMonitor.saveDataDirty == true) {
                if (clientMode == NetworkClientMode.MMO) {
                    if (loggedInAccounts.ContainsKey(playerCharacterMonitor.accountId) == false) {
                        // can't do anything without a token
                        return;
                    }
                    gameServerClient.SavePlayerCharacter(
                        playerCharacterMonitor.accountId,
                        loggedInAccounts[playerCharacterMonitor.accountId].token,
                        playerCharacterMonitor.playerCharacterSaveData.PlayerCharacterId,
                        playerCharacterMonitor.playerCharacterSaveData.SaveData);
                }
            }
        }

        public void GetLoginToken(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.GetLoginToken({clientId}, {username}, {password})");

            loginRequests.Add(clientId, username);
            if (clientMode == NetworkClientMode.MMO) {
                gameServerClient.Login(clientId, username, password);
            } else {
                LobbyLogin(clientId, username, password);
            }
        }

        public void LobbyLogin(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.LobbyLogin({clientId}, {username}, {password})");
            if (lobbyAccounts.ContainsKey(username) == false) {
                int accountId = CreateLobbyAccount(username, password);
                ProcessLoginResponse(clientId, accountId, true, string.Empty);
                return;
            } else {
                if (lobbyAccounts[username].password == password) {
                    // password correct
                    ProcessLoginResponse(clientId, lobbyAccounts[username].accountId, true, string.Empty);
                    return;
                } else {
                    // password incorrect
                    ProcessLoginResponse(clientId, -1, false, string.Empty);
                    return;
                }
            }
        }

        private int CreateLobbyAccount(string username, string password) {
            TemporaryLobbyAccount temporaryLobbyAccount = new TemporaryLobbyAccount(GetNewLobbyAccountId(), username, password);
            lobbyAccounts.Add(username, temporaryLobbyAccount);
            return temporaryLobbyAccount.accountId;
        }

        private int GetNewLobbyAccountId() {
            int returnValue = lobbyGameAccountCounter;
            lobbyGameAccountCounter++;
            return returnValue;
        }

        public void ProcessLoginResponse(int clientId, int accountId, bool correctPassword, string token) {
            //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {accountId}, {correctPassword}, {token})");

            if (correctPassword == true) {
                if (loggedInAccounts.ContainsKey(accountId)) {
                    if (playerManagerServer.ActivePlayers.ContainsKey(accountId)) {
                        // if the player is already logged in, we need to add a spawn request to match the current position and direction of the player
                        playerManagerServer.AddSpawnRequest(accountId, new SpawnPlayerRequest() {
                            overrideSpawnDirection = true,
                            spawnForwardDirection = playerManagerServer.ActivePlayers[accountId].transform.forward,
                            overrideSpawnLocation = true,
                            spawnLocation = playerManagerServer.ActivePlayers[accountId].transform.position
                        });
                    }
                    // if the account is already logged in, kick the old client
                    KickPlayer(accountId);
                }
                AddLoggedInAccount(clientId, accountId, token);
            }
            loginRequests.Remove(clientId);
            OnAuthenticationResult(clientId, accountId, true, correctPassword);
            
            if (correctPassword == false) {
                return;
            }

            OnLobbyLogin(accountId);
            networkController.AdvertiseLobbyLogin(accountId, loggedInAccounts[accountId].username);
        }

        public void CreatePlayerCharacter(int accountId, AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerServer.CreatePlayerCharacter(AnyRPGSaveData)");
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.CreatePlayerCharacter(accountId, loggedInAccounts[accountId].token, anyRPGSaveData);
        }

        public void ProcessCreatePlayerCharacterResponse(int accountId) {
            Debug.Log($"NetworkManagerServer.ProcessCreatePlayerCharacterResponse({accountId})");

            OnCreatePlayerCharacter(accountId);
        }


        public void DeletePlayerCharacter(int accountId, int playerCharacterId) {
            Debug.Log($"NetworkManagerServer.DeletePlayerCharacter({playerCharacterId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.DeletePlayerCharacter(accountId, loggedInAccounts[accountId].token, playerCharacterId);
        }

        public void ProcessStopNetworkUnitServer(UnitController unitController) {
            //Debug.Log($"NetworkManagerServer.ProcessStopNetworkUnitServer({unitController.gameObject.name})");

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            characterManager.ProcessStopNetworkUnit(unitController);

            if (playerManagerServer.ActivePlayerLookup.ContainsKey(unitController)) {
                StopMonitoringPlayerUnit(playerManagerServer.ActivePlayerLookup[unitController]);
            }
        }

        public void ProcessDeletePlayerCharacterResponse(int accountId) {
            Debug.Log($"NetworkManagerServer.ProcessDeletePlayerCharacterResponse({accountId})");

            OnDeletePlayerCharacter(accountId);
        }

        public void LoadCharacterList(int accountId) {
            Debug.Log($"NetworkManagerServer.LoadCharacterList({accountId})");
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                //return new List<PlayerCharacterSaveData>();
                return;
            }
            gameServerClient.LoadCharacterList(accountId, loggedInAccounts[accountId].token);
        }

        public bool PlayerCharacterIsActive(int playerCharacterId) {
            return activePlayerCharacters.ContainsKey(playerCharacterId);
        }

        public int GetPlayerCharacterAccountId(int playerCharacterId) {
            if (activePlayerCharacters.ContainsKey(playerCharacterId)) {
                return activePlayerCharacters[playerCharacterId].accountId;
            }
            return -1;
        }

        public void AddPlayerMonitor(int accountId, PlayerCharacterSaveData playerCharacterSaveData) {

            if (activePlayerCharacters.ContainsKey(accountId)) {
                return;
            }
            else {
                PlayerCharacterMonitor playerCharacterMonitor = new PlayerCharacterMonitor(
                    systemGameManager,
                    accountId,
                    playerCharacterSaveData,
                    null
                );
                activePlayerCharacters.Add(accountId, playerCharacterMonitor);
            }
        }

        public void MonitorPlayerUnit(int accountId, UnitController unitController) {
            
            if (activePlayerCharacters.ContainsKey(accountId) == false) {
                return;
            }
            activePlayerCharacters[accountId].SetUnitController(unitController);
            playerManagerServer.AddActivePlayer(accountId, unitController);
        }

        public void StopMonitoringPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.StopMonitoringPlayerUnit({playerCharacterId})");

            if (activePlayerCharacters.ContainsKey(accountId)) {
                playerManagerServer.RemoveActivePlayer(accountId);

                activePlayerCharacters[accountId].StopMonitoring();
                // flush data to database before stop monitoring
                SavePlayerCharacter(activePlayerCharacters[accountId]);
                //activePlayerCharactersByAccount.Remove(activePlayerCharacters[playerCharacterId].accountId);
                activePlayerCharacters.Remove(accountId);
            }

        }

        public void ProcessLoadCharacterListResponse(int accountId, List<PlayerCharacterData> playerCharacters) {
            //Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({accountId})");

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
            if (playerCharacterDataDict.ContainsKey(accountId)) {
                playerCharacterDataDict[accountId] = playerCharacterSaveDataDict;
            } else {
                playerCharacterDataDict.Add(accountId, playerCharacterSaveDataDict);
            }

            OnLoadCharacterList(accountId, playerCharacterSaveDataList);
        }

        public PlayerCharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            if (playerCharacterDataDict.ContainsKey(accountId) == false) {
                return null;
            }
            if (playerCharacterDataDict[accountId].ContainsKey(playerCharacterId) == false) {
                return null;
            }
            return playerCharacterDataDict[accountId][playerCharacterId];
        }

        public string GetAccountToken(int accountId) {
            Debug.Log($"NetworkManagerServer.GetClientToken({accountId})");

            if (loggedInAccounts.ContainsKey(accountId)) {
                return loggedInAccounts[accountId].token;
            }
            return string.Empty;
        }

        public void ProcessClientDisconnect(int clientId) {
            Debug.Log($"NetworkManagerServer.ProcessClientDisconnect({clientId})");

            if (loggedInAccountsByClient.ContainsKey(clientId) == false) {
                return;
            }
            int accountId = loggedInAccountsByClient[clientId].accountId;
            loggedInAccounts.Remove(accountId);
            loggedInAccountsByClient.Remove(clientId);

            OnLobbyLogout(accountId);
            foreach (LobbyGame lobbyGame in lobbyGames.Values) {
                if (lobbyGame.leaderAccountId == accountId) {
                    CancelLobbyGame(accountId, lobbyGame.gameId);
                    break;
                }
            }
            networkController?.AdvertiseLobbyLogout(accountId);
        }

        public void ActivateServerMode() {
            //Debug.Log($"NetworkManagerServer.ActivateServerMode()");

            serverModeActive = true;
            OnStartServer();
        }

        public void DeactivateServerMode() {
            Debug.Log($"NetworkManagerServer.DeactivateServerMode()");

            serverModeActive = false;

            loggedInAccountsByClient.Clear();
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

        public void KickPlayer(int accountId) {
            Debug.Log($"NetworkManagerServer.KickPlayer({accountId})");
            networkController?.KickPlayer(accountId);
        }

        public string GetClientIPAddress(int clientId) {
            return networkController?.GetClientIPAddress(clientId);
        }

        public void CreateLobbyGame(string sceneResourceName, int accountId, bool allowLateJoin) {
            
            LobbyGame lobbyGame = new LobbyGame(accountId, lobbyGameCounter, sceneResourceName, loggedInAccounts[accountId].username, allowLateJoin);
            lobbyGameCounter++;
            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            lobbyGameAccountLookup.Add(accountId, lobbyGame.gameId);
            lobbyGameChatText.Add(lobbyGame.gameId, string.Empty);
            OnCreateLobbyGame(lobbyGame);
            networkController.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int accountId, int gameId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId) {
                // game not found, or requesting client is not leader
                return;
            }
            foreach (int accountIdInGame in lobbyGames[gameId].PlayerList.Keys) {
                lobbyGameAccountLookup.Remove(accountIdInGame);
            }
            lobbyGames.Remove(gameId);
            lobbyGameChatText.Remove(gameId);
            OnCancelLobbyGame(gameId);
            networkController.AdvertiseCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            lobbyGames[gameId].AddPlayer(accountId, loggedInAccounts[accountId].username);
            lobbyGameAccountLookup.Add(accountId, gameId);
            OnJoinLobbyGame(gameId, accountId, loggedInAccounts[accountId].username);
            networkController.AdvertiseAccountJoinLobbyGame(gameId, accountId, loggedInAccounts[accountId].username);
        }

        public void RequestLobbyGameList(int accountId) {
            networkController.SetLobbyGameList(accountId, lobbyGames.Values.ToList<LobbyGame>());
        }

        public void RequestLobbyPlayerList(int accountId) {
            Dictionary<int, string> lobbyPlayerList = new Dictionary<int, string>();
            foreach (int loggedInClientId in loggedInAccountsByClient.Keys) {
                lobbyPlayerList.Add(loggedInClientId, loggedInAccountsByClient[loggedInClientId].username);
            }
            networkController.SetLobbyPlayerList(accountId, lobbyPlayerList);
        }

        public void ChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccountsByClient.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            if (lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // client isn't part of lobby game
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].unitProfileName = unitProfileName;
            networkController.AdvertiseChooseLobbyGameCharacter(gameId, accountId, unitProfileName);
        }

        public void ToggleLobbyGameReadyStatus(int gameId, int accountId) {
            //Debug.Log($"NetworkManagerClient.ToggleLobbyGameReadyStatus({gameId}, {accountId})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // game did not exist or client was not in game
                return;
            }

            lobbyGames[gameId].PlayerList[accountId].ready = !lobbyGames[gameId].PlayerList[accountId].ready;
            networkController.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, lobbyGames[gameId].PlayerList[accountId].ready);
        }

        public void RequestStartLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId || lobbyGames[gameId].inProgress == true) {
                // game did not exist, non leader tried to start, or already in progress, nothing to do
                return;
            }
            StartLobbyGame(gameId);
        }

        public void RequestJoinLobbyGameInProgress(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].inProgress == false || lobbyGames[gameId].allowLateJoin == false) {
                // game did not exist or not in progress or does not allow late joins
                return;
            }
            JoinLobbyGameInProgress(gameId, accountId);
        }

        public void JoinLobbyGameInProgress(int gameId, int accountId) {
            Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId})");

            if (playerManagerServer.SpawnRequests.ContainsKey(accountId) == false) {
                playerManagerServer.AddSpawnRequest(accountId, new SpawnPlayerRequest());
            } else {
                // player already has a spawn request, so this is a rejoin.  Leave it alone because it contains the last correct position and direction
            }
            networkController.AdvertiseJoinLobbyGameInProgress(gameId, accountId);
        }

        public void StartLobbyGame(int gameId) {
            Debug.Log($"NetworkManagerServer.StartLobbyGame({gameId})");

            lobbyGames[gameId].inProgress = true;
            OnStartLobbyGame(gameId);
            // create spawn requests for all players in the game
            foreach (int accountId in lobbyGames[gameId].PlayerList.Keys) {
                playerManagerServer.AddSpawnRequest(accountId, new SpawnPlayerRequest());
            }
            networkController.StartLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            if (lobbyGames[gameId].leaderAccountId == accountId) {
                CancelLobbyGame(accountId, gameId);
            } else {
                lobbyGames[gameId].RemovePlayer(accountId);
                lobbyGameAccountLookup.Add(accountId, gameId);
                OnLeaveLobbyGame(gameId, accountId);
                networkController.AdvertiseAccountLeaveLobbyGame(gameId, accountId);
            }
        }

        public void SendLobbyChatMessage(string messageText, int accountId) {
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";
            lobbyChatText += addedText;
            lobbyChatText = ShortenStringOnNewline(lobbyChatText, maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyChatMessage(addedText);
        }

        public void SendLobbyGameChatMessage(string messageText, int accountId, int gameId) {
            if (loggedInAccountsByClient.ContainsKey(accountId) == false) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";
            lobbyGameChatText[gameId] += addedText;
            lobbyGameChatText[gameId] = ShortenStringOnNewline(lobbyGameChatText[gameId], maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyGameChatMessage(addedText, gameId);
        }

        public void SendSceneChatMessage(string messageText, int accountId) {
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            logManager.WriteChatMessageServer(accountId, messageText);
        }

        public void AdvertiseSceneChatMessage(string messageText, int accountId) {
            networkController.AdvertiseSendSceneChatMessage(messageText, accountId);

            if (activePlayerCharacters.ContainsKey(accountId) == false) {
                // no unit logged in
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";

            activePlayerCharacters[accountId].unitController.UnitEventController.NotifyOnBeginChatMessage(addedText);
        }

        public void AdvertiseLoadScene(string sceneName, int accountId) {
            Debug.Log($"NetworkManagerServer.AdvertiseLoadScene({sceneName}, {accountId})");
            
            DespawnPlayerUnit(accountId);
            networkController.AdvertiseLoadScene(sceneName, accountId);
        }

        public void DespawnPlayerUnit(int accountId) {
            Debug.Log($"NetworkManagerServer.DespawnPlayerUnit({accountId})");

            activePlayerCharacters[accountId].Despawn();
            playerManagerServer.DespawnPlayerUnit(accountId);
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

        public void AdvertiseTeleport(int accountId, TeleportEffectProperties teleportEffectProperties) {
            Debug.Log($"NetworkManagerServer.AdvertiseTeleport({accountId}, {teleportEffectProperties.LevelName})");

            DespawnPlayerUnit(accountId);
            networkController.AdvertiseLoadScene(teleportEffectProperties.LevelName, accountId);
        }

        public void ReturnObjectToPool(GameObject returnedObject) {
            networkController.ReturnObjectToPool(returnedObject);
        }

        /*
        public void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex, UnitController sourceUnitController) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                networkController.AdvertiseInteractWithQuestGiver(interactable, optionIndex, playerManagerServer.ActivePlayerLookup[sourceUnitController]);
            }
        }
        */

        public void InteractWithOption(UnitController sourceUnitController, Interactable interactable, int componentIndex, int choiceIndex) {
            interactionManager.InteractWithOptionServer(sourceUnitController, interactable, componentIndex, choiceIndex);
        }

        /*
        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            networkController.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
        }
        */

        /*
        public void AdvertiseInteractWithClassChangeComponent(int accountId, Interactable interactable, int optionIndex) {
            networkController.AdvertiseInteractWithClassChangeComponentServer(accountId, interactable, optionIndex);
        }
        */

        public void HandleSceneLoadEnd(Scene scene, int loadRequestHashCode) {
            Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode})");
            if (lobbyGameLoadRequestHashCodes.ContainsKey(loadRequestHashCode) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode}) - lobby game load request");
                AddLobbyGameSceneHandle(lobbyGameLoadRequestHashCodes[loadRequestHashCode], scene);
                lobbyGameLoadRequestHashCodes.Remove(loadRequestHashCode);
            }
            levelManagerServer.AddLoadedScene(scene);
            levelManagerServer.ProcessLevelLoad(scene);
        }

        private void AddLobbyGameSceneHandle(int lobbyGameId, Scene scene) {
            if (lobbyGameSceneHandles.ContainsKey(lobbyGameId) == false) {
                lobbyGameSceneHandles.Add(lobbyGameId, new Dictionary<string, int>());
            }
            if (lobbyGameSceneHandles[lobbyGameId].ContainsKey(scene.name) == false) {
                lobbyGameSceneHandles[lobbyGameId].Add(scene.name, scene.handle);
            }
            if (lobbyGameSceneHandleLookup.ContainsKey(scene.handle) == false) {
                lobbyGameSceneHandleLookup.Add(scene.handle, lobbyGameId);
            }
        }

        public void HandleSceneUnloadEnd(int sceneHandle, string sceneName) {
            Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle})");

            if (lobbyGameSceneHandleLookup.ContainsKey(sceneHandle) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle}) - lobby game unload request");
                int lobbyGameId = lobbyGameSceneHandleLookup[sceneHandle];
                if (lobbyGameSceneHandles.ContainsKey(lobbyGameId) == true && lobbyGameSceneHandles[lobbyGameId].ContainsKey(sceneName) == true) {
                    lobbyGameSceneHandles[lobbyGameId].Remove(sceneName);
                }
                lobbyGameSceneHandleLookup.Remove(sceneHandle);
            }
            levelManagerServer.RemoveLoadedScene(sceneHandle, sceneName);
        }

        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return networkController.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        public GameObject SpawnModelPrefab(int clientSpawnRequestId, int serverSpawnRequestId, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerServer.SpawnModelPrefab({spawnRequestId})");

            return networkController.SpawnModelPrefabServer(clientSpawnRequestId, serverSpawnRequestId, spawnPrefab, parentTransform, position, forward);
        }

        public void SetPlayerCharacterClass(string className, int accountId) {
            CharacterClass characterClass = systemDataFactory.GetResource<CharacterClass>(className);
            if (characterClass == null) {
                return;
            }
            playerManagerServer.SetPlayerCharacterClass(characterClass, accountId);
        }

        public void SetPlayerCharacterSpecialization(string specializationName, int accountId) {
            ClassSpecialization classSpecialization = systemDataFactory.GetResource<ClassSpecialization>(specializationName);
            if (classSpecialization == null) {
                return;
            }
            playerManagerServer.SetPlayerCharacterSpecialization(classSpecialization, accountId);
        }

        public void SetPlayerFaction(string factionName, int accountId) {
            Faction faction = systemDataFactory.GetResource<Faction>(factionName);
            if (faction == null) {
                return;
            }
            playerManagerServer.SetPlayerFaction(faction, accountId);
        }

        public void LearnSkill(string skillName, int accountId) {
            Skill skill = systemDataFactory.GetResource<Skill>(skillName);
            if (skill == null) {
                return;
            }
            playerManagerServer.LearnSkill(skill, accountId);
        }

        public void AcceptQuest(string questName, int accountId) {
            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest == null) {
                return;
            }
            playerManagerServer.AcceptQuest(quest, accountId);
        }

        public void CompleteQuest(string questName, QuestRewardChoices questRewardChoices, int accountId) {
            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest == null) {
                return;
            }
            playerManagerServer.CompleteQuest(quest, questRewardChoices, accountId);
        }


        public void AdvertiseMessageFeedMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseMessageFeedMessage(playerManagerServer.ActivePlayerLookup[sourceUnitController], message);
        }

        public void AdvertiseSystemMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseSystemMessage(playerManagerServer.ActivePlayerLookup[sourceUnitController], message);
        }

        public void SellVendorItem(Interactable interactable, int componentIndex, int itemInstanceId, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) == false) {
                return;
            }
            vendorManager.SellItemToVendorServer(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, systemItemManager.InstantiatedItems[itemInstanceId]);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness, int accountId) {
            Debug.Log($"NetworkManagerServer.RequestSpawnUnit({interactable.gameObject.name}, {componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfile.ResourceName}, {unitToughness?.ResourceName}, {accountId})");

            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            unitSpawnManager.SpawnUnit(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            networkController.AdvertiseAddToBuyBackCollection(sourceUnitController, playerManagerServer.ActivePlayerLookup[sourceUnitController], interactable, componentIndex, newInstantiatedItem);
        }

        public void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            Debug.Log($"NetworkManagerServer.AdvertiseSellItemToPlayer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            networkController.AdvertiseSellItemToPlayer(sourceUnitController, interactable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            vendorManager.BuyItemFromVendorServer(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, collectionIndex, itemIndex, resourceName, accountId);
        }

        public void TakeAllLoot(int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                lootManager.TakeAllLootInternal(accountId, playerManagerServer.ActivePlayers[accountId]);
            }
        }

        public void RequestTakeLoot(int lootDropId, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                lootManager.TakeLoot(accountId, lootDropId);
            }
        }

        public void RequestBeginCrafting(Recipe recipe, int craftAmount, int accountId) {
            Debug.Log($"NetworkManagerServer.RequestBeginCrafting({recipe.DisplayName}, {craftAmount}, {accountId})");

            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                craftingManager.BeginCrafting(playerManagerServer.ActivePlayers[accountId], recipe, craftAmount);
            }
        }

        public void RequestCancelCrafting(int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                craftingManager.CancelCrafting(playerManagerServer.ActivePlayers[accountId]);
            }
        }


        public void AddAvailableDroppedLoot(int accountId, List<LootDrop> items) {
            //Debug.Log($"NetworkManagerServer.AddAvailableDroppedLoot({accountId}, count: {items.Count})");

            networkController.AddAvailableDroppedLoot(accountId, items);
        }

        public void AddLootDrop(int accountId, int lootDropId, int itemId) {
            networkController.AddLootDrop(accountId, lootDropId, itemId);
        }

        public void AdvertiseTakeLoot(int accountId, int lootDropId) {
            networkController.AdvertiseTakeLoot(accountId, lootDropId);
        }

        public void SetLobbyGameLoadRequestHashcode(int gameId, int hashCode) {
            if (lobbyGameLoadRequestHashCodes.ContainsKey(hashCode) == false) {
                lobbyGameLoadRequestHashCodes.Add(hashCode, gameId);
            }
        }

        public void RequestSpawnLobbyGamePlayer(int accountId, int gameId, int clientSpawnRequestId, UnitProfile unitProfile, string sceneName) {
            Debug.Log($"NetworkManagerServer.RequestSpawnLobbyGamePlayer({accountId}, {gameId}, {clientSpawnRequestId}, {unitProfile.ResourceName}, {sceneName})");

            PlayerCharacterSaveData playerCharacterSaveData = GetPlayerCharacterSaveData(gameId, accountId, unitProfile);

            int serverSpawnRequestId = characterManager.GetClientSpawnRequestId();

            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(systemDataFactory, playerCharacterSaveData.SaveData);
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
            CharacterRequestData characterRequestData = new CharacterRequestData(this, GameMode.Network, characterConfigurationRequest);
            characterRequestData.clientSpawnRequestId = clientSpawnRequestId;
            characterRequestData.serverSpawnRequestId = serverSpawnRequestId;
            characterRequestData.isServer = true;

            characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);

            //if (activePlayerCharacters.ContainsKey(accountId)) {
                // this code is meant to be used when the player is rejoining a game, so we can use the saved position and rotation
                /*
                position = new Vector3(
                    playerCharacterSaveData.SaveData.PlayerLocationX,
                    playerCharacterSaveData.SaveData.PlayerLocationY,
                    playerCharacterSaveData.SaveData.PlayerLocationZ);
                forward = new Vector3(
                    playerCharacterSaveData.SaveData.PlayerRotationX,
                    playerCharacterSaveData.SaveData.PlayerRotationY,
                    playerCharacterSaveData.SaveData.PlayerRotationZ);
                */
            //} else {
            //}

            SpawnPlayerRequest spawnPlayerRequest = playerManagerServer.GetSpawnPlayerRequest(accountId, sceneName);

            Vector3 position = spawnPlayerRequest.spawnLocation;
            if (spawnPlayerRequest.overrideSpawnLocation == false) {
                // we were loading the default location, so randomize the spawn position a bit so players don't all spawn in the same place
                position = new Vector3(position.x + UnityEngine.Random.Range(-2f, 2f), position.y, position.z + UnityEngine.Random.Range(-2f, 2f));
            }

            AddPlayerMonitor(accountId, playerCharacterSaveData);

            networkController.SpawnLobbyGamePlayer(accountId, clientSpawnRequestId, serverSpawnRequestId, characterRequestData, position, spawnPlayerRequest.spawnForwardDirection, sceneName);
        }



        private PlayerCharacterSaveData GetPlayerCharacterSaveData(int gameId, int accountId, UnitProfile unitProfile) {
            if (activePlayerCharacters.ContainsKey(accountId)) {
                return activePlayerCharacters[accountId].playerCharacterSaveData;
            } else {
                PlayerCharacterSaveData playerCharacterSaveData = saveManager.CreateSaveData();
                playerCharacterSaveData.PlayerCharacterId = accountId;
                playerCharacterSaveData.SaveData.playerName = lobbyGames[gameId].PlayerList[accountId].userName;
                playerCharacterSaveData.SaveData.unitProfileName = unitProfile.ResourceName;
                playerCharacterSaveData.SaveData.CurrentScene = lobbyGames[gameId].sceneResourceName;
                return playerCharacterSaveData;
            }
        }

        public void ConfigureSpawnedCharacter(UnitController unitController, CharacterRequestData characterRequestData) {
        }

        public void PostInit(UnitController unitController, CharacterRequestData characterRequestData) {
            Debug.Log($"NetworkManagerServer.PostInit({unitController.gameObject.name}, account: {characterRequestData.accountId})");

            // load player data from the active player characters dictionary
            if (!activePlayerCharacters.ContainsKey(characterRequestData.accountId)) {
                //Debug.LogError($"NetworkManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            unitController.CharacterSaveManager.LoadSaveDataToCharacter(activePlayerCharacters[characterRequestData.accountId].playerCharacterSaveData.SaveData);
        }

        public Scene GetAccountScene(int accountId, string sceneName) {
            return networkController.GetAccountScene(accountId, sceneName);
        }


        /*
        public void SetCraftingManagerAbility(UnitController sourceUnitController, string abilityName) {
            Debug.Log($"NetworkManagerServer.SetCraftingManagerAbility({sourceUnitController.gameObject.name}, {abilityName})");

            networkController.SetCraftingManagerAbility(playerManagerServer.ActivePlayerLookup[sourceUnitController], abilityName);
        }
        */

        /*
        public void AdvertiseInteractWithSkillTrainerComponent(int accountId, Interactable interactable, int optionIndex) {
            networkController.AdvertiseInteractWithSkillTrainerComponentServer(accountId, interactable, optionIndex);
        }

        public void AdvertiseInteractWithAnimatedObjectComponent(int accountId, Interactable interactable, int optionIndex) {
            networkController.AdvertiseInteractWithAnimatedObjectComponentServer(accountId, interactable, optionIndex);
        }
        */
    }

}
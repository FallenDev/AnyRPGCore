using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkManagerClient : ConfiguredMonoBehaviour {

        public event Action<string> OnClientVersionFailure = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };
        public event Action<string> OnSendLobbyChatMessage = delegate { };
        public event Action<string, int> OnSendLobbyGameChatMessage = delegate { };
        public event Action<int, string> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };
        public event Action<List<LobbyGame>> OnSetLobbyGameList = delegate { };
        public event Action<Dictionary<int, string>> OnSetLobbyPlayerList = delegate { };
        public event Action<int, int, string> OnChooseLobbyGameCharacter = delegate { };

        private string username = string.Empty;
        private string password = string.Empty;
        
        private bool isLoggingInOrOut = false;

        private NetworkClientMode clientMode = NetworkClientMode.Lobby;
        private int clientId;
        private LobbyGame lobbyGame;

        [SerializeField]
        private NetworkController networkController = null;

        private Dictionary<int, LoggedInAccount> lobbyGamePlayerList = new Dictionary<int, LoggedInAccount>();
        
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();
        private Dictionary<int, string> lobbyPlayers = new Dictionary<int, string>();

        // game manager references
        private PlayerManager playerManager = null;
        private CharacterManager characterManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public string Username { get => username; }
        public string Password { get => password; }
        public NetworkClientMode ClientMode { get => clientMode; set => clientMode = value; }
        public Dictionary<int, LoggedInAccount> LobbyGamePlayerList { get => lobbyGamePlayerList; }
        public LobbyGame LobbyGame { get => lobbyGame; }
        public int ClientId { get => clientId; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
        }

        public bool Login(string username, string password, string server) {
            //Debug.Log($"NetworkManagerClient.Login({username}, {password})");
            
            isLoggingInOrOut = true;

            this.username = username;
            this.password = password;
            return networkController.Login(username, password, server);
        }

        public void Logout() {
            isLoggingInOrOut = true;
            networkController.Logout();
        }

        public void LoadScene(string sceneName) {
            //Debug.Log($"NetworkManagerClient.LoadScene({sceneName})");

            networkController.LoadScene(sceneName);
        }

        public void SpawnPlayer(int playerCharacterId, CharacterRequestData characterRequestData, Transform parentTransform) {
            //Debug.Log($"NetworkManagerClient.SpawnPlayer({playerCharacterId})");

            if (characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.NetworkUnitPrefab == null) {
                Debug.LogWarning($"NetworkManagerClient.SpawnPlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}) On UnitProfile Network Unit Prefab is null ");
            }
            networkController.SpawnPlayer(playerCharacterId, characterRequestData, parentTransform);
        }

        public GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            return networkController.SpawnModelPrefab(spawnRequestId, prefab, parentTransform, position, forward);
        }

        public void SendLobbyChatMessage(string messageText) {
            networkController.SendLobbyChatMessage(messageText);
        }

        public void SendLobbyGameChatMessage(string messageText, int gameId) {
            networkController.SendLobbyGameChatMessage(messageText, gameId);
        }

        public void RequestLobbyPlayerList() {
            networkController.RequestLobbyPlayerList();
        }

        public bool CanSpawnPlayerOverNetwork() {
            return networkController.CanSpawnCharacterOverNetwork();
        }

        public bool OwnPlayer(UnitController unitController) {
            return networkController.OwnPlayer(unitController);
        }

        public void ProcessStopClient(UnitController unitController) {
            if (playerManager.UnitController == unitController) {
                playerManager.ProcessStopClient();
            } else {
                characterManager.ProcessStopClient(unitController);
            }
        }

        public void ProcessStopConnection() {
            Debug.Log($"NetworkManagerClient.ProcessStopConnection()");
            systemGameManager.SetGameMode(GameMode.Local);
            if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                if (isLoggingInOrOut == false) {
                    uIManager.AddPopupWindowToQueue(uIManager.disconnectedWindow);
                }
                isLoggingInOrOut = false;
                levelManager.LoadMainMenu();
                return;
            }

            // don't open disconnected window if this was an expected logout;
            if (isLoggingInOrOut == true) {
                isLoggingInOrOut = false;
                return;
            }
            
            // main menu, close main menu windows and open the disconnected window
            uIManager.newGameWindow.CloseWindow();
            uIManager.loadGameWindow.CloseWindow();
            uIManager.clientLobbyWindow.CloseWindow();
            uIManager.clientLobbyGameWindow.CloseWindow();
            uIManager.createLobbyGameWindow.CloseWindow();
            uIManager.disconnectedWindow.OpenWindow();
        }

        public void ProcessClientVersionFailure(string requiredClientVersion) {
            Debug.Log($"NetworkManagerClient.ProcessClientVersionFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.wrongClientVersionWindow.OpenWindow();
            OnClientVersionFailure(requiredClientVersion);
        }

        public void ProcessAuthenticationFailure() {
            Debug.Log($"NetworkManagerClient.ProcessAuthenticationFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.loginFailedWindow.OpenWindow();
        }

        public void ProcessLoginSuccess() {
            Debug.Log($"NetworkManagerClient.ProcessLoginSuccess()");

            // not doing this here because the connector has not spawned yet.
            //uIManager.ProcessLoginSuccess();

            isLoggingInOrOut = false;
        }

        public void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerClient.CreatePlayerCharacterClient(AnyRPGSaveData)");

            networkController.CreatePlayerCharacter(anyRPGSaveData);
        }

        public void RequestLobbyGameList() {
            networkController.RequestLobbyGameList();
        }

        public void LoadCharacterList() {
            //Debug.Log($"NetworkManagerClient.LoadCharacterList()");

            networkController.LoadCharacterList();
        }

        public void DeletePlayerCharacter(int playerCharacterId) {
            Debug.Log($"NetworkManagerClient.DeletePlayerCharacter({playerCharacterId})");

            networkController.DeletePlayerCharacter(playerCharacterId);
        }

        public void CreateLobbyGame(string sceneName) {
            networkController.CreateLobbyGame(sceneName);
        }

        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            Debug.Log($"NetworkManagerClient.AdvertiseCreateLobbyGame({lobbyGame.leaderClientId}) clientid: {clientId}");

            if (lobbyGame.leaderClientId == clientId) {
                this.lobbyGame = lobbyGame;
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
            OnCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int gameId) {
            networkController.CancelLobbyGame(gameId);
        }

        public void AdvertiseCancelLobbyGame(int gameId) {
            OnCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId) {
            networkController.JoinLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId) {
            networkController.LeaveLobbyGame(gameId);
        }

        public void SetClientId(int clientId) {
            this.clientId = clientId;
        }

        /*
        public int GetClientId() {
            Debug.Log($"NetworkManagerClient.GetClientId()");

            return networkController.GetClientId();
        }
        */

        public void AdvertiseClientJoinLobbyGame(int gameId, int clientId, string userName) {
            OnJoinLobbyGame(gameId, clientId, userName);
            lobbyGames[gameId].AddPlayer(clientId, userName);
            if (clientId == this.clientId) {
                // this client just joined a game
                lobbyGame = lobbyGames[gameId];
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
        }

        public void AdvertiseClientLeaveLobbyGame(int gameId, int clientId) {
            OnLeaveLobbyGame(gameId, clientId);
        }

        public void AdvertiseSendLobbyChatMessage(string messageText) {
            OnSendLobbyChatMessage(messageText);
        }

        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            OnSendLobbyGameChatMessage(messageText, gameId);
        }

        public void AdvertiseLobbyLogin(int clientId, string userName) {
            OnLobbyLogin(clientId, userName);
        }

        public void AdvertiseLobbyLogout(int clientId) {
            OnLobbyLogout(clientId);
        }

        public void SetLobbyGameList(List<LobbyGame> lobbyGames) {
            this.lobbyGames.Clear();
            foreach (LobbyGame lobbyGame in lobbyGames) {
                this.lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            }
            OnSetLobbyGameList(this.lobbyGames.Values.ToList<LobbyGame>());
        }

        public void SetLobbyPlayerList(Dictionary<int, string> lobbyPlayers) {
            this.lobbyPlayers.Clear();
            foreach (int loggedInClientId in lobbyPlayers.Keys) {
                this.lobbyPlayers.Add(loggedInClientId, lobbyPlayers[loggedInClientId]);
            }
            OnSetLobbyPlayerList(lobbyPlayers);
        }

        public void ChooseLobbyGameCharacter(string unitProfileName) {
            networkController.ChooseLobbyGameCharacter(unitProfileName, lobbyGame.gameId);
        }

        public void AdvertiseChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName) {
            Debug.Log($"NetworkManagerClient.AdvertiseChooseLobbyGameCharacter({gameId}, {clientId}, {unitProfileName})");

            if (gameId != lobbyGame.gameId) {
                // this is for another game, ignore it
                return;
            }
            if (lobbyGame.PlayerList.ContainsKey(clientId) == false) {
                // this client is not part of the current lobby game
                return;
            }
            lobbyGame.PlayerList[clientId].unitProfileName = unitProfileName;
            
            OnChooseLobbyGameCharacter(gameId, clientId, unitProfileName);
            uIManager.newGameWindow.CloseWindow();
        }
    }

    public enum NetworkClientMode { Lobby, MMO }

}
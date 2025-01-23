using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class ClientLobbyPanel : WindowContentController {

        [Header("ClientLobbyPanelController")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        [SerializeField]
        protected GameObject lobbyGameTemplate = null;

        [SerializeField]
        protected Transform lobbyGameContainer = null;

        [SerializeField]
        protected TMP_InputField chatInput = null;

        [SerializeField]
        protected TextMeshProUGUI chatDisplay = null;

        [SerializeField]
        protected HighlightButton logoutButton = null;

        [SerializeField]
        protected HighlightButton createGameButton = null;

        //[SerializeField]
        //protected HighlightButton joinGameButton = null;

        protected Dictionary<string, List<CreditsNode>> categoriesDictionary = new Dictionary<string, List<CreditsNode>>();

        private string lobbyChatText = string.Empty;
        private int maxLobbyChatTextSize = 64000;

        private Dictionary<int, ClientPlayerLobbyConnectionButton> playerButtons = new Dictionary<int, ClientPlayerLobbyConnectionButton>();
        private Dictionary<int, ClientLobbyGameConnectionButtonController> lobbyGameButtons = new Dictionary<int, ClientLobbyGameConnectionButtonController>();


        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            chatDisplay.text = string.Empty;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }


        public void Logout() {
            networkManagerClient.Logout();
            uIManager.clientLobbyWindow.CloseWindow();
        }

        public void CreateGame() {
            Debug.Log($"ClientLobbyPanelController.CreateGame()");

            uIManager.createLobbyGameWindow.OpenWindow();
        }

        public void SetStatusLabel() {
            serverStatusText.text = $"Logged In As: {networkManagerClient.Username}";
        }

        public void HandleLobbyLogin(int clientId, string userName) {
            AddPlayerToList(clientId, userName);
        }

        public void HandleLobbyLogout(int clientId) {
            RemovePlayerFromList(clientId);
        }

        public void HandleCreateLobbyGame(LobbyGame lobbyGame) {
            AddLobbyGameToList(lobbyGame.gameId, lobbyGame);
        }

        public void HandleCancelLobbyGame(int gameId) {
            RemoveLobbyGameFromList(gameId);
        }

        public void SendChatMessage() {
            networkManagerClient.SendLobbyChatMessage(chatInput.text);
        }

        public void HandleSendLobbyChatMessage(string messageText) {
            lobbyChatText += messageText;
            while (lobbyChatText.Length > maxLobbyChatTextSize && lobbyChatText.Contains("\n")) {
                lobbyChatText = lobbyChatText.Split("\n", 1)[1];
            }
            chatDisplay.text = lobbyChatText;
        }

        public void RequestLobbyPlayerList() {
            //Debug.Log($"ClientLobbyPanelController.RequestLobbyPlayerList()");

            networkManagerClient.RequestLobbyPlayerList();
        }

        public void HandleSetLobbyPlayerList(Dictionary<int, string> userNames) {
            PopulatePlayerList(userNames);
        }


        public void PopulatePlayerList(Dictionary<int, string> userNames) {
            //Debug.Log($"ClientLobbyPanelController.PopulatePlayerList()");

            foreach (KeyValuePair<int, string> loggedInAccount in userNames) {
                AddPlayerToList(loggedInAccount.Key, loggedInAccount.Value);
            }
        }

        public void AddPlayerToList(int clientId, string userName) {
            //Debug.Log($"ClientLobbyPanelController.AddPlayerToList({userName})");

            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            ClientPlayerLobbyConnectionButton clientPlayerLobbyConnectionButtonController = go.GetComponent<ClientPlayerLobbyConnectionButton>();
            clientPlayerLobbyConnectionButtonController.Configure(systemGameManager);
            clientPlayerLobbyConnectionButtonController.SetClientId(clientId, userName);
            //uINavigationControllers[1].AddActiveButton(clientPlayerLobbyConnectionButtonController.joinbu);
            playerButtons.Add(clientId, clientPlayerLobbyConnectionButtonController);
        }

        public void RemovePlayerFromList(int clientId) {
            //Debug.Log($"ClientLobbyPanelController.RemovePlayerFromList({clientId})");

            if (playerButtons.ContainsKey(clientId)) {
                //uINavigationControllers[1].ClearActiveButton(playerButtons[clientId].KickButton);
                if (playerButtons[clientId].gameObject != null) {
                    playerButtons[clientId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerButtons[clientId].gameObject);
                }
            }
        }

        public void ClearPlayerList() {

            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (ClientPlayerLobbyConnectionButton clientPlayerLobbyConnectionButtonController in playerButtons.Values) {
                if (clientPlayerLobbyConnectionButtonController.gameObject != null) {
                    clientPlayerLobbyConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientPlayerLobbyConnectionButtonController.gameObject);
                }
            }
            playerButtons.Clear();
            //uINavigationControllers[1].ClearActiveButtons();
        }


        public void RequestLobbyGameList() {
            //Debug.Log($"ClientLobbyPanelController.RequestLobbyGameList()");

            networkManagerClient.RequestLobbyGameList();
        }

        public void HandleSetLobbyGameList(List<LobbyGame> lobbyGames) {
            PopulateLobbyGameList(lobbyGames);
        }

        public void PopulateLobbyGameList(List<LobbyGame> lobbyGames) {
            //Debug.Log($"ClientLobbyPanelController.PopulateLobbyGameList()");

            foreach (LobbyGame lobbyGame in lobbyGames) {
                AddLobbyGameToList(lobbyGame.gameId, lobbyGame);
            }
        }

        public void AddLobbyGameToList(int gameId, LobbyGame lobbyGame) {
            //Debug.Log($"ClientLobbyPanelController.AddLobbyGameToList({gameId})");

            GameObject go = objectPooler.GetPooledObject(lobbyGameTemplate, lobbyGameContainer);
            ClientLobbyGameConnectionButtonController clientLobbyGameConnectionButtonController = go.GetComponent<ClientLobbyGameConnectionButtonController>();
            clientLobbyGameConnectionButtonController.Configure(systemGameManager);
            clientLobbyGameConnectionButtonController.SetGameId(gameId, lobbyGame.sceneName, lobbyGame.leaderUserName);
            uINavigationControllers[1].AddActiveButton(clientLobbyGameConnectionButtonController.JoinButton);
            lobbyGameButtons.Add(gameId, clientLobbyGameConnectionButtonController);
        }

        public void RemoveLobbyGameFromList(int gameId) {
            Debug.Log($"ClientLobbyPanelController.RemoveLobbyGameFromList({gameId})");

            if (lobbyGameButtons.ContainsKey(gameId)) {
                uINavigationControllers[1].ClearActiveButton(lobbyGameButtons[gameId].JoinButton);
                if (lobbyGameButtons[gameId].gameObject != null) {
                    lobbyGameButtons[gameId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(lobbyGameButtons[gameId].gameObject);
                }
            }
        }

        public void ClearLobbyGameList() {

            // clear the list so any button left over from a previous time opening the window aren't shown
            foreach (ClientLobbyGameConnectionButtonController clientLobbyGameConnectionButtonController in lobbyGameButtons.Values) {
                if (clientLobbyGameConnectionButtonController.gameObject != null) {
                    clientLobbyGameConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientLobbyGameConnectionButtonController.gameObject);
                }
            }
            lobbyGameButtons.Clear();
            uINavigationControllers[1].ClearActiveButtons();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            SetStatusLabel();
            RequestLobbyPlayerList();
            RequestLobbyGameList();
            networkManagerClient.OnSendLobbyChatMessage += HandleSendLobbyChatMessage;
            networkManagerClient.OnLobbyLogin += HandleLobbyLogin;
            networkManagerClient.OnLobbyLogout += HandleLobbyLogout;
            networkManagerClient.OnCreateLobbyGame += HandleCreateLobbyGame;
            networkManagerClient.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerClient.OnSetLobbyGameList += HandleSetLobbyGameList;
            networkManagerClient.OnSetLobbyPlayerList += HandleSetLobbyPlayerList;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnSendLobbyChatMessage -= HandleSendLobbyChatMessage;
            networkManagerClient.OnLobbyLogin += HandleLobbyLogin;
            networkManagerClient.OnLobbyLogout += HandleLobbyLogout;
            networkManagerClient.OnCreateLobbyGame -= HandleCreateLobbyGame;
            networkManagerClient.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerClient.OnSetLobbyGameList -= HandleSetLobbyGameList;
            networkManagerClient.OnSetLobbyPlayerList -= HandleSetLobbyPlayerList;

            ClearPlayerList();
            ClearLobbyGameList();
        }
    }
}
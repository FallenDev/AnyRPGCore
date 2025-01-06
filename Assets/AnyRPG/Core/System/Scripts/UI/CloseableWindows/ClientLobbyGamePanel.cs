using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class ClientLobbyGamePanel : WindowContentController {

        [Header("Client Lobby Game Panel")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        [SerializeField]
        protected TMP_InputField chatInput = null;

        [SerializeField]
        protected TextMeshProUGUI chatDisplay = null;

        [SerializeField]
        protected Image sceneImage = null;

        [SerializeField]
        protected TextMeshProUGUI sceneNameText = null;

        [SerializeField]
        protected TextMeshProUGUI sceneDescriptionText = null;

        [SerializeField]
        protected HighlightButton leaveButton = null;

        [SerializeField]
        protected HighlightButton cancelGameButton = null;

        [SerializeField]
        protected HighlightButton startGameButton = null;

        private string lobbyGameChatText = string.Empty;
        private int maxLobbyChatTextSize = 64000;

        private Dictionary<int, ClientPlayerLobbyConnectionButtonController> playerButtons = new Dictionary<int, ClientPlayerLobbyConnectionButtonController>();

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

        public void ChooseLobbyGameCharacter() {

        }

        public void StartGame() {

        }


        public void CancelLobbyGame() {
            networkManagerClient.CancelLobbyGame(networkManagerClient.LobbyGame.gameId);
        }

        public void Leave() {
            uIManager.clientLobbyGameWindow.CloseWindow();
        }

        public void SetStatusLabel() {
            serverStatusText.text = $"Logged In As: {networkManagerClient.Username}";
            SceneNode sceneNode = systemDataFactory.GetResource<SceneNode>(networkManagerClient.LobbyGame.sceneName);
            if (sceneNode == null) {
                Debug.LogWarning($"Could not find scene {networkManagerClient.LobbyGame.sceneName}");
                return;
            }
            if (sceneNode.LoadingScreenImage != null) {
                sceneImage.sprite = sceneNode.LoadingScreenImage;
                sceneImage.color = Color.white;
            } else {
                sceneImage.sprite = null;
                sceneImage.color = Color.black;
            }
            sceneNameText.text = sceneNode.DisplayName;
            sceneDescriptionText.text = sceneNode.Description;
        }

        public void HandleJoinLobbyGame(int gameId, int clientId, string userName) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                return;
            }
            AddPlayerToList(clientId, userName);
        }

        public void HandleLeaveLobbyGame(int clientId, int gameId) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                return;
            }
            RemovePlayerFromList(clientId);
        }


        public void HandleLobbyLogout(int clientId) {
            RemovePlayerFromList(clientId);
        }

        public void HandleCancelLobbyGame(int gameId) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                return;
            }
            Close();
        }

        public void SendChatMessage() {
            networkManagerClient.SendLobbyGameChatMessage(chatInput.text, networkManagerClient.LobbyGame.gameId);
        }

        public void HandleSendLobbyGameChatMessage(string messageText, int gameId) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                // this message is meant for a different lobby game and we will ignore it
                return;
            }
            lobbyGameChatText += messageText;
            lobbyGameChatText = NetworkManagerServer.ShortenStringOnNewline(lobbyGameChatText, maxLobbyChatTextSize);
            chatDisplay.text = lobbyGameChatText;
        }

        public void RequestLobbyGamePlayerList() {
            Debug.Log($"ClientLobbyPanelController.RequestLobbyPlayerList()");

            networkManagerClient.RequestLobbyPlayerList();
        }

        public void PopulatePlayerList(Dictionary<int, string> userNames) {
            Debug.Log($"ClientLobbyGamePanel.PopulatePlayerList({userNames.Count})");

            foreach (KeyValuePair<int, string> loggedInAccount in userNames) {
                AddPlayerToList(loggedInAccount.Key, loggedInAccount.Value);
            }
        }

        public void AddPlayerToList(int clientId, string userName) {
            Debug.Log($"ClientLobbyPanelController.AddPlayerToList({userName})");

            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            ClientPlayerLobbyConnectionButtonController clientPlayerLobbyConnectionButtonController = go.GetComponent<ClientPlayerLobbyConnectionButtonController>();
            clientPlayerLobbyConnectionButtonController.Configure(systemGameManager);
            clientPlayerLobbyConnectionButtonController.SetClientId(clientId, userName);
            //uINavigationControllers[1].AddActiveButton(clientPlayerLobbyConnectionButtonController.joinbu);
            playerButtons.Add(clientId, clientPlayerLobbyConnectionButtonController);
        }

        public void RemovePlayerFromList(int clientId) {
            Debug.Log($"ClientLobbyPanelController.RemovePlayerFromList({clientId})");

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
            foreach (ClientPlayerLobbyConnectionButtonController clientPlayerLobbyConnectionButtonController in playerButtons.Values) {
                if (clientPlayerLobbyConnectionButtonController.gameObject != null) {
                    clientPlayerLobbyConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientPlayerLobbyConnectionButtonController.gameObject);
                }
            }
            playerButtons.Clear();
            //uINavigationControllers[1].ClearActiveButtons();
        }



        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            SetStatusLabel();
            PopulatePlayerList(networkManagerClient.LobbyGame.PlayerList);
            networkManagerClient.OnSendLobbyGameChatMessage += HandleSendLobbyGameChatMessage;
            networkManagerClient.OnJoinLobbyGame += HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame += HandleLeaveLobbyGame;
            networkManagerClient.OnCancelLobbyGame += HandleCancelLobbyGame;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnSendLobbyGameChatMessage -= HandleSendLobbyGameChatMessage;
            networkManagerClient.OnJoinLobbyGame -= HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame -= HandleLeaveLobbyGame;
            networkManagerClient.OnCancelLobbyGame -= HandleCancelLobbyGame;

            ClearPlayerList();
        }
    }
}
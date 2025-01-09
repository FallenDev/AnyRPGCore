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
        protected Image characterImage = null;

        [SerializeField]
        protected TextMeshProUGUI characterNameText = null;

        [SerializeField]
        protected TextMeshProUGUI characterDescriptionText = null;

        [SerializeField]
        protected HighlightButton leaveButton = null;

        [SerializeField]
        protected HighlightButton cancelGameButton = null;

        [SerializeField]
        protected HighlightButton readyButton = null;

        [SerializeField]
        protected TextMeshProUGUI readyButtonText = null;

        [SerializeField]
        protected HighlightButton startGameButton = null;

        private string lobbyGameChatText = string.Empty;
        private int maxLobbyChatTextSize = 64000;

        private UnitProfile unitProfile = null;

        private Dictionary<int, ClientPlayerLobbyGameConnectionButton> playerButtons = new Dictionary<int, ClientPlayerLobbyGameConnectionButton>();

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
            uIManager.newGameWindow.OpenWindow();
        }

        public void StartGame() {
            networkManagerClient.StartLobbyGame(networkManagerClient.LobbyGame.gameId);
        }


        public void CancelLobbyGame() {
            networkManagerClient.CancelLobbyGame(networkManagerClient.LobbyGame.gameId);
        }

        public void ToggleReady() {
            networkManagerClient.ToggleLobbyGameReadyStatus(networkManagerClient.LobbyGame.gameId);
        }

        public void Leave() {
            networkManagerClient.LeaveLobbyGame(networkManagerClient.LobbyGame.gameId);
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
            AddPlayerToList(clientId, userName, string.Empty);
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

        public void PopulatePlayerList(Dictionary<int, LobbyGamePlayerInfo> userNames) {
            Debug.Log($"ClientLobbyGamePanel.PopulatePlayerList({userNames.Count})");

            foreach (KeyValuePair<int, LobbyGamePlayerInfo> loggedInAccount in userNames) {
                AddPlayerToList(loggedInAccount.Key, loggedInAccount.Value.userName, loggedInAccount.Value.unitProfileName);
            }
        }

        public void AddPlayerToList(int clientId, string userName, string unitProfileName) {
            Debug.Log($"ClientLobbyGamePanel.AddPlayerToList({clientId}, {userName})");

            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            ClientPlayerLobbyGameConnectionButton clientPlayerLobbyGameConnectionButton = go.GetComponent<ClientPlayerLobbyGameConnectionButton>();
            clientPlayerLobbyGameConnectionButton.Configure(systemGameManager);
            if (networkManagerClient.LobbyGame.leaderClientId == clientId) {
                clientPlayerLobbyGameConnectionButton.SetClientId(clientId, $"{userName} (leader)", unitProfileName);
            } else {
                clientPlayerLobbyGameConnectionButton.SetClientId(clientId, userName, unitProfileName);
            }
            //uINavigationControllers[1].AddActiveButton(clientPlayerLobbyGameConnectionButton.joinbu);
            playerButtons.Add(clientId, clientPlayerLobbyGameConnectionButton);
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
            foreach (ClientPlayerLobbyGameConnectionButton clientPlayerLobbyGameConnectionButton in playerButtons.Values) {
                if (clientPlayerLobbyGameConnectionButton.gameObject != null) {
                    clientPlayerLobbyGameConnectionButton.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientPlayerLobbyGameConnectionButton.gameObject);
                }
            }
            playerButtons.Clear();
            //uINavigationControllers[1].ClearActiveButtons();
        }

        public void HandleChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName) {
            
            if (clientId == networkManagerClient.ClientId) {
                unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (unitProfile != null) {
                    characterImage.sprite = unitProfile.Icon;
                    characterImage.color = Color.white;
                    characterNameText.text = unitProfile.DisplayName;
                    characterDescriptionText.text = unitProfile.Description;
                    readyButton.Button.interactable = true;
                } else {
                    characterImage.sprite = null;
                    characterImage.color = Color.black;
                }
            }
            playerButtons[clientId].SetUnitProfileName(unitProfileName);
        }

        public void HandleSetLobbyGameReadyStatus(int gameId, int clientId, bool ready) {
            Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameReadyStatus({gameId}, {clientId}, {ready})");

            if (networkManagerClient.LobbyGame.gameId != gameId) {
                return;
            }

            if (clientId == networkManagerClient.ClientId) {
                if (ready) {
                    readyButtonText.text = "Not Ready";
                } else {
                    readyButtonText.text = "Ready";
                }
            }
            
            playerButtons[clientId].SetReadyStatus(ready);

            if (networkManagerClient.ClientId == networkManagerClient.LobbyGame.leaderClientId) {
                // check if the start button can be made interactable
                if (AllPlayersReady()) {
                    startGameButton.Button.interactable = true;
                } else {
                    startGameButton.Button.interactable = false;

                }
            }
        }

        private bool AllPlayersReady() {
            foreach (LobbyGamePlayerInfo lobbyGamePlayerInfo in networkManagerClient.LobbyGame.PlayerList.Values) {
                if (lobbyGamePlayerInfo.ready == false) {
                    return false;
                }
            }
            return true;
        }


        public void UpdateNavigationButtons() {
            
            // hide the cancel game button for anyone other than the leader
            if (networkManagerClient.ClientId == networkManagerClient.LobbyGame.leaderClientId) {
                cancelGameButton.gameObject.SetActive(true);
                startGameButton.gameObject.SetActive(true);
                startGameButton.Button.interactable = false;
            } else {
                cancelGameButton.gameObject.SetActive(false);
                startGameButton.gameObject.SetActive(false);
            }
            readyButton.Button.interactable = false;
            readyButtonText.text = "Ready";

            uINavigationControllers[0].UpdateNavigationList();
        }


        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            SetStatusLabel();
            PopulatePlayerList(networkManagerClient.LobbyGame.PlayerList);
            UpdateNavigationButtons();
            networkManagerClient.OnSendLobbyGameChatMessage += HandleSendLobbyGameChatMessage;
            networkManagerClient.OnJoinLobbyGame += HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame += HandleLeaveLobbyGame;
            networkManagerClient.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerClient.OnChooseLobbyGameCharacter += HandleChooseLobbyGameCharacter;
            networkManagerClient.OnSetLobbyGameReadyStatus += HandleSetLobbyGameReadyStatus;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnSendLobbyGameChatMessage -= HandleSendLobbyGameChatMessage;
            networkManagerClient.OnJoinLobbyGame -= HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame -= HandleLeaveLobbyGame;
            networkManagerClient.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerClient.OnChooseLobbyGameCharacter -= HandleChooseLobbyGameCharacter;
            networkManagerClient.OnSetLobbyGameReadyStatus -= HandleSetLobbyGameReadyStatus;

            ClearPlayerList();
        }
    }
}
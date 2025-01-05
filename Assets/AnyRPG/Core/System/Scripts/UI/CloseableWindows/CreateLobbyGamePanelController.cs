using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class CreateLobbyGamePanelController : WindowContentController {

        [Header("CreateLobbyGamePanelController")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        //[SerializeField]
        //protected HighlightButton returnButton = null;

        [SerializeField]
        protected HighlightButton createGameButton = null;

        [SerializeField]
        protected HighlightButton cancelGameButton = null;

        [SerializeField]
        protected HighlightButton startGameButton = null;

        private Dictionary<int, LobbyGameConnectionButtonController> playerButtons = new Dictionary<int, LobbyGameConnectionButtonController>();

        private LobbyGame lobbyGame = null;

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            cancelGameButton.Button.interactable = false;
            startGameButton.Button.interactable = false;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }


        public void PopulatePlayerList() {
            Debug.Log($"CreateLobbyGamePanelController.PopulatePlayerList()");

            foreach (KeyValuePair<int, LoggedInAccount> loggedInAccount in networkManagerClient.LobbyGamePlayerList) {
                AddPlayerToList(loggedInAccount.Value.clientId, loggedInAccount.Value.username);
            }
        }

        public void AddPlayerToList(int clientId, string userName) {
            Debug.Log($"CreateLobbyGamePanelController.AddPlayerToList({userName})");

            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            LobbyGameConnectionButtonController lobbyGameConnectionButtonController = go.GetComponent<LobbyGameConnectionButtonController>();
            lobbyGameConnectionButtonController.Configure(systemGameManager);
            lobbyGameConnectionButtonController.SetClientId(clientId, userName);
            uINavigationControllers[1].AddActiveButton(lobbyGameConnectionButtonController.KickButton);
            playerButtons.Add(clientId, lobbyGameConnectionButtonController);
        }

        public void RemovePlayerFromList(int clientId) {
            Debug.Log($"CreateLobbyGamePanelController.RemovePlayerFromList({clientId})");

            if (playerButtons.ContainsKey(clientId)) {
                uINavigationControllers[1].ClearActiveButton(playerButtons[clientId].KickButton);
                if (playerButtons[clientId].gameObject != null) {
                    playerButtons[clientId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerButtons[clientId].gameObject);
                }
            }
        }

        public void ClearPlayerList() {

            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (LobbyGameConnectionButtonController lobbyGameConnectionButtonController in playerButtons.Values) {
                if (lobbyGameConnectionButtonController.gameObject != null) {
                    lobbyGameConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(lobbyGameConnectionButtonController.gameObject);
                }
            }
            playerButtons.Clear();
            uINavigationControllers[1].ClearActiveButtons();
        }

        public void CloseMenu() {
            uIManager.hostServerWindow.CloseWindow();
        }

        public void CreateGame() {
            Debug.Log($"CreateLobbyGamePanelController.CreateGame()");

            networkManagerClient.CreateLobbyGame();
        }

        public void CancelGame() {
            Debug.Log($"CreateLobbyGamePanelController.CancelGame()");

            networkManagerClient.CancelLobbyGame(lobbyGame.GameId);
        }

        public void StartGame() {
            Debug.Log($"CreateLobbyGamePanelController.StartGame()");
        }

        public void HandleCreateLobbyGame(LobbyGame lobbyGame) {
            if (lobbyGame.leaderClientId != networkManagerClient.GetClientId()) {
                // the game was not created by this player
                return;
            }
            this.lobbyGame = lobbyGame;
            serverStatusText.text = "Game Status: Accepting Players";
            createGameButton.Button.interactable = false;
            cancelGameButton.Button.interactable = true;
            startGameButton.Button.interactable = true;
        }

        public void HandleCancelLobbyGame(int gameId) {
            if (lobbyGame.leaderClientId != networkManagerClient.GetClientId()) {
                // the game was not created by this player
                return;
            }
            serverStatusText.text = "Game Status: Not Yet Created";
            createGameButton.Button.interactable = true;
            cancelGameButton.Button.interactable = false;
            startGameButton.Button.interactable = false;
        }

        public void HandleClientJoinLobbyGame(int gameId, int clientId) {
            Debug.Log($"CreateLobbyGamePanelController.HandleClientJoinLobbyGame({clientId})");

            AddPlayerToList(clientId, networkManagerClient.LobbyGamePlayerList[clientId].username);
        }

        public void HandleClientLeaveLobbyGame(int gameId, int clientId) {
            Debug.Log($"CreateLobbyGamePanelController.HandleClientLeaveLobbyGame({clientId})");
            
            RemovePlayerFromList(clientId);
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            PopulatePlayerList();
            networkManagerClient.OnCreateLobbyGame += HandleCreateLobbyGame;
            networkManagerClient.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerClient.OnClientJoinLobbyGame += HandleClientJoinLobbyGame;
            networkManagerClient.OnClientLeaveLobbyGame += HandleClientLeaveLobbyGame;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnCreateLobbyGame -= HandleCreateLobbyGame;
            networkManagerClient.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerClient.OnClientJoinLobbyGame -= HandleClientJoinLobbyGame;
            networkManagerClient.OnClientLeaveLobbyGame -= HandleClientLeaveLobbyGame;
            ClearPlayerList();
        }
    }
}
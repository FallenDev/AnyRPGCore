using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class ClientLobbyPanelController : WindowContentController {

        [Header("ClientLobbyPanelController")]

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
        protected HighlightButton joinGameButton = null;

        protected Dictionary<string, List<CreditsNode>> categoriesDictionary = new Dictionary<string, List<CreditsNode>>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            PopulatePlayerList();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }


        public void PopulatePlayerList() {

            foreach (KeyValuePair<int, string> loggedInAccount in networkManagerServer.LoggedInAccounts) {
                GameObject go = null;
                go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
                PlayerConnectionButtonController playerConnectionButtonController = go.GetComponent<PlayerConnectionButtonController>();
                playerConnectionButtonController.PlayerNameText.text = loggedInAccount.Value;
                playerConnectionButtonController.PlayerInfoText.text = "10.10.10.11";
                uINavigationControllers[1].AddActiveButton(playerConnectionButtonController.NameHighlightButton);
                uINavigationControllers[1].AddActiveButton(playerConnectionButtonController.AttributionHighlightButton);
                playerConnectionButtonController.NameHighlightButton.Configure(systemGameManager);
                playerConnectionButtonController.AttributionHighlightButton.Configure(systemGameManager);
            }
        }

        public void Logout() {
            networkManagerClient.Logout();
            uIManager.clientLobbyWindow.CloseWindow();
        }

        public void CreateGame() {
            Debug.Log($"ClientLobbyPanelController.CreateGame()");

        }

        public void JoinGame() {
            Debug.Log($"ClientLobbyPanelController.JoinGame()");

        }

        public void SetStatusLabel() {
            serverStatusText.text = $"Logged In As: a guy";
        }

        public void HandleLobbyLogin() {
            PopulatePlayerList();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            PopulatePlayerList();
            //networkManagerServer.OnStartServer += SetStartServerLabel;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            //networkManagerServer.OnStartServer -= SetStartServerLabel;
        }
    }
}
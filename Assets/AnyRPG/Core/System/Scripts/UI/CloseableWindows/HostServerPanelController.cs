using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class HostServerPanelController : WindowContentController {

        [Header("HostGamePanelController")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        //[SerializeField]
        //protected HighlightButton returnButton = null;

        [SerializeField]
        protected HighlightButton startServerButton = null;

        [SerializeField]
        protected HighlightButton stopServerButton = null;

        protected Dictionary<string, List<CreditsNode>> categoriesDictionary = new Dictionary<string, List<CreditsNode>>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;

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
        }


        public void PopulatePlayerList() {

            foreach (string categoryName in categoriesDictionary.Keys) {
                GameObject go = null;
                go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
                PlayerConnectionButtonController playerConnectionButtonController = go.GetComponent<PlayerConnectionButtonController>();
                playerConnectionButtonController.PlayerNameText.text = "Awesome player";
                playerConnectionButtonController.PlayerInfoText.text = "10.10.10.11";
                uINavigationControllers[1].AddActiveButton(playerConnectionButtonController.NameHighlightButton);
                uINavigationControllers[1].AddActiveButton(playerConnectionButtonController.AttributionHighlightButton);
                playerConnectionButtonController.NameHighlightButton.Configure(systemGameManager);
                playerConnectionButtonController.AttributionHighlightButton.Configure(systemGameManager);
            }
        }

        public void CloseMenu() {
            uIManager.hostServerWindow.CloseWindow();
        }

        public void StartServer() {
            Debug.Log($"HostServerPanelController.StartServer()");

            networkManagerServer.StartServer();
        }

        public void StopServer() {
            Debug.Log($"HostServerPanelController.StopServer()");

            networkManagerServer.StopServer();
        }

        public void SetStartServerLabel() {
            serverStatusText.text = "Server Status: Online";
        }

        public void SetStopServerLabel() {
            serverStatusText.text = "Server Status: Offline";
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            networkManagerServer.OnStartServer += SetStartServerLabel;
            networkManagerServer.OnStopServer += SetStopServerLabel;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerServer.OnStartServer -= SetStartServerLabel;
            networkManagerServer.OnStopServer -= SetStopServerLabel;
        }
    }
}
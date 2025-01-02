using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayOnlineMenuController : WindowContentController {

        /*
        [SerializeField]
        private HighlightButton continueButton = null;

        [SerializeField]
        private HighlightButton newGameButton = null;

        [SerializeField]
        private HighlightButton loadGameButton = null;
        */

        // game manager references
        protected UIManager uIManager = null;
        protected SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            
            /*
            continueButton.Configure(systemGameManager);
            newGameButton.Configure(systemGameManager);
            loadGameButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
        }

        public void HostServer() {
            //Debug.Log("PlayOnlineMenuController.NewGame()");
            uIManager.playOnlineMenuWindow.CloseWindow();
            uIManager.hostServerWindow.OpenWindow();
        }

        public void JoinServer() {
            //Debug.Log("PlayOnlineMenuController.JoinServer()");
            uIManager.playOnlineMenuWindow.CloseWindow();
            uIManager.networkLoginWindow.OpenWindow();
        }

    }

}
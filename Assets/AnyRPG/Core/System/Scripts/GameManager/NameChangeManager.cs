using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeManager : InteractableOptionManager {

        // game manager references
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void RequestChangePlayerName(string newName) {
            //Debug.Log($"NameChangeManager.RequestChangePlayerName({newName})");

            if (systemGameManager.GameMode == GameMode.Local) {
                playerManagerServer.SetPlayerName(playerManager.UnitController, newName);
            } else {
                networkManagerClient.RequestChangePlayerName(newName);
            }

            ConfirmAction(playerManager.UnitController);
        }

    }

}
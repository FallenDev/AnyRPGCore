using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogController : ConfiguredClass {

        // references
        private Interactable interactable;

        private int dialogIndex = 0;

        private float maxDialogTime = 300f;
        private float chatDisplayTime = 5f;

        private Coroutine dialogCoroutine = null;

        // game manager references
        private PlayerManager playerManager = null;
        private LogManager logManager = null;
        private NetworkManagerServer networkManagerServer = null;

        public int DialogIndex { get => dialogIndex; }

        public DialogController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void Cleanup() {
            CleanupDialog();
        }

        private void CleanupDialog() {
            if (dialogCoroutine != null) {
                interactable.StopCoroutine(dialogCoroutine);
                interactable.ProcessEndDialog();
                dialogCoroutine = null;
            }
        }

        public void BeginDialog(UnitController sourceUnitController, string dialogName, DialogComponent caller = null) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.BeginDialog(" + dialogName + ")");
            Dialog tmpDialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (tmpDialog != null) {
                BeginDialog(sourceUnitController, tmpDialog, caller);
            }
        }

        public void BeginDialog(UnitController sourceUnitController, Dialog dialog, DialogComponent caller = null) {
            Debug.Log($"{interactable.gameObject.name}.DialogController.BeginDialog({sourceUnitController.gameObject.name}, {dialog.ResourceName})");

            if (dialog != null && dialogCoroutine == null) {
                dialogCoroutine = interactable.StartCoroutine(PlayDialog(sourceUnitController, dialog, caller));
            }
        }

        public void BeginChatMessage(string messageText) {
            CleanupDialog();
            dialogCoroutine = interactable.StartCoroutine(PlayChatMessage(messageText));
        }

        public IEnumerator PlayChatMessage(string messageText) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.PlayDialog(" + dialog.DisplayName + ")");

            interactable.ProcessBeginDialog();

            interactable.ProcessDialogTextUpdate(messageText);

            yield return new WaitForSeconds(chatDisplayTime);
            interactable.ProcessEndDialog();
        }

        public IEnumerator PlayDialog(UnitController sourceUnitController, Dialog dialog, DialogComponent caller = null) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.PlayDialog(" + dialog.DisplayName + ")");

            interactable.ProcessBeginDialog();
            float elapsedTime = 0f;
            dialogIndex = 0;
            DialogNode currentdialogNode = null;

            // this needs to be reset to allow for repeatable dialogs to replay
            dialog.ResetStatus(sourceUnitController);

            while (dialog.TurnedIn(sourceUnitController) == false) {
                foreach (DialogNode dialogNode in dialog.DialogNodes) {
                    if (dialogNode.StartTime <= elapsedTime && dialogNode.Shown(sourceUnitController, dialog, dialogIndex) == false) {
                        currentdialogNode = dialogNode;
                        PlayDialogNode(dialogNode);
                        interactable.InteractableEventController.NotifyOnPlayDialogNode(dialog, dialogIndex);
                        dialogNode.SetShown(sourceUnitController, dialog, true, dialogIndex);
                        dialogIndex++;
                    }
                }
                if (dialogIndex >= dialog.DialogNodes.Count) {
                    dialog.SetTurnedIn(sourceUnitController, true);
                    if (caller != null) {
                        caller.NotifyOnConfirmAction(sourceUnitController);
                    }
                }
                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxDialogTime) {
                    break;
                }
                yield return null;
                dialogCoroutine = null;
            }

            if (currentdialogNode != null) {
                yield return new WaitForSeconds(currentdialogNode.ShowTime);
            }
            interactable.ProcessEndDialog();
        }

        public void PlayDialogNode(string dialogName, int dialogIndex) {
            Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialogNode({dialogName}, {dialogIndex})");

            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (dialog != null && dialog.DialogNodes.Count > dialogIndex) {
                PlayDialogNode(dialog.DialogNodes[dialogIndex]);
            }
        }

        public void PlayDialogNode(DialogNode dialogNode) {
            Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialogNode()");

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }
            //bool writeMessage = true;
            if (playerManager != null && playerManager.ActiveUnitController != null) {
                if (Vector3.Distance(interactable.transform.position, playerManager.ActiveUnitController.transform.position) > systemConfigurationManager.MaxChatTextDistance) {
                    //writeMessage = false;
                    return;
                }
            }
            if (logManager != null) {
                logManager.WriteChatMessageClient($"{interactable.DisplayName}: {dialogNode.Description}");
            }
            interactable.ProcessDialogTextUpdate(dialogNode.Description);
            if (dialogNode.AudioClip != null) {
                interactable.UnitComponentController.PlayVoiceSound(dialogNode.AudioClip);
            }
        }


    }

}
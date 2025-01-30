using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogComponent : InteractableOptionComponent {

        // game manager references
        private DialogManager dialogManager = null;

        public DialogProps Props { get => interactableOptionProps as DialogProps; }

        public DialogComponent(Interactable interactable, DialogProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            dialogManager = systemGameManager.DialogManager;
        }

        public override void Cleanup() {
            base.Cleanup();
            CleanupPrerequisiteOwner();
        }

        public void CleanupPrerequisiteOwner() {
            Props.CleanupPrerequisiteOwner(this);
        }

        public override void NotifyOnConfirmAction(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.NameChangeInteractable.HandleConfirmAction()");
            base.NotifyOnConfirmAction(sourceUnitController);

            // since the dialog completion status is itself a form of prerequisite, we should call the prerequisite update here
            HandleOptionStateChange();
        }

        public List<Dialog> GetCurrentOptionList(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.DialogInteractable.GetCurrentOptionList()");
            List<Dialog> currentList = new List<Dialog>();
            if (interactable.CombatOnly == false) {
                foreach (Dialog dialog in Props.DialogList) {
                    //Debug.Log(interactable.gameObject.name + ".DialogInteractable.GetCurrentOptionList() : found dialog: " + dialog.DisplayName);
                    if (dialog.PrerequisitesMet(sourceUnitController) == true && (dialog.TurnedIn(sourceUnitController) == false || dialog.Repeatable == true)) {
                        currentList.Add(dialog);
                    }
                }
            }
            //Debug.Log($"{gameObject.name}.DialogInteractable.GetCurrentOptionList(): List Size: " + currentList.Count);
            return currentList;
        }

        public override bool Interact(UnitController sourceUnitController, int optionIndex) {
            //Debug.Log(interactable.gameObject.name + ".DialogInteractable.Interact()");
            base.Interact(sourceUnitController, optionIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int optionIndex) {
            base.ClientInteraction(sourceUnitController, optionIndex);
            
            List<Dialog> currentList = GetCurrentOptionList(sourceUnitController);
            if (currentList.Count == 0) {
                return;
            } else /*if (currentList.Count == 1)*/ {
                if (currentList[optionIndex].Automatic) {
                    interactable.DialogController.BeginDialog(sourceUnitController, currentList[optionIndex]);
                } else {
                    dialogManager.SetDialog(currentList[optionIndex], this.interactable, this, optionIndex);
                    uIManager.dialogWindow.OpenWindow();
                }
            }/* else {
                interactable.OpenInteractionWindow();
            }*/

        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck = false, bool passedRangeCheck = false, bool processNonCombatCheck = true) {
            //Debug.Log($"{gameObject.name}.DialogInteractable.CanInteract()");
            if (!base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck)) {
                return false;
            }
            if (GetCurrentOptionList(sourceUnitController).Count == 0) {
                return false;
            }
            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.dialogWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.DialogInteractable.GetCurrentOptionCount(): " + GetCurrentOptionList().Count);
            return GetCurrentOptionList(sourceUnitController).Count;
        }

        public override void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            UpdateDialogStatuses(sourceUnitController);
            base.HandlePlayerUnitSpawn(sourceUnitController);
        }

        public void UpdateDialogStatuses(UnitController sourceUnitController) {
            foreach (Dialog dialog in Props.DialogList) {
                dialog.UpdatePrerequisites(sourceUnitController, false);
            }

            bool preRequisitesUpdated = false;
            foreach (Dialog dialog in Props.DialogList) {
                if (dialog.PrerequisitesMet(sourceUnitController) == true) {
                    preRequisitesUpdated = true;
                }
            }

            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates(sourceUnitController);
            }

        }

    }

}
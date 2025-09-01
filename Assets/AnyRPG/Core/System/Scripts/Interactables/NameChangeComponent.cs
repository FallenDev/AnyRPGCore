using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeComponent : InteractableOptionComponent {

        // game manager references
        NameChangeManagerClient nameChangeManager = null;

        public NameChangeProps Props { get => interactableOptionProps as NameChangeProps; }

        public NameChangeComponent(Interactable interactable, NameChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            nameChangeManager = systemGameManager.NameChangeManagerClient;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.NameChangeInteractable.Interact()");
            
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            nameChangeManager.SetProps(this, componentIndex, choiceIndex);
            uIManager.nameChangeWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.nameChangeWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".NameChangeInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
            return GetValidOptionCount(sourceUnitController);
        }

        public void SetPlayerName(UnitController sourceUnitController, string newName) {
            if (newName != null && newName != string.Empty) {
                sourceUnitController.BaseCharacter.ChangeCharacterName(newName);
            }

            NotifyOnConfirmAction(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}

    }

}
using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeComponent : InteractableOptionComponent {

        // game manager references
        NameChangeManager nameChangeManager = null;

        public NameChangeProps Props { get => interactableOptionProps as NameChangeProps; }

        public NameChangeComponent(Interactable interactable, NameChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            nameChangeManager = systemGameManager.NameChangeManager;
        }

        public override bool Interact(UnitController source, int optionIndex) {
            //Debug.Log($"{gameObject.name}.NameChangeInteractable.Interact()");
            
            base.Interact(source, optionIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int optionIndex) {
            base.ClientInteraction(sourceUnitController, optionIndex);
            nameChangeManager.BeginInteraction(this, optionIndex);
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

        //public override bool PlayInteractionSound() {
        //    return true;
        //}

    }

}
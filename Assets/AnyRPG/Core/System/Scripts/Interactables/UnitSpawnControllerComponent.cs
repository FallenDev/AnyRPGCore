using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnControllerComponent : InteractableOptionComponent {

        // game manager references
        private UnitSpawnManager unitSpawnManager = null;

        public UnitSpawnControllerProps Props { get => interactableOptionProps as UnitSpawnControllerProps; }

        public UnitSpawnControllerComponent(Interactable interactable, UnitSpawnControllerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Spawn Characters";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            unitSpawnManager = systemGameManager.UnitSpawnManager;
        }

        public override bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.Interact(sourceUnitController, componentIndex, choiceIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            unitSpawnManager.SetProps(Props, this, componentIndex, choiceIndex);
            uIManager.unitSpawnWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.unitSpawnWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

    }

}
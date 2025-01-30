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
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Spawn Characters";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            unitSpawnManager = systemGameManager.UnitSpawnManager;
        }

        public override bool Interact(UnitController source, int optionIndex) {
            base.Interact(source, optionIndex);
            unitSpawnManager.SetProps(Props, this, optionIndex);
            uIManager.unitSpawnWindow.OpenWindow();
            return true;
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
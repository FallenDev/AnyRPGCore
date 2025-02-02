using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TeleportComponent : PortalComponent {

        public TeleportProps TeleportProps { get => interactableOptionProps as TeleportProps; }

        public TeleportComponent(Interactable interactable, TeleportProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.PortalInteractable.Interact()");
            base.Interact(sourceUnitController, componentIndex, choiceIndex);

            sourceUnitController.CharacterAbilityManager.BeginAbility(TeleportProps.BaseAbility.AbilityProperties);
            return true;
        }


    }
}
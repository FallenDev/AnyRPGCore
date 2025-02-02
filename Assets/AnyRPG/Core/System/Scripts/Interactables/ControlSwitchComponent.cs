using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ControlSwitchComponent : InteractableOptionComponent {

        public ControlSwitchProps Props { get => interactableOptionProps as ControlSwitchProps; }

        // keep track of the number of times this switch has been activated
        protected int activationCount = 0;

        // can be on or off
        protected bool onState = false;

        public bool OnState { get => onState; set => onState = value; }

        public ControlSwitchComponent(Interactable interactable, ControlSwitchProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Interactable";
        }

        public override bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log(interactable.gameObject.name + ".ControlSwitchComponent.Interact()");
            if (Props.ActivationLimit > 0 && activationCount >= Props.ActivationLimit) {
                // this has already been activated the number of allowed times
                return false;
            }
            activationCount++;
            if (Props.SwitchGroup != null && Props.SwitchGroup.Count > 0) {
                int activeSwitches = 0;
                foreach (ControlSwitchComponent controlSwitch in Props.SwitchGroup) {
                    if (controlSwitch.OnState) {
                        activeSwitches++;
                    }
                }
                if (onState == false && activeSwitches < Props.SwitchGroup.Count) {
                    return false;
                } else if (onState == true && activeSwitches >= Props.SwitchGroup.Count) {
                    return false;
                }

            }
            onState = !onState;
            base.Interact(sourceUnitController, componentIndex, choiceIndex);

            if (Props.ControlObjects != null) {
                foreach (InteractableOptionComponent interactableOption in Props.ControlObjects) {
                    interactableOption.Interact(sourceUnitController, componentIndex, choiceIndex);
                }
            }
            

            return false;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.interactionWindow.CloseWindow();
        }

    }

}
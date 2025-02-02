using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AnimatedObjectComponent : InteractableOptionComponent {

        public AnimatedObjectProps Props { get => interactableOptionProps as AnimatedObjectProps; }

        // by default it is considered closed when not using the sheathed position
        private bool objectOpen = false;

        public AnimatedObjectComponent(Interactable interactable, AnimatedObjectProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Interactable";
        }

        public override bool CanInteract(UnitController source, bool processRangeCheck = false, bool passedRangeCheck = false, bool processNonCombatCheck = true) {

            if (Props.SwitchOnly == true) {
                return false;
            }
            return base.CanInteract(source, processRangeCheck, passedRangeCheck, processNonCombatCheck);
        }

        public override bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            base.Interact(sourceUnitController, componentIndex, choiceIndex);

            if (Props.AnimationComponent == null) {
                Debug.Log("AnimatedObjectComponent.Interact(): Animation component was null");
                return false;
            }
            ChooseMovement(sourceUnitController, componentIndex);

            return false;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            uIManager.interactionWindow.CloseWindow();
        }

        public void ChooseMovement(UnitController sourceUnitController, int componentIndex) {
            //interactable.InteractableEventController.NotifyOnAnimatedObjectChooseMovement(sourceUnitController, optionIndex);
            if (objectOpen) {
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    Props.AnimationComponent.Play(Props.CloseAnimationClip.name);
                }
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                    if (Props.OpenAudioClip != null) {
                        interactable.UnitComponentController.PlayEffectSound(Props.CloseAudioClip);
                    }
                }
                objectOpen = false;
            } else {
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    Props.AnimationComponent.Play(Props.OpenAnimationClip.name);
                }
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                    if (Props.CloseAudioClip != null) {
                        interactable.UnitComponentController.PlayEffectSound(Props.OpenAudioClip);
                    }
                }
                objectOpen = true;
            }
        }


    }

}
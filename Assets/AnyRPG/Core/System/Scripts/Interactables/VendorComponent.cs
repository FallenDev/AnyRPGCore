using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        // game manager references
        private VendorManager vendorManager = null;

        public VendorProps Props { get => interactableOptionProps as VendorProps; }

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Purchase Items";
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            vendorManager = systemGameManager.VendorManager;
        }

        /*
        protected override void AddUnitProfileSettings() {
            if (unitProfile != null) {
                interactableOptionProps = unitProfile.VendorProps;
            }
            HandlePrerequisiteUpdates();
        }
        */

        public override bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.Interact(sourceUnitController, componentIndex, choiceIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            if (!uIManager.vendorWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);

                vendorManager.SetProps(Props, this, componentIndex, choiceIndex);
                uIManager.vendorWindow.OpenWindow();
            }
        }


        public override void StopInteract() {
            base.StopInteract();
            uIManager.vendorWindow.CloseWindow();
        }

        public override bool PlayInteractionSound() {
            return true;
        }

        public override AudioClip GetInteractionSound(VoiceProps voiceProps) {
            return voiceProps.RandomStartVendorInteract;
        }


    }

}
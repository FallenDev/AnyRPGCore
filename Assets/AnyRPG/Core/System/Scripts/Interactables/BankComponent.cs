using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class BankComponent : InteractableOptionComponent {

        public BankProps Props { get => interactableOptionProps as BankProps; }

        public BankComponent(Interactable interactable, BankProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Bank";
        }

        public override bool Interact(UnitController source, int optionIndex) {
            //Debug.Log($"{gameObject.name}.Bank.Interact(" + (source == null ? "null" : source.name) +")");
            base.Interact(source, optionIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int optionIndex) {
            base.ClientInteraction(sourceUnitController, optionIndex);
            uIManager.interactionWindow.CloseWindow();
            if (!uIManager.bankWindow.IsOpen) {
                uIManager.bankWindow.OpenWindow();
            }
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.bankWindow.CloseWindow();
        }

        public override bool PlayInteractionSound() {
            return true;
        }

    }

}
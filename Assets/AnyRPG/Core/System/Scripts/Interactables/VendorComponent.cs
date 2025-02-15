using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        private VendorCollection buyBackCollection = null;

        // game manager references
        private VendorManager vendorManager = null;
        private MessageFeedManager messageFeedManager = null;
        private CurrencyConverter currencyConverter = null;

        public VendorProps Props { get => interactableOptionProps as VendorProps; }
        public VendorCollection BuyBackCollection { get => buyBackCollection; set => buyBackCollection = value; }

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactionPanelTitle = "Purchase Items";
            buyBackCollection = ScriptableObject.CreateInstance(typeof(VendorCollection)) as VendorCollection;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            vendorManager = systemGameManager.VendorManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            currencyConverter = systemGameManager.CurrencyConverter;
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

        public void AddToBuyBackCollection(InstantiatedItem newInstantiatedItem) {
            VendorItem newVendorItem = new VendorItem();
            newVendorItem.Quantity = 1;
            newVendorItem.InstantiatedItem = newInstantiatedItem;
            vendorManager.VendorComponent.BuyBackCollection.VendorItems.Add(newVendorItem);
        }


        public bool SellItem(UnitController sourceUnitController, InstantiatedItem instantiatedItem) {
            if (instantiatedItem.Item.BuyPrice(sourceUnitController) <= 0 || instantiatedItem.Item.GetSellPrice(instantiatedItem, sourceUnitController).Key == null) {
                messageFeedManager.WriteMessage(sourceUnitController, $"The vendor does not want to buy the {instantiatedItem.DisplayName}");
                return false;
            }
            KeyValuePair<Currency, int> sellAmount = instantiatedItem.Item.GetSellPrice(instantiatedItem, sourceUnitController);

            sourceUnitController.CharacterCurrencyManager.AddCurrency(sellAmount.Key, sellAmount.Value);
            AddToBuyBackCollection(instantiatedItem);
            instantiatedItem.Slot.RemoveItem(instantiatedItem);

            string priceString = currencyConverter.GetCombinedPriceString(sellAmount.Key, sellAmount.Value);
            messageFeedManager.WriteMessage(sourceUnitController, $"Sold {instantiatedItem.DisplayName} for {priceString}");

            return true;
        }
    }

}
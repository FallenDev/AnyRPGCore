using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorManager : InteractableOptionManager {

        private VendorProps vendorProps = null;
        private VendorComponent vendorComponent = null;

        public VendorProps VendorProps { get => vendorProps; set => vendorProps = value; }
        public VendorComponent VendorComponent { get => vendorComponent; set => vendorComponent = value; }

        public void SetProps(VendorProps vendorProps, VendorComponent vendorComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.vendorProps = vendorProps;
            this.vendorComponent = vendorComponent;
            BeginInteraction(vendorComponent, componentIndex, choiceIndex);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            vendorProps = null;
        }

        public void SellItem(UnitController sourceUnitController, InstantiatedItem instantiatedItem) {
            if (systemGameManager.GameMode == GameMode.Local) {
                vendorComponent.SellItem(sourceUnitController, instantiatedItem);
            } else {
                networkManagerClient.SellVendorItem(vendorComponent.Interactable, componentIndex, instantiatedItem.InstanceId);
            }
        }

        public void SellItemServer(UnitController sourceUnitController, Interactable interactable, int optionIndex, InstantiatedItem instantiatedItem) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[optionIndex] is VendorComponent) {
                (currentInteractables[optionIndex] as VendorComponent).SellItem(sourceUnitController, instantiatedItem);
            }
        }
    }

}
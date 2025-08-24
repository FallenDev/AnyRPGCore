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

        public void SellItemToVendor(UnitController sourceUnitController, InstantiatedItem instantiatedItem) {
            if (systemGameManager.GameMode == GameMode.Local) {
                vendorComponent.SellItemToVendor(sourceUnitController, componentIndex, instantiatedItem);
            } else {
                networkManagerClient.SellItemToVendor(vendorComponent.Interactable, componentIndex, instantiatedItem.InstanceId);
            }
        }

        public void SellItemToVendorServer(UnitController sourceUnitController, Interactable interactable, int componentIndex, InstantiatedItem instantiatedItem) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                (currentInteractables[componentIndex] as VendorComponent).SellItemToVendor(sourceUnitController, componentIndex, instantiatedItem);
            }
        }

        public void BuyItemFromVendor(UnitController sourceUnitController, VendorItem vendorItem, int collectionIndex, int itemIndex) {
            //Debug.Log($"VendorManager.BuyItemFromVendor({sourceUnitController.gameObject.name}, {vendorItem.Item.ResourceName}, {collectionIndex}, {itemIndex})");

            if (systemGameManager.GameMode == GameMode.Local) {
                vendorComponent.BuyItemFromVendor(sourceUnitController, componentIndex, vendorItem, collectionIndex, itemIndex);
            } else {
                networkManagerClient.BuyItemFromVendor(vendorComponent.Interactable, componentIndex, collectionIndex, itemIndex, vendorItem.Item.ResourceName);
            }
        }

        public void BuyItemFromVendorServer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int accountId) {
            //Debug.Log($"VendorManager.BuyItemFromVendorServer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {accountId})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                VendorComponent vendorComponent = (currentInteractables[componentIndex] as VendorComponent);
                List<VendorCollection> localVendorCollections = vendorComponent.GetVendorCollections(accountId);
                if (localVendorCollections.Count > collectionIndex && localVendorCollections[collectionIndex].VendorItems.Count > itemIndex) {
                    VendorItem vendorItem = localVendorCollections[collectionIndex].VendorItems[itemIndex];
                    if (vendorItem.Item.ResourceName == resourceName) {
                        vendorComponent.BuyItemFromVendor(sourceUnitController, componentIndex, vendorItem, collectionIndex, itemIndex);
                    }
                }
            }

        }
    }

}
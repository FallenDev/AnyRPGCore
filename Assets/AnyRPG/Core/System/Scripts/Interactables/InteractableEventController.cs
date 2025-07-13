using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AnyRPG {

    public class InteractableEventController : ConfiguredClass {

        //public event System.Action<UnitController, int> OnAnimatedObjectChooseMovement = delegate { };
        //public event System.Action<UnitController, int> OnInteractionWithOptionStarted = delegate { };
        public event System.Action<string, int> OnPlayDialogNode = delegate { };
        public event System.Action<UnitController, InstantiatedItem> OnAddToBuyBackCollection = delegate { };
        public event System.Action<VendorItem> OnSellItemToPlayer = delegate { };
        public event System.Action<Dictionary<int, List<int>>> OnDropLoot = delegate { };

        // interactable this controller is attached to
        private Interactable interactable;

        public InteractableEventController() {
            //this.Interactable = interactable;
        }

        public void SetInteractable(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public InteractableEventController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        #region EventNotifications

        public void NotifyOnPlayDialogNode(Dialog dialog, int dialogIndex) {
            Debug.Log($"{interactable.gameObject.name}.InteractableEventController.NotifyOnPlayDialogNode({dialog.ResourceName}, {dialogIndex})");
            OnPlayDialogNode(dialog.ResourceName, dialogIndex);
        }

        public void NotifyOnAddToBuyBackCollection(UnitController sourceUnitController, InstantiatedItem newInstantiatedItem) {
            OnAddToBuyBackCollection(sourceUnitController, newInstantiatedItem);
        }

        public void NotifyOnSellItemToPlayer(VendorItem vendorItem) {
            OnSellItemToPlayer(vendorItem);
        }

        public void NotifyOnDropLoot(Dictionary<int, List<int>> lootDropIdLookup) {
            OnDropLoot(lootDropIdLookup);
        }

        // temporarily disabled because this object is not created early enough in the process when its a unitcontroller
        // this if fixed now ^

        //public void NotifyOnAnimatedObjectChooseMovement(UnitController sourceUnitController, int optionIndex) {
        //    OnAnimatedObjectChooseMovement(sourceUnitController, optionIndex);
        //}

        //public void NotifyOnInteractionWithOptionStarted(UnitController sourceUnitController, int optionIndex) {
        //    OnInteractionWithOptionStarted(sourceUnitController, optionIndex);
        //}

        #endregion


    }

}
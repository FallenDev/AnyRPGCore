using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InventoryPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [Header("Inventory Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        // game manager references
        //protected PlayerManager playerManager = null;
        protected SystemEventManager systemEventManager = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
            bagBarController.SetBagButtonCount(systemConfigurationManager.MaxInventoryBags);
            bagBarController.SetBagPanel(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("InventoryPanel.ProcessCreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            inventoryManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
            inventoryManager.OnAddInventorySlot += HandleAddSlot;
            inventoryManager.OnRemoveInventorySlot += HandleRemoveSlot;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            inventoryManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
            inventoryManager.OnAddInventorySlot -= HandleAddSlot;
            inventoryManager.OnRemoveInventorySlot -= HandleRemoveSlot;
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            //Debug.Log("InventoryPanel.HandlePlayerUnitDespawn()");

            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            //Debug.Log("InventoryPanel.HandleAddInventoryBagNode()");
            bagBarController.AddBagButton(bagNode);
            //bagNode.BagPanel = this;
        }


    }

}
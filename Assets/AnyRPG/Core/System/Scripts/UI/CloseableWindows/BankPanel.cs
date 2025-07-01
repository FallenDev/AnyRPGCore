using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BankPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;
        [Header("Bank Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        // game manager references
        //protected PlayerManager playerManager = null;
        protected SystemEventManager systemEventManager = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
            bagBarController.SetBagButtonCount(systemConfigurationManager.MaxBankBags);
            bagBarController.SetBagPanel(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("BankPanel.ProcessCreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            inventoryManager.OnAddBankBagNode += HandleAddBankBagNode;
            inventoryManager.OnAddBankSlot += HandleAddSlot;
            inventoryManager.OnRemoveBankSlot += HandleRemoveSlot;

        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            inventoryManager.OnAddBankBagNode -= HandleAddBankBagNode;
            inventoryManager.OnAddBankSlot -= HandleAddSlot;
            inventoryManager.OnRemoveBankSlot -= HandleRemoveSlot;
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddBankBagNode(BagNode bagNode) {
            //Debug.Log("BankPanel.HandleAddBankBagNode()");
            bagBarController.AddBagButton(bagNode);
            //bagNode.BagPanel = this;
        }


    }

}
using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootDrop : ConfiguredClass, IDescribable {

        // game manager references
        protected LootManager lootManager = null;
        protected UIManager uIManager = null;

        public virtual string ResourceName => string.Empty;
        public virtual string Description => string.Empty;

        public ItemQuality ItemQuality {
            get {
                return InstantiatedItem.ItemQuality;
            }
        }

        public Sprite Icon {
            get {
                return InstantiatedItem.Icon;
            }
        }

        public string DisplayName {
            get {
                return InstantiatedItem.DisplayName;
            }
        }

        public InstantiatedItem InstantiatedItem { get; set; }

        public LootTableState LootTableState { get; set; }

        public LootDrop(InstantiatedItem item, LootTableState lootTableState, SystemGameManager systemGameManager) {
            LootTableState = lootTableState;
            InstantiatedItem = item;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
            uIManager = systemGameManager.UIManager;
        }

        public virtual void TakeLoot(UnitController sourceUnitController) {

            if (ProcessTakeLoot(sourceUnitController)) {
                AfterLoot(sourceUnitController);
                Remove();
                lootManager.TakeLoot(sourceUnitController, this);
            }
        }

        public void SetBackgroundImage(Image backgroundImage) {
            uIManager.SetItemBackground(InstantiatedItem.Item, backgroundImage, new Color32(0, 0, 0, 255));
        }

        public bool HasItem(Item item) {
            return (InstantiatedItem.ResourceName == item.ResourceName);
        }

        protected bool ProcessTakeLoot(UnitController sourceUnitController) {
            return sourceUnitController.CharacterInventoryManager.AddItem(InstantiatedItem, false);
        }

        public void Remove() {
            LootTableState.RemoveDroppedItem(this);
            if (LootTableState.DroppedItems.Count == 0) {
                lootManager.RemoveLootTableState(LootTableState);
            }
        }

        public void AfterLoot(UnitController sourceUnitController) {
            if (InstantiatedItem is InstantiatedCurrencyItem) {
                (InstantiatedItem as InstantiatedCurrencyItem).Use(sourceUnitController);
            } else if (InstantiatedItem is InstantiatedQuestStartItem) {
                (InstantiatedItem as InstantiatedQuestStartItem).Use(sourceUnitController);
            }
        }

        public string GetSummary() {
            return InstantiatedItem.GetSummary();
        }

        public string GetDescription() {
            return InstantiatedItem.GetDescription();
        }

    }

}
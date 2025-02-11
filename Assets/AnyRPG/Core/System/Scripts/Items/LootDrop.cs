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

        public virtual Sprite Icon => null;

        public virtual string ResourceName => string.Empty;
        public virtual string DisplayName => string.Empty;
        public virtual string Description => string.Empty;

        public virtual ItemQuality ItemQuality {
            get {
                return null;
            }
        }


        public LootDrop(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        public virtual void SetBackgroundImage(Image backgroundImage) {
            // do nothing, only used in child classes
        }

        public virtual bool HasItem(Item item) {
            return false;
        }


        public virtual void TakeLoot(UnitController sourceUnitController) {

            if (ProcessTakeLoot(sourceUnitController)) {
                AfterLoot(sourceUnitController);
                Remove();
                lootManager.TakeLoot(this);
            }

        }

        protected virtual bool ProcessTakeLoot(UnitController sourceUnitController) {
            // need a fake value by default
            return true;
        }

        public virtual void AfterLoot(UnitController sourceUnitController) {

        }

        public virtual void Remove() {
            //Debug.Log("LootDrop.Remove()");

        }

        public virtual string GetSummary() {
            return string.Empty;
        }

        public virtual string GetDescription() {
            return string.Empty;
        }
    }

    public class CurrencyLootDrop : LootDrop {

        private Sprite icon = null;

        private string summary = string.Empty;

        public override Sprite Icon {
            get {
                return icon;
            }
        }

        public override string DisplayName {
            get {
                return GetSummary();
            }
        }

        // game manager references
        private CurrencyConverter currencyConverter = null;
        private PlayerManager playerManager = null;
        private LogManager logManager = null;

        private Dictionary<LootableCharacterComponent, CurrencyNode> currencyNodes = new Dictionary<LootableCharacterComponent, CurrencyNode>();

        public Dictionary<LootableCharacterComponent, CurrencyNode> CurrencyNodes { get => currencyNodes; set => currencyNodes = value; }

        public CurrencyLootDrop(SystemGameManager systemGameManager) : base(systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
        }

        public override void SetBackgroundImage(Image backgroundImage) {
            base.SetBackgroundImage(backgroundImage);
            backgroundImage.sprite = null;
            backgroundImage.color = new Color32(0, 0, 0, 255);
        }

        public void AddCurrencyNode(LootableCharacterComponent lootableCharacter, CurrencyNode currencyNode) {
            //Debug.Log("LootableDrop.AddCurrencyNode(" + lootableCharacter.name + ", " + currencyNode.currency.DisplayName + " " + currencyNode.MyAmount +")");

            currencyNodes.Add(lootableCharacter, currencyNode);

            List<CurrencyNode> usedCurrencyNodes = new List<CurrencyNode>();
            foreach (CurrencyNode tmpCurrencyNode in currencyNodes.Values) {
                usedCurrencyNodes.Add(tmpCurrencyNode);
            }
            KeyValuePair<Sprite, string> keyValuePair = currencyConverter.RecalculateValues(usedCurrencyNodes, true);
            icon = keyValuePair.Key;
            summary = keyValuePair.Value;
        }

        public override string GetSummary() {
            //Debug.Log("LootableDrop.GetDescription()");
            return GetDescription();
        }

        protected override bool ProcessTakeLoot(UnitController sourceUnitController) {
            base.ProcessTakeLoot(sourceUnitController);
            foreach (LootableCharacterComponent lootableCharacter in currencyNodes.Keys) {
                if (currencyNodes[lootableCharacter].currency != null) {
                    sourceUnitController.CharacterCurrencyManager.AddCurrency(currencyNodes[lootableCharacter].currency, currencyNodes[lootableCharacter].Amount);
                    List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                    tmpCurrencyNode.Add(currencyNodes[lootableCharacter]);
                    logManager.WriteSystemMessage("Gained " + currencyConverter.RecalculateValues(tmpCurrencyNode, false).Value.Replace("\n", ", "));
                    lootableCharacter.TakeCurrencyLoot();
                }
            }
            return true;
        }

        public override string GetDescription() {
            //Debug.Log("LootableDrop.GetSummary()");
            return summary;
        }

    }

    public class ItemLootDrop : LootDrop {

        // game manager references
        private UIManager uIManager = null;
        //private InventoryManager inventoryManager = null;
        private PlayerManager playerManager = null;

        public override ItemQuality ItemQuality {
            get {
                if (InstantiatedItem != null) {
                    return InstantiatedItem.ItemQuality;
                }
                return base.ItemQuality;
            }
        }

        public override Sprite Icon {
            get {
                if (InstantiatedItem != null) {
                    return InstantiatedItem.Icon;
                }
                return base.Icon;
            }
        }

        public override string DisplayName {
            get {
                if (InstantiatedItem != null) {
                    return InstantiatedItem.DisplayName;
                }
                return base.DisplayName;
            }
        }

        public InstantiatedItem InstantiatedItem { get; set; }

        public LootTableState LootTableState { get; set; }

        public ItemLootDrop(InstantiatedItem item, LootTableState lootTableState, SystemGameManager systemGameManager) : base(systemGameManager) {
            LootTableState = lootTableState;
            InstantiatedItem = item;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            //inventoryManager = systemGameManager.InventoryManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public override void SetBackgroundImage(Image backgroundImage) {
            base.SetBackgroundImage(backgroundImage);
            uIManager.SetItemBackground(InstantiatedItem.Item, backgroundImage, new Color32(0, 0, 0, 255));
        }

        public override bool HasItem(Item item) {
            return (InstantiatedItem.ResourceName == item.ResourceName);
        }

        protected override bool ProcessTakeLoot(UnitController sourceUnitController) {
            base.ProcessTakeLoot(sourceUnitController);
            return sourceUnitController.CharacterInventoryManager.AddItem(InstantiatedItem, false);
        }

        public override void Remove() {
            base.Remove();
            LootTableState.RemoveDroppedItem(this);
            if (LootTableState.DroppedItems.Count == 0) {
                lootManager.RemoveLootTableState(LootTableState);
            }
        }

        public override void AfterLoot(UnitController sourceUnitController) {
            base.AfterLoot(sourceUnitController);
            if (InstantiatedItem is InstantiatedCurrencyItem) {
                (InstantiatedItem as InstantiatedCurrencyItem).Use(sourceUnitController);
            } else if (InstantiatedItem is InstantiatedQuestStartItem) {
                (InstantiatedItem as InstantiatedQuestStartItem).Use(sourceUnitController);
            }
        }

        public override string GetSummary() {
            if (InstantiatedItem != null) {
                return InstantiatedItem.GetSummary();
            }
            return base.GetSummary();
        }

        public override string GetDescription() {
            if (InstantiatedItem != null) {
                return InstantiatedItem.GetDescription();
            }
            return base.GetDescription();
        }
    }

}
using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedCurrencyItem : InstantiatedItem {

        private CurrencyItem currencyItem = null;

        private string gainCurrencyName = string.Empty;
        private int gainCurrencyAmount = 0;
        private CurrencyNode currencyNode;

        public string GainCurrencyName { get => gainCurrencyName; }
        public int GainCurrencyAmount { get => gainCurrencyAmount; }
        public CurrencyNode CurrencyNode { get => currencyNode; }

        public InstantiatedCurrencyItem(SystemGameManager systemGameManager, int instanceId, CurrencyItem currencyItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, currencyItem, itemQuality) {
            this.currencyItem = currencyItem;
            gainCurrencyName = currencyItem.GainCurrencyName;
            gainCurrencyAmount = currencyItem.GainCurrencyAmount;
            currencyNode = currencyItem.CurrencyNode;
        }

        public void OverrideCurrency(string newGainCurrencyName, int newGainCurrencyAmount) {
            gainCurrencyName = newGainCurrencyName;
            gainCurrencyAmount = newGainCurrencyAmount;
            Currency tmpCurrency = systemDataFactory.GetResource<Currency>(newGainCurrencyName);
            if (tmpCurrency != null) {
                currencyNode = new CurrencyNode();
                currencyNode.currency = tmpCurrency;
                currencyNode.Amount = newGainCurrencyAmount;
            } else {
                Debug.LogError($"CurrencyItem.SetupScriptableObjects(): Could not find currency : {gainCurrencyName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
            }
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log("CurrencyItem.Use()");
            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }
            if (currencyNode.currency != null) {
                sourceUnitController.CharacterCurrencyManager.AddCurrency(currencyNode.currency, currencyNode.Amount);
            }
            Remove();
            return true;
        }

        public override InventorySlotSaveData GetSlotSaveData() {
            //Debug.Log($"InstantiatedCurrencyItem.GetSlotSaveData()");

            InventorySlotSaveData saveData = base.GetSlotSaveData();
            saveData.gainCurrencyName = gainCurrencyName;
            saveData.gainCurrencyAmount = gainCurrencyAmount;
            //Debug.Log($"InstantiatedCurrencyItem.GetSlotSaveData(): gainCurrencyName = {gainCurrencyName}, gainCurrencyAmount = {gainCurrencyAmount}");
            return saveData;
        }

        public override void LoadSaveData(InventorySlotSaveData inventorySlotSaveData) {
            //Debug.Log($"InstantiatedCurrencyItem.LoadSaveData({inventorySlotSaveData.ItemName}) name: {inventorySlotSaveData.gainCurrencyName} amount: {inventorySlotSaveData.gainCurrencyAmount}");

            base.LoadSaveData(inventorySlotSaveData);
            OverrideCurrency(inventorySlotSaveData.gainCurrencyName, inventorySlotSaveData.gainCurrencyAmount);
        }

        /*
        public override string GetSummary() {
            return base.GetSummary();
        }
        */

        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + currencyItem.GetCurrencyItemDescription(gainCurrencyName, gainCurrencyAmount);
        }

        /*
        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log(DisplayName + ".CurrencyItem.GetSummary();");
            string tmpCurrencyName = string.Empty;
            if (currencyNode.currency != null) {
                tmpCurrencyName = currencyNode.currency.DisplayName;
            }
            return base.GetDescription(usedItemQuality, usedItemLevel) + string.Format("\n<color=green>Use: Gain {0} {1}</color>", tmpCurrencyName, currencyNode.Amount);
        }
        */

    }

}
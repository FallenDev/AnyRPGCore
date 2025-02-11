using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedCurrencyItem : InstantiatedItem {

        private CurrencyItem currencyItem = null;

        public InstantiatedCurrencyItem(SystemGameManager systemGameManager, int instanceId, CurrencyItem currencyItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, currencyItem, itemQuality) {
            this.currencyItem = currencyItem;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log("CurrencyItem.Use()");
            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }
            if (currencyItem.CurrencyNode.currency != null) {
                sourceUnitController.CharacterCurrencyManager.AddCurrency(currencyItem.CurrencyNode.currency, currencyItem.CurrencyNode.Amount);
            }
            Remove();
            return true;
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
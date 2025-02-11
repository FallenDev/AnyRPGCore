using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : ConfiguredMonoBehaviour {

        private int itemIdCount = 0;

        // game manager references
        SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public InstantiatedItem GetNewInstantiatedItem(string itemName, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            Item item = systemDataFactory.GetResource<Item>(itemName);
            if (item == null) {
                return null;
            }
            return GetNewInstantiatedItem(item, usedItemQuality);
        }

        /// <summary>
        /// Get a new instantiated Item
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            InstantiatedItem instantiatedItem = item.GetNewInstantiatedItem(systemGameManager, itemIdCount, item, usedItemQuality);
            instantiatedItem.InitializeNewItem(usedItemQuality);
            itemIdCount++;
            return instantiatedItem;
        }
        /*
        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            InstantiatedItem instantiatedItem = new InstantiatedItem(systemGameManager, itemIdCount, item, usedItemQuality);
            instantiatedItem.InitializeNewItem(usedItemQuality);
            return instantiatedItem;
        }
        */

    }

}
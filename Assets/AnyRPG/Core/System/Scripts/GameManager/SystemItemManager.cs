using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : ConfiguredMonoBehaviour {

        private int clientItemIdCount = 0;
        private int serverItemIdCount = 0;

        // game manager references
        SystemDataFactory systemDataFactory = null;
        NetworkManagerServer networkManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
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
            InstantiatedItem instantiatedItem = GetNewInstantiatedItem(GetNewItemInstanceId(), item, usedItemQuality);
            return instantiatedItem;
        }

        public int GetNewItemInstanceId() {
            int returnValue = (networkManagerServer.ServerModeActive == true ? serverItemIdCount : clientItemIdCount);
            if (networkManagerServer.ServerModeActive == true) {
                serverItemIdCount--;
            } else {
                clientItemIdCount++;
            }
            return returnValue;
        }

        public InstantiatedItem GetNewInstantiatedItem(int itemInstanceId, Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            InstantiatedItem instantiatedItem = item.GetNewInstantiatedItem(systemGameManager, itemInstanceId, item, usedItemQuality);
            instantiatedItem.InitializeNewItem(usedItemQuality);
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
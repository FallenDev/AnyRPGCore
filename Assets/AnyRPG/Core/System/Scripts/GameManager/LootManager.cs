using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class LootManager : ConfiguredMonoBehaviour {

        public event System.Action OnTakeLoot = delegate { };
        public event Action OnAvailableLootAdded = delegate { };

        // clientId, LootDrop
        // a list that is reset every time the loot window opens or closes to give the proper list depending on what was looted
        private Dictionary<int, List<LootDrop>> availableDroppedLoot = new Dictionary<int, List<LootDrop>>();

        // this list is solely for the purpose of tracking dropped loot to ensure that unique items cannot be dropped twice
        // if one drops and is left on a body unlooted and another enemy is killed
        private List<LootTableState> lootTableStates = new List<LootTableState>();
        private Dictionary<int, LootTableState> lootTableStateDict = new Dictionary<int, LootTableState>();

        private CurrencyItem currencyLootItem = null;

        private int lootDropId = 0;
        // a dictionary of all dropped loot
        private Dictionary<int, LootDrop> lootDropIndex = new Dictionary<int, LootDrop>();

        // game manager references
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;
        private SystemItemManager systemItemManager = null;

        public Dictionary<int, List<LootDrop>> AvailableDroppedLoot { get => availableDroppedLoot; }
        public CurrencyItem CurrencyLootItem { get => currencyLootItem; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyLootItem = ScriptableObject.CreateInstance<CurrencyItem>();
            currencyLootItem.ResourceName = "System Currency Loot Item";
            // pre populate client id 0 so this works before loot is dropped
            availableDroppedLoot.Add(0, new List<LootDrop>());
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            systemItemManager = systemGameManager.SystemItemManager;
        }

        public void AddAvailableLoot(UnitController sourceUnitController, List<LootDrop> items) {
            //Debug.Log($"LootManager.AddAvailableLoot({sourceUnitController.gameObject.name}, count: {items.Count})");

            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                AddAvailableLoot(playerManagerServer.ActivePlayerLookup[sourceUnitController], items);
            }
        }

        public void AddAvailableLoot(int clientId, List<int> lootDropIds) {
            //Debug.Log($"LootManager.AddAvailableLoot({clientId}, count: {lootDropIds.Count})");

            List<LootDrop> lootDrops = new List<LootDrop>();
            foreach (int lootDropId in lootDropIds) {
                if (lootDropIndex.ContainsKey(lootDropId)) {
                    lootDrops.Add(lootDropIndex[lootDropId]);
                }
            }
            AddAvailableLoot(clientId, lootDrops);
        }

        public void AddAvailableLoot(int clientId, List<LootDrop> items) {
            Debug.Log($"LootManager.AddAvailableLoot({clientId}, count: {items.Count})");

            if (availableDroppedLoot.ContainsKey(clientId)) {
                availableDroppedLoot[clientId] = items;
            } else {
                availableDroppedLoot.Add(clientId, items);
            }
            
            // copy this data to the client
            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AddAvailableDroppedLoot(clientId, items);
            } else {
                OnAvailableLootAdded();
            }
        }

        public void ClearAvailableDroppedLoot() {
            //Debug.Log("LootManager.ClearDroppedLoot()");

            availableDroppedLoot[0].Clear();
        }

        public void RequestTakeLoot(LootDrop lootDrop, UnitController sourceUnitController) {
            if (systemGameManager.GameMode == GameMode.Local) {
                lootDrop.TakeLoot(playerManager.UnitController);
            } else {
                networkManagerClient.RequestTakeLoot(lootDrop.LootDropId);
            }
        }

        public void TakeLoot(UnitController sourceUnitController, LootDrop lootDrop) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                TakeLoot(playerManagerServer.ActivePlayerLookup[sourceUnitController], lootDrop);
            }
        }

        public void TakeLoot(int clientId, int lootDropId) {
            //Debug.Log("LootManager.TakeLoot()");
            if (lootDropIndex.ContainsKey(lootDropId) == false) {
                return;
            }
            LootDrop lootDrop = lootDropIndex[lootDropId];
            TakeLoot(clientId, lootDrop);
        }

        public void TakeLoot(int clientId, LootDrop lootDrop) {
            //Debug.Log("LootManager.TakeLoot()");

            RemoveLootTableStateIndex(lootDropId);
            RemoveFromAvailableDroppedItems(clientId, lootDrop);

            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AdvertiseTakeLoot(clientId, lootDrop.LootDropId);
            }

            SystemEventManager.TriggerEvent("OnTakeLoot", new EventParamProperties());
            OnTakeLoot();
        }

        public void RemoveFromAvailableDroppedItems(int clientId, LootDrop lootDrop) {
            //Debug.Log("LootManager.RemoveFromDroppedItems()");

            if (availableDroppedLoot.ContainsKey(clientId) && availableDroppedLoot[clientId].Contains(lootDrop)) {
                availableDroppedLoot[clientId].Remove(lootDrop);
            }
        }

        public void TakeAllLoot(UnitController sourceUnitController) {
            if (systemGameManager.GameMode == GameMode.Local) {
                TakeAllLootInternal(0, sourceUnitController);
            } else {
                networkManagerClient.TakeAllLoot();
            }
        }

        public void TakeAllLootInternal(int clientId, UnitController sourceUnitController) {
            //Debug.Log("LootManager.TakeAllLoot()");

            // added emptyslotcount to prevent game from freezup when no bag space left and takeall button pressed
            int maximumLoopCount = availableDroppedLoot[clientId].Count;
            int currentLoopCount = 0;
            while (availableDroppedLoot[clientId].Count > 0 && sourceUnitController.CharacterInventoryManager.EmptySlotCount() > 0 && currentLoopCount < maximumLoopCount) {
                availableDroppedLoot[clientId][0].TakeLoot(sourceUnitController);
                currentLoopCount++;
            }

            if (availableDroppedLoot[clientId].Count > 0 && sourceUnitController.CharacterInventoryManager.EmptySlotCount() == 0) {
                if (sourceUnitController.CharacterInventoryManager.EmptySlotCount() == 0) {
                    //Debug.Log("No space left in inventory");
                }
                messageFeedManager.WriteMessage(sourceUnitController, "Inventory is full!");
            }
        }

        public void AddLootTableState(LootTableState lootTableState) {
            //Debug.Log("LootManager.AddLootTableState()");

            if (lootTableStates.Contains(lootTableState) == false) {
                lootTableStates.Add(lootTableState);
            }
        }

        public void RemoveLootTableState(LootTableState lootTableState) {
            //Debug.Log("LootManager.RemoveLootTableState()");

            if (lootTableStates.Contains(lootTableState)) {
                lootTableStates.Remove(lootTableState);
            }
        }

        public void AddLootTableStateIndex(int LootDropId, LootTableState lootTableState) {
            //Debug.Log("LootManager.AddLootTableState()");

            if (lootTableStateDict.ContainsKey(lootDropId) == false) {
                lootTableStateDict.Add(lootDropId, lootTableState);
            }
        }

        public void RemoveLootTableStateIndex(int lootDropId) {
            if (lootTableStateDict.ContainsKey(lootDropId) == false) {
                return;
            }
            LootTableState lootTableState = lootTableStateDict[lootDropId];
            lootTableState.RemoveDroppedItem(lootDropIndex[lootDropId]);
            if (lootTableState.DroppedItems.Count == 0) {
                RemoveLootTableState(lootTableState);
            }
        }

        public bool CanDropUniqueItem(UnitController sourceUnitController, Item item) {
            //Debug.Log("LootManager.CanDropUniqueItem(" + item.DisplayName + ")");
            if (sourceUnitController.CharacterInventoryManager.GetItemCount(item.ResourceName) > 0) {
                return false;
            }
            if (sourceUnitController.CharacterEquipmentManager.HasEquipment(item.ResourceName) == true) {
                return false;
            }
            foreach (LootTableState lootTableState in lootTableStates) {
                foreach (LootDrop lootDrop in lootTableState.DroppedItems) {
                    if (lootDrop.HasItem(item)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public int GetLootDropId() {
            lootDropId++;
            return lootDropId;
        }

        public void AddLootDropToIndex(UnitController sourceUnitController, LootDrop lootDrop) {
            //Debug.Log($"LootManager.AddLootDropToIndex({sourceUnitController.gameObject.name}, {lootDrop.LootDropId})");
            
            lootDropIndex.Add(lootDrop.LootDropId, lootDrop);
            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AddLootDrop(playerManagerServer.ActivePlayerLookup[sourceUnitController], lootDropId, lootDrop.InstantiatedItem.InstanceId);
            }
        }

        public void AddNetworkLootDrop(int lootDropId, int itemId) {
            Debug.Log($"LootManager.AddNetworkLootDrop({lootDropId}, {itemId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemId) == false) {
                return;
            }
            LootDrop lootDrop = new LootDrop(lootDropId, systemItemManager.InstantiatedItems[itemId], systemGameManager);
            lootDropIndex.Add(lootDropId, lootDrop);
        }

    }

}
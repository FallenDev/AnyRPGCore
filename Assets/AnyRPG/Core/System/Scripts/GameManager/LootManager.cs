using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootManager : ConfiguredMonoBehaviour {

        public event System.Action OnTakeLoot = delegate { };

        // clientId, LootDrop
        // a list that is reset every time the loot window opens or closes to give the proper list depending on what was looted
        private Dictionary<int, List<LootDrop>> droppedLoot = new Dictionary<int, List<LootDrop>>();

        // this list is solely for the purpose of tracking dropped loot to ensure that unique items cannot be dropped twice
        // if one drops and is left on a body unlooted and another enemy is killed
        private List<LootTableState> lootTableStates = new List<LootTableState>();

        private CurrencyItem currencyLootItem = null;

        // game manager references
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;

        public Dictionary<int, List<LootDrop>> DroppedLoot { get => droppedLoot; }
        public CurrencyItem CurrencyLootItem { get => currencyLootItem; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyLootItem = ScriptableObject.CreateInstance<CurrencyItem>();
            currencyLootItem.ResourceName = "System Currency Loot Item";
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public void AddLoot(UnitController sourceUnitController, List<LootDrop> items) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                AddLoot(playerManagerServer.ActivePlayerLookup[sourceUnitController], items);
            }
        }

        public void AddLoot(int clientId, List<LootDrop> items) {
            //Debug.Log("LootManager.AddLoot()");
            if (droppedLoot.ContainsKey(clientId)) {
                droppedLoot[clientId] = items;
            } else {
                droppedLoot.Add(clientId, items);
            }
            // add the code here to copy this data to the client
            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AddDroppedLoot(clientId, items);
            }
        }

        public void ClearDroppedLoot() {
            //Debug.Log("LootManager.ClearDroppedLoot()");

            droppedLoot.Clear();
        }

        public void TakeLoot(UnitController sourceUnitController, LootDrop lootDrop) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                TakeLoot(playerManagerServer.ActivePlayerLookup[sourceUnitController], lootDrop);
            }
        }

        public void TakeLoot(int clientId, LootDrop lootDrop) {
            //Debug.Log("LootManager.TakeLoot()");

            RemoveFromDroppedItems(clientId, lootDrop);

            SystemEventManager.TriggerEvent("OnTakeLoot", new EventParamProperties());
            OnTakeLoot();
        }

        public void RemoveFromDroppedItems(int clientId, LootDrop lootDrop) {
            //Debug.Log("LootManager.RemoveFromDroppedItems()");

            if (droppedLoot.ContainsKey(clientId) && droppedLoot[clientId].Contains(lootDrop)) {
                droppedLoot[clientId].Remove(lootDrop);
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
            int maximumLoopCount = droppedLoot[clientId].Count;
            int currentLoopCount = 0;
            while (droppedLoot[clientId].Count > 0 && sourceUnitController.CharacterInventoryManager.EmptySlotCount() > 0 && currentLoopCount < maximumLoopCount) {
                droppedLoot[clientId][0].TakeLoot(sourceUnitController);
                currentLoopCount++;
            }

            if (droppedLoot[clientId].Count > 0 && sourceUnitController.CharacterInventoryManager.EmptySlotCount() == 0) {
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

    }

}
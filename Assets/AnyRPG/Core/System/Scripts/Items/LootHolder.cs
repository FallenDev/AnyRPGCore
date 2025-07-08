using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootHolder : ConfiguredClass {

        /// <summary>
        /// lootTable, accountId, lootTableState
        /// </summary>
        private Dictionary<LootTable, Dictionary<int, LootTableState>> lootTableStates = new Dictionary<LootTable, Dictionary<int, LootTableState>>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;

        public Dictionary<LootTable, Dictionary<int, LootTableState>> LootTableStates { get => lootTableStates; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }
        
        public void InitializeLootTableStates() {
            lootTableStates = new Dictionary<LootTable, Dictionary<int, LootTableState>>();
        }
        

        public void AddLootTableState(LootTable lootTable) {
            lootTableStates.Add(lootTable, new Dictionary<int, LootTableState>());
        }

        public void ClearLootTableStates() {
            lootTableStates.Clear();
        }

        public List<LootDrop> GetLoot(UnitController sourceUnitController, LootTable lootTable, bool rollLoot) {
            Debug.Log($"LootHolder.GetLoot({sourceUnitController?.name}, {rollLoot})");

            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController) == false) {
                return new List<LootDrop>();
            }
            int accountId = playerManagerServer.ActivePlayerLookup[sourceUnitController];
            if (lootTableStates.ContainsKey(lootTable) == false) {
                //Debug.Log($"{gameObject.name}.LootHolder.GetLoot(): lootTableStates does not contain {lootTable.name}");
                return new List<LootDrop>();
            }

            // add account if it does not exist
            if (lootTableStates[lootTable].ContainsKey(accountId) == false) {
                lootTableStates[lootTable].Add(accountId, new LootTableState(systemGameManager));
            }
            return lootTableStates[lootTable][accountId].GetLoot(sourceUnitController, lootTable, rollLoot);
        }
    }
}
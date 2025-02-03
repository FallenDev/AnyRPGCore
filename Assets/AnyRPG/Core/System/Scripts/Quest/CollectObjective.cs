using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CollectObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        protected string itemName = null;

        [Tooltip("If true, the name can be partially matched")]
        [SerializeField]
        protected bool partialMatch = false;

        // game manager references
        //protected InventoryManager inventoryManager = null;
        

        public override string ObjectiveName { get => itemName; }

        public override Type ObjectiveType {
            get {
                return typeof(CollectObjective);
            }
        }

        public void UpdateItemCount(UnitController sourceUnitController, Item item) {

            // change this with check reference to item prefab in the future
            if (SystemDataUtility.MatchResource(item.ResourceName, itemName, partialMatch)) {
                UpdateCompletionCount(sourceUnitController);
            }
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {

            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            SetCurrentAmount(sourceUnitController,
                sourceUnitController.CharacterInventoryManager.GetItemCount(itemName, partialMatch)
                + sourceUnitController.CharacterEquipmentManager.GetEquipmentCount(itemName, partialMatch));

            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount(sourceUnitController) != 0) {
                messageFeedManager.WriteMessage(sourceUnitController, string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                messageFeedManager.WriteMessage(sourceUnitController, string.Format("Collect {0} {1}: Objective Complete", CurrentAmount(sourceUnitController), DisplayName));
            }
            questBase.CheckCompletion(sourceUnitController, true, printMessages);
            base.UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public void Complete(UnitController sourceUnitController) {
            List<Item> items = sourceUnitController.CharacterInventoryManager.GetItems(itemName, Amount);
            foreach (Item item in items) {
                item.Remove();
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            systemEventManager.OnItemCountChanged += UpdateItemCount;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            systemEventManager.OnItemCountChanged -= UpdateItemCount;
        }

        /*
        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inventoryManager = systemGameManager.InventoryManager;
        }
        */

    }


}
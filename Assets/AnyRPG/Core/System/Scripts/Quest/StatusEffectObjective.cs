using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class StatusEffectObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected string effectName = null;

        public override string ObjectiveName { get => effectName; }

        public override Type ObjectiveType {
            get {
                return typeof(StatusEffectObjective);
            }
        }

        private StatusEffectProperties statusEffect;

        public void UpdateApplyCount(UnitController sourceUnitController) {
            bool completeBefore = IsComplete(sourceUnitController);
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
                questBase.CheckCompletion(sourceUnitController);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", statusEffect.DisplayName, CurrentAmount(sourceUnitController), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount(sourceUnitController), statusEffect.DisplayName));
                }
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {

            base.UpdateCompletionCount(sourceUnitController, printMessages);
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            if (sourceUnitController.CharacterStats.GetStatusEffectNode(statusEffect) != null) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
                questBase.CheckCompletion(sourceUnitController, true, printMessages);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", statusEffect.DisplayName, CurrentAmount(sourceUnitController), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount(sourceUnitController), statusEffect.DisplayName));
                }
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            statusEffect.OnApply += UpdateApplyCount;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            statusEffect.OnApply -= UpdateApplyCount;
        }

        public override string GetUnformattedStatus(UnitController sourceUnitController) {
            string beginText = string.Empty;
            //beginText = "Use ";
            //return beginText + DisplayName + ": " + Mathf.Clamp(CurrentAmount, 0, Amount) + "/" + Amount;
            return DisplayName + ": " + Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount) + "/" + Amount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, QuestBase quest) {
            base.SetupScriptableObjects(systemGameManager, quest);
            
            if (effectName != null && effectName != string.Empty) {
                StatusEffect tmpAbility = systemDataFactory.GetResource<AbilityEffect>(effectName) as StatusEffect;
                if (tmpAbility != null) {
                    statusEffect = tmpAbility.StatusEffectProperties;
                } else {
                    Debug.LogError("StatusEffectObjective.SetupScriptableObjects(): Could not find ability : " + effectName + " while inititalizing an ability objective for " + quest.ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}
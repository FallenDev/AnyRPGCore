using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New Achievement", menuName = "AnyRPG/Achievement")]
    public class Achievement : QuestBase {

        // game manager references
        protected AchievementLog achievementLog = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            achievementLog = systemGameManager.AchievementLog;
        }

        protected override void ProcessMarkComplete(UnitController sourceUnitController, bool printMessages) {
            base.ProcessMarkComplete(sourceUnitController, printMessages);
            if (printMessages == true) {
                messageFeedManager.WriteMessage(string.Format("Achievement: {0} Complete!", DisplayName));
            }
            playerManager.PlayLevelUpEffects(sourceUnitController, 0);

            SetTurnedIn(sourceUnitController, true);
        }

        protected override QuestSaveData GetSaveData(UnitController sourceUnitController) {
            Debug.Log($"{ResourceName}.Achievement.GetSaveData({sourceUnitController.gameObject.name})");

            return sourceUnitController.CharacterQuestLog.GetAchievementSaveData(this);
        }

        protected override void SetSaveData(UnitController sourceUnitController, string QuestName, QuestSaveData questSaveData) {
            sourceUnitController.CharacterQuestLog.SetAchievementSaveData(ResourceName, questSaveData);
        }

        protected override bool HasQuest(UnitController sourceUnitController) {
            return sourceUnitController.CharacterQuestLog.HasAchievement(ResourceName);
        }

    }

}
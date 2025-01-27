using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class VisitZoneObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        protected string zoneName = null;

        public override string ObjectiveName { get => zoneName; }

        public override Type ObjectiveType {
            get {
                return typeof(VisitZoneObjective);
            }
        }

        private SceneNode objectiveSceneNode = null;

        // NEW (HOPEFULLY) SAFE COMPLETION CHECK CODE THAT SHOULDN'T RESULT IN RUNAWAY STACK OVERFLOW ETC
        public void AddCompletionAmount(UnitController sourceUnitController) {
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }

            SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
            questBase.CheckCompletion(sourceUnitController);
            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount(sourceUnitController) != 0) {
                messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                messageFeedManager.WriteMessage(string.Format("{0}: Objective Complete", DisplayName));
            }
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {
            base.UpdateCompletionCount(sourceUnitController, printMessages);
            SceneNode sceneNode = systemDataFactory.GetResource<SceneNode>(zoneName);
            if (sceneNode != null && sceneNode.SceneFile == levelManager.ActiveSceneName) {
                AddCompletionAmount(sourceUnitController);
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);

            objectiveSceneNode = null;
            if (zoneName != null && zoneName != string.Empty) {
                objectiveSceneNode = systemDataFactory.GetResource<SceneNode>(zoneName);
            } else {
                Debug.LogError("VisitZoneObjective.OnAcceptQuest(): Could not find scene node : " + zoneName + " while inititalizing a visit zone objective.  CHECK INSPECTOR");
                return;
            }
            // disabled for now.  this should be an active objective, and not able to be completed if a zone was previously visited
            // this allows creating quests where you have to travel back to a zone you've already been to and perform a new task
            //UpdateCompletionCount(printMessages);
            objectiveSceneNode.OnVisitZone += AddCompletionAmount;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest(sourceUnitController);
            objectiveSceneNode.OnVisitZone -= AddCompletionAmount;
        }

    }
}
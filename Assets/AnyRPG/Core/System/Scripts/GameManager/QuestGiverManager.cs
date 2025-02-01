using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestGiverManager : InteractableOptionManager {

        private QuestGiverComponent questGiver = null;

        // game manager references
        private PlayerManager playerManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;

        public QuestGiverComponent QuestGiver { get => questGiver; set => questGiver = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public List<Quest> GetAvailableQuestList(UnitController sourceUnitController) {
            List<Quest> returnList = new List<Quest>();

            foreach (QuestNode questNode in questGiver.Props.Quests) {
                if (!sourceUnitController.CharacterQuestLog.HasQuest(questNode.Quest.ResourceName)) {
                    returnList.Add(questNode.Quest);
                }
            }

            return returnList;
        }

        public void AcceptQuestClient(UnitController sourceUnitController, Quest quest) {
            Debug.Log($"QuestGiverManager.AcceptQuestClient({sourceUnitController.gameObject.name}, {quest.ResourceName})");
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                AcceptQuestInternal(sourceUnitController, quest);
            } else {
                networkManagerClient.AcceptQuest(quest);
            }
        }

        public void AcceptQuestInternal(UnitController sourceUnitController, Quest quest) {
            Debug.Log($"QuestGiverManager.AcceptQuestInternal({sourceUnitController.gameObject.name}, {quest.ResourceName})");

            sourceUnitController.CharacterQuestLog.AcceptQuest(quest);

            ConfirmAction(sourceUnitController);
        }


        public override void EndInteraction() {
            base.EndInteraction();

            questGiver = null;
        }

        public void SetQuestGiver(QuestGiverComponent questGiverComponent, int optionIndex) {
            //Debug.Log($"QuestGiverManager.SetQuestGiver({optionIndex})");
            this.questGiver = questGiverComponent;
            
            BeginInteraction(questGiverComponent, optionIndex);
        }


    }

}
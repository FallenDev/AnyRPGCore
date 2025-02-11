using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class QuestGiverManager : InteractableOptionManager {

        private QuestGiverComponent questGiver = null;

        // game manager references
        private PlayerManager playerManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;
        private MessageFeedManager messageFeedManager = null;
        private LogManager logManager = null;
        private CurrencyConverter currencyConverter = null;

        public QuestGiverComponent QuestGiver { get => questGiver; set => questGiver = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            logManager = systemGameManager.LogManager;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public List<Quest> GetAvailableQuestList(UnitController sourceUnitController) {
            List<Quest> returnList = new List<Quest>();

            foreach (QuestNode questNode in questGiver.QuestGiverProps.Quests) {
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

        public void SetQuestGiver(QuestGiverComponent questGiverComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"QuestGiverManager.SetQuestGiver({optionIndex})");
            this.questGiver = questGiverComponent;
            
            BeginInteraction(questGiverComponent, componentIndex, choiceIndex);
        }

        public void CompleteQuest(UnitController sourceUnitController, Quest quest, QuestRewardChoices questRewardChoices) {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                CompleteQuestInternal(sourceUnitController, quest, questRewardChoices);
            } else {
                networkManagerClient.CompleteQuest(quest, questRewardChoices);
            }

        }

        public void CompleteQuestInternal(UnitController sourceUnitController, Quest currentQuest, QuestRewardChoices questRewardChoices) {
            //Debug.Log("QuestGiverUI.CompleteQuest()");
            if (!currentQuest.IsComplete(sourceUnitController)) {
                Debug.Log("QuestGiverManager.CompleteQuest(): currentQuest is not complete, exiting!");
                return;
            }

            bool itemCountMatches = false;
            bool abilityCountMatches = false;
            bool factionCountMatches = false;
            bool skillCountMatches = false;
            if (currentQuest.ItemRewards.Count == 0 || currentQuest.MaxItemRewards == 0 || currentQuest.MaxItemRewards == questRewardChoices.itemRewardIndexes.Count) {
                itemCountMatches = true;
            }
            if (currentQuest.FactionRewards.Count == 0 || currentQuest.MaxFactionRewards == 0 || currentQuest.MaxFactionRewards == questRewardChoices.factionRewardIndexes.Count) {
                factionCountMatches = true;
            }
            if (currentQuest.AbilityRewards.Count == 0 || currentQuest.MaxAbilityRewards == 0 || currentQuest.MaxAbilityRewards == questRewardChoices.abilityRewardIndexes.Count) {
                abilityCountMatches = true;
            }
            if (currentQuest.SkillRewards.Count == 0 || currentQuest.MaxSkillRewards == 0 || currentQuest.MaxSkillRewards == questRewardChoices.skillRewardIndexes.Count) {
                skillCountMatches = true;
            }

            if (!itemCountMatches || !abilityCountMatches || !factionCountMatches || !skillCountMatches) {
                messageFeedManager.WriteMessage(sourceUnitController, "You must choose rewards before turning in this quest");
                return;
            }

            // currency rewards
            List<CurrencyNode> currencyNodes = currentQuest.GetCurrencyReward(sourceUnitController);
            foreach (CurrencyNode currencyNode in currencyNodes) {
                sourceUnitController.CharacterCurrencyManager.AddCurrency(currencyNode.currency, currencyNode.Amount);
                List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                tmpCurrencyNode.Add(currencyNode);
                logManager.WriteSystemMessage(sourceUnitController, "Gained " + currencyConverter.RecalculateValues(tmpCurrencyNode, false).Value.Replace("\n", ", "));
            }

            // item rewards first in case not enough space in inventory
            // TO FIX: THIS CODE DOES NOT DEAL WITH PARTIAL STACKS AND WILL REQUEST ONE FULL SLOT FOR EVERY REWARD
            if (questRewardChoices.itemRewardIndexes.Count > 0) {
                if (sourceUnitController.CharacterInventoryManager.EmptySlotCount() < questRewardChoices.itemRewardIndexes.Count) {
                    messageFeedManager.WriteMessage(sourceUnitController, "Not enough room in inventory!");
                    return;
                }
                foreach (int rewardIndex in questRewardChoices.itemRewardIndexes) {
                    currentQuest.ItemRewards[rewardIndex].GiveReward(sourceUnitController);
                }
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedItemRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            currentQuest.HandInItems(sourceUnitController);

            // faction rewards
            if (questRewardChoices.factionRewardIndexes.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards");
                foreach (int rewardIndex in questRewardChoices.factionRewardIndexes) {
                    currentQuest.FactionRewards[rewardIndex].GiveReward(sourceUnitController);
                }
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedFactionRewardIcons()) {
                    //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards: got a reward button!");
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            // ability rewards
            if (questRewardChoices.abilityRewardIndexes.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Ability Rewards");
                foreach (int rewardIndex in questRewardChoices.abilityRewardIndexes) {
                    currentQuest.AbilityRewards[rewardIndex].AbilityProperties.GiveReward(sourceUnitController);
                }
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedAbilityRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            // skill rewards
            if (questRewardChoices.skillRewardIndexes.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Skill Rewards");
                foreach (int rewardIndex in questRewardChoices.skillRewardIndexes) {
                    currentQuest.SkillRewards[rewardIndex].GiveReward(sourceUnitController);
                }

                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedSkillRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(sourceUnitController);
                    }
                }
                */
            }

            // xp reward
            sourceUnitController.CharacterStats.GainXP(LevelEquations.GetXPAmountForQuest(sourceUnitController, currentQuest, systemConfigurationManager));

            //UpdateButtons(currentQuest);

            // DO THIS HERE OR TURNING THE QUEST RESULTING IN THIS WINDOW RE-OPENING WOULD JUST INSTA-CLOSE IT INSTEAD
            //uIManager.questGiverWindow.CloseWindow();

            sourceUnitController.CharacterQuestLog.TurnInQuest(currentQuest);

            // do this last
            // DO THIS AT THE END OR THERE WILL BE NO SELECTED QUESTGIVERQUESTSCRIPT
            if (questGiver != null) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): questGiver is not null");
                // MUST BE DONE IN CASE WINDOW WAS OPEN INBETWEEN SCENES BY ACCIDENT
                //Debug.Log("QuestGiverUI.CompleteQuest() Updating questGiver queststatus");
                questGiver.UpdateQuestStatus(sourceUnitController);
                questGiver.HandleCompleteQuest();
            } else {
                Debug.Log("QuestGiverUI.CompleteQuest(): questGiver is null!");
            }

            /*
            if (SelectedQuestGiverQuestScript != null) {
                SelectedQuestGiverQuestScript.DeSelect();
            }
            */

        }


    }

    public class QuestRewardChoices {
        public List<int> itemRewardIndexes = new List<int>();
        public List<int> factionRewardIndexes = new List<int>();
        public List<int> abilityRewardIndexes = new List<int>();
        public List<int> skillRewardIndexes = new List<int>();

        public QuestRewardChoices() {
        }
    }

}
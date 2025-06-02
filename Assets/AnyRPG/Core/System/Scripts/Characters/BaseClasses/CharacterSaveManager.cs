using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSaveManager : ConfiguredClass {

        private UnitController unitController;

        private AnyRPGSaveData saveData = null;


        private Dictionary<string, BehaviorSaveData> behaviorSaveDataDictionary = new Dictionary<string, BehaviorSaveData>();
        private Dictionary<string, DialogSaveData> dialogSaveDataDictionary = new Dictionary<string, DialogSaveData>();

        private Dictionary<string, SceneNodeSaveData> sceneNodeSaveDataDictionary = new Dictionary<string, SceneNodeSaveData>();

        private bool eventSubscriptionsInitialized = false;

        // game manager references
        private ActionBarManager actionBarManager = null;
        private SystemItemManager systemItemManager = null;
        private SaveManager saveManager = null;
        private LevelManager levelManager = null;

        public Dictionary<string, BehaviorSaveData> BehaviorSaveDataDictionary { get => behaviorSaveDataDictionary; set => behaviorSaveDataDictionary = value; }
        public Dictionary<string, DialogSaveData> DialogSaveDataDictionary { get => dialogSaveDataDictionary; set => dialogSaveDataDictionary = value; }

        public Dictionary<string, SceneNodeSaveData> SceneNodeSaveDataDictionary { get => sceneNodeSaveDataDictionary; set => sceneNodeSaveDataDictionary = value; }

        public AnyRPGSaveData SaveData { get => saveData; }

        public CharacterSaveManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            //saveData = saveManager.CreateSaveData().SaveData;
            saveData = new AnyRPGSaveData();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            systemItemManager = systemGameManager.SystemItemManager;
            saveManager = systemGameManager.SaveManager;
            levelManager = systemGameManager.LevelManager;
        }

        public void CreateEventSubscriptions() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized == false) {
                unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
                unitController.UnitEventController.OnGainXP += HandleGainXP;
                unitController.UnitEventController.OnNameChange += HandleNameChange;
                unitController.UnitEventController.OnFactionChange += HandleFactionChange;
                unitController.UnitEventController.OnRaceChange += HandleRaceChange;
                unitController.UnitEventController.OnClassChange += HandleClassChange;
                unitController.UnitEventController.OnSpecializationChange += HandleSpecializationChange;
                /*
                unitController.CharacterInventoryManager.OnInventoryChanged += SaveInventory;
                unitController.CharacterEquipmentManager.OnEquipmentChanged += SaveEquipment;
                unitController.CharacterQuestLog.OnQuestLogUpdated += SaveQuestLog;
                unitController.CharacterRecipeManager.OnRecipeListUpdated += SaveRecipeList;
                unitController.CharacterSkillManager.OnSkillListUpdated += SaveSkillList;
                unitController.CharacterAbilityManager.OnAbilityListUpdated += SaveAbilityList;
                unitController.CharacterFactionManager.OnReputationChanged += SaveReputation;
                */
                eventSubscriptionsInitialized = true;
            }
        }

        private void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            if (newSpecialization != null) {
                saveData.classSpecialization = newSpecialization.ResourceName;
            } else {
                saveData.classSpecialization = string.Empty;
            }
        }

        private void HandleClassChange(UnitController sourceUnitController, CharacterClass newClass, CharacterClass oldClass) {
            if (newClass != null) {
                saveData.characterClass = newClass.ResourceName;
            } else {
                saveData.characterClass = string.Empty;
            }
        }

        private void HandleRaceChange(CharacterRace newRace, CharacterRace oldRace) {
            if (newRace != null) {
                saveData.characterRace = newRace.ResourceName;
            } else {
                saveData.characterRace = string.Empty;
            }
        }

        private void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            if (newFaction != null) {
                saveData.playerFaction = newFaction.ResourceName;
            } else {
                saveData.playerFaction = string.Empty;
            }
        }

        private void HandleNameChange(string newName) {
            saveData.playerName = newName;
        }

        private void HandleGainXP(UnitController sourceUnitController, int gainedXP, int currentXP) {
            saveData.currentExperience = currentXP;
        }

        public void HandleLevelChanged(int newLevel) {
            saveData.PlayerLevel = newLevel;
        }

        public void SetSaveData(AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SetSaveData()");

            this.saveData = anyRPGSaveData;
        }

        public DialogSaveData GetDialogSaveData(Dialog dialog) {
            if (dialogSaveDataDictionary.ContainsKey(dialog.ResourceName)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): dialogSaveData size {dialogSaveDataDictionary[dialog.resourceName].dialogNodeShown.Count}");
                return dialogSaveDataDictionary[dialog.ResourceName];
            } else {
                DialogSaveData saveData = new DialogSaveData();
                saveData.dialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): initialized list size {dialog.DialogNodes.Count} : {saveData.dialogNodeShown.Count}");
                saveData.DialogName = dialog.ResourceName;
                dialogSaveDataDictionary.Add(dialog.ResourceName, saveData);
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): dialogSaveData size {dialogSaveDataDictionary[dialog.resourceName].dialogNodeShown.Count}");
                return saveData;
            }
            //return saveData;
        }

        public BehaviorSaveData GetBehaviorSaveData(BehaviorProfile behaviorProfile) {
            BehaviorSaveData saveData;
            if (behaviorSaveDataDictionary.ContainsKey(behaviorProfile.ResourceName)) {
                saveData = behaviorSaveDataDictionary[behaviorProfile.ResourceName];
            } else {
                saveData = new BehaviorSaveData();
                saveData.BehaviorName = behaviorProfile.ResourceName;
                behaviorSaveDataDictionary.Add(behaviorProfile.ResourceName, saveData);
            }
            return saveData;
        }

        public bool GetDialogNodeShown(Dialog dialog, int index) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogNodeShown({dialog.ResourceName}, {index})");
            DialogSaveData saveData = GetDialogSaveData(dialog);
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogNodeShown({dialog.ResourceName}, {index}) count: {saveData.dialogNodeShown.Count}");
            return saveData.dialogNodeShown[index];
        }

        public void SetDialogNodeShown(Dialog dialog, bool value, int index) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.dialogNodeShown[index] = value;
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
        }

        public void ResetDialogNodes(Dialog dialog) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.dialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
        }

        public SceneNodeSaveData GetSceneNodeSaveData(SceneNode sceneNode) {
            SceneNodeSaveData saveData;
            if (sceneNodeSaveDataDictionary.ContainsKey(sceneNode.ResourceName)) {
                saveData = sceneNodeSaveDataDictionary[sceneNode.ResourceName];
            } else {
                saveData = new SceneNodeSaveData();
                saveData.persistentObjects = new List<PersistentObjectSaveData>();
                saveData.SceneName = sceneNode.ResourceName;
                sceneNodeSaveDataDictionary.Add(sceneNode.ResourceName, saveData);
            }
            return saveData;
        }


        public void LoadSaveDataToCharacter(AnyRPGSaveData saveData) {
            Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSaveDataToCharacter()");

            SetSaveData(saveData);

            LoadDialogData(saveData);
            LoadBehaviorData(saveData);
            LoadSceneNodeData(saveData);

            // complex data
            LoadEquippedBagData(saveData);
            LoadInventorySlotData(saveData);
            LoadBankSlotData(saveData);
            LoadAbilityData(saveData);

            // THIS NEEDS TO BE DOWN HERE SO THE PLAYERSTATS EXISTS TO SUBSCRIBE TO THE EQUIP EVENTS AND INCREASE STATS
            // testing - move here to prevent learning auto-attack ability twice
            LoadEquipmentData(saveData);

            LoadSkillData(saveData);
            LoadRecipeData(saveData);
            LoadReputationData(saveData);

            // test loading this earlier to avoid having duplicates on bars
            LoadActionBarData(saveData);

            LoadCurrencyData(saveData);
            LoadStatusEffectData(saveData);
            LoadPetData(saveData);


            // set resources after equipment loaded for modifiers
            LoadResourcePowerData(saveData);

            // quest data gets loaded last because it could rely on other data such as dialog completion status, which don't get saved because they are inferred
            LoadQuestData(saveData);
            LoadAchievementData(saveData);

        }

        public void LoadDialogData(AnyRPGSaveData anyRPGSaveData) {
            unitController.CharacterSaveManager.DialogSaveDataDictionary.Clear();
            foreach (DialogSaveData dialogSaveData in anyRPGSaveData.dialogSaveData) {
                if (dialogSaveData.DialogName != null && dialogSaveData.DialogName != string.Empty) {
                    unitController.CharacterSaveManager.DialogSaveDataDictionary.Add(dialogSaveData.DialogName, dialogSaveData);
                }
            }
        }

        public void LoadBehaviorData(AnyRPGSaveData anyRPGSaveData) {
            unitController.CharacterSaveManager.BehaviorSaveDataDictionary.Clear();
            foreach (BehaviorSaveData behaviorSaveData in anyRPGSaveData.behaviorSaveData) {
                if (behaviorSaveData.BehaviorName != null && behaviorSaveData.BehaviorName != string.Empty) {
                    unitController.CharacterSaveManager.BehaviorSaveDataDictionary.Add(behaviorSaveData.BehaviorName, behaviorSaveData);
                }
            }
        }

        public void LoadSceneNodeData(AnyRPGSaveData anyRPGSaveData) {
            sceneNodeSaveDataDictionary.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in anyRPGSaveData.sceneNodeSaveData) {
                if (sceneNodeSaveData.SceneName != null && sceneNodeSaveData.SceneName != string.Empty) {
                    sceneNodeSaveDataDictionary.Add(sceneNodeSaveData.SceneName, sceneNodeSaveData);
                }
            }
        }

        public void LoadQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadQuestData()");
            unitController.CharacterQuestLog.QuestSaveDataDictionary.Clear();
            unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {

                if (questSaveData.QuestName == null || questSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                unitController.CharacterQuestLog.QuestSaveDataDictionary.Add(questSaveData.QuestName, questSaveData);

                Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();

                // add objectives to dictionary
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.questObjectives) {
                    // perform null check to allow opening of older save files without null reference
                    if (questObjectiveSaveData.ObjectiveType != null && questObjectiveSaveData.ObjectiveType != string.Empty) {
                        if (!objectiveDictionary.ContainsKey(questObjectiveSaveData.ObjectiveType)) {
                            objectiveDictionary.Add(questObjectiveSaveData.ObjectiveType, new Dictionary<string, QuestObjectiveSaveData>());
                        }
                        objectiveDictionary[questObjectiveSaveData.ObjectiveType].Add(questObjectiveSaveData.ObjectiveName, questObjectiveSaveData);
                    }
                }

                unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.Add(questSaveData.QuestName, objectiveDictionary);
            }

            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                unitController.CharacterQuestLog.LoadQuest(questSaveData);
            }
        }

        public void LoadAchievementData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAchievementData()");
            unitController.CharacterQuestLog.AchievementSaveDataDictionary.Clear();
            unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData achievementSaveData in anyRPGSaveData.achievementSaveData) {

                if (achievementSaveData.QuestName == null || achievementSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                unitController.CharacterQuestLog.AchievementSaveDataDictionary.Add(achievementSaveData.QuestName, achievementSaveData);

                Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();

                // add objectives to dictionary
                foreach (QuestObjectiveSaveData achievementObjectiveSaveData in achievementSaveData.questObjectives) {
                    // perform null check to allow opening of older save files without null reference
                    if (achievementObjectiveSaveData.ObjectiveType != null && achievementObjectiveSaveData.ObjectiveType != string.Empty) {
                        if (!objectiveDictionary.ContainsKey(achievementObjectiveSaveData.ObjectiveType)) {
                            objectiveDictionary.Add(achievementObjectiveSaveData.ObjectiveType, new Dictionary<string, QuestObjectiveSaveData>());
                        }
                        objectiveDictionary[achievementObjectiveSaveData.ObjectiveType].Add(achievementObjectiveSaveData.ObjectiveName, achievementObjectiveSaveData);
                    }
                }

                unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.Add(achievementSaveData.QuestName, objectiveDictionary);
            }

            foreach (QuestSaveData questSaveData in anyRPGSaveData.achievementSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                unitController.CharacterQuestLog.AcceptAchievement(questSaveData);
            }
        }
        public void LoadResourcePowerData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadResourcePowerData()");

            foreach (ResourcePowerSaveData resourcePowerSaveData in anyRPGSaveData.resourcePowerSaveData) {
                //Debug.Log("Savemanager.LoadResourcePowerData(): loading questsavedata");
                unitController.CharacterStats.SetResourceAmount(resourcePowerSaveData.ResourceName, resourcePowerSaveData.amount);
            }

        }

        public void LoadStatusEffectData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadStatusEffectData()");
            foreach (StatusEffectSaveData statusEffectSaveData in anyRPGSaveData.statusEffectSaveData) {
                unitController.CharacterAbilityManager.ApplySavedStatusEffects(statusEffectSaveData);
            }
        }

        public void LoadCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadCurrencyData()");
            foreach (CurrencySaveData currencySaveData in anyRPGSaveData.currencySaveData) {
                unitController.CharacterCurrencyManager.AddCurrency(systemDataFactory.GetResource<Currency>(currencySaveData.CurrencyName), currencySaveData.Amount);
            }
        }


        public void LoadPetData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (PetSaveData petSaveData in anyRPGSaveData.petSaveData) {
                if (petSaveData.PetName != string.Empty) {
                    unitController.CharacterPetManager.AddPet(petSaveData.PetName);
                }
            }

        }

        public void LoadEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadEquippedBagData()");
            unitController.CharacterInventoryManager.LoadEquippedBagData(anyRPGSaveData.equippedBagSaveData, false);
            unitController.CharacterInventoryManager.LoadEquippedBagData(anyRPGSaveData.equippedBankBagSaveData, true);
        }

        public void LoadInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadInventorySlotData()");
            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in anyRPGSaveData.inventorySlotSaveData) {
                LoadSlotData(inventorySlotSaveData, counter, false);
                counter++;
            }
        }

        public void LoadBankSlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadBankSlotData()");
            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in anyRPGSaveData.bankSlotSaveData) {
                LoadSlotData(inventorySlotSaveData, counter, true);
                counter++;
            }
        }

        private void LoadSlotData(InventorySlotSaveData inventorySlotSaveData, int counter, bool bank) {
            if (inventorySlotSaveData.ItemName != string.Empty && inventorySlotSaveData.ItemName != null) {
                for (int i = 0; i < inventorySlotSaveData.stackCount; i++) {
                    InstantiatedItem newInstantiatedItem = unitController.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData(inventorySlotSaveData.ItemName, inventorySlotSaveData);
                    if (newInstantiatedItem == null) {
                        Debug.Log("Savemanager.LoadInventorySlotData(): COULD NOT LOAD ITEM FROM ITEM MANAGER: " + inventorySlotSaveData.ItemName);
                    } else {
                        if (bank == true) {
                            unitController.CharacterInventoryManager.AddBankItem(newInstantiatedItem, counter);
                        } else {
                            unitController.CharacterInventoryManager.AddInventoryItem(newInstantiatedItem, counter);
                        }
                    }
                }
            }
        }

        public void LoadAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (AbilitySaveData abilitySaveData in anyRPGSaveData.abilitySaveData) {
                if (abilitySaveData.AbilityName != string.Empty) {
                    unitController.CharacterAbilityManager.LoadAbility(abilitySaveData.AbilityName);
                }
            }
        }

        public void LoadEquipmentData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadEquipmentData()");

            foreach (EquipmentSaveData equipmentSaveData in anyRPGSaveData.equipmentSaveData) {
                if (equipmentSaveData.EquipmentName != string.Empty) {
                    InstantiatedEquipment newInstantiatedEquipment = unitController.CharacterInventoryManager.GetNewInstantiatedItem(equipmentSaveData.EquipmentName) as InstantiatedEquipment;
                    if (newInstantiatedEquipment != null) {
                        newInstantiatedEquipment.DisplayName = equipmentSaveData.DisplayName;
                        newInstantiatedEquipment.DropLevel = equipmentSaveData.dropLevel;
                        if (equipmentSaveData.itemQuality != null && equipmentSaveData.itemQuality != string.Empty) {
                            newInstantiatedEquipment.ItemQuality = systemDataFactory.GetResource<ItemQuality>(equipmentSaveData.itemQuality);
                        }
                        if (equipmentSaveData.randomSecondaryStatIndexes != null) {
                            newInstantiatedEquipment.RandomStatIndexes = equipmentSaveData.randomSecondaryStatIndexes;
                            newInstantiatedEquipment.InitializeRandomStatsFromIndex();
                        }
                        unitController.CharacterEquipmentManager.Equip(newInstantiatedEquipment, null);
                    }
                }
            }
        }

        public void LoadReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadReputationData()");
            //int counter = 0;
            foreach (ReputationSaveData reputationSaveData in anyRPGSaveData.reputationSaveData) {
                FactionDisposition factionDisposition = new FactionDisposition();
                factionDisposition.Faction = systemDataFactory.GetResource<Faction>(reputationSaveData.ReputationName);
                factionDisposition.disposition = reputationSaveData.Amount;
                unitController.CharacterFactionManager.LoadReputation(factionDisposition.Faction, (int)factionDisposition.disposition);
                //counter++;
            }
        }


        public void LoadSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadSkillData()");
            foreach (SkillSaveData skillSaveData in anyRPGSaveData.skillSaveData) {
                unitController.CharacterSkillManager.LoadSkill(skillSaveData.SkillName);
            }
        }

        public void LoadRecipeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadRecipeData()");
            foreach (RecipeSaveData recipeSaveData in anyRPGSaveData.recipeSaveData) {
                unitController.CharacterRecipeManager.LoadRecipe(recipeSaveData.RecipeName);
            }
        }

        public void LoadActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadActionBarData()");

            LoadActionButtonData(anyRPGSaveData.actionBarSaveData, actionBarManager.GetMouseActionButtons());
            LoadGamepadActionButtonData(anyRPGSaveData.gamepadActionBarSaveData, actionBarManager.GamepadActionButtons);
            actionBarManager.SetGamepadActionButtonSet(anyRPGSaveData.GamepadActionButtonSet, false);
            actionBarManager.UpdateVisuals();
        }

        private void LoadActionButtonData(List<ActionBarSaveData> actionBarSaveDatas, List<ActionButton> actionButtons) {
            IUseable useable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in actionBarSaveDatas) {
                useable = null;
                if (actionBarSaveData.isItem == true) {
                    // find item in bag
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory");
                    //useable = systemDataFactory.GetResource<Item>(actionBarSaveData.DisplayName);
                    useable = systemItemManager.GetNewInstantiatedItem(actionBarSaveData.DisplayName);
                } else {
                    // find ability from system ability manager
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.DisplayName != null && actionBarSaveData.DisplayName != string.Empty) {
                        useable = systemDataFactory.GetResource<Ability>(actionBarSaveData.DisplayName).AbilityProperties;
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): saved action bar had no name");
                    }
                    if (actionBarSaveData.savedName != null && actionBarSaveData.savedName != string.Empty) {
                        IUseable savedUseable = systemDataFactory.GetResource<Ability>(actionBarSaveData.savedName).AbilityProperties;
                        if (savedUseable != null) {
                            actionButtons[counter].SavedUseable = savedUseable;
                        }
                    }
                }
                if (useable != null) {
                    actionButtons[counter].SetUseable(useable, false);
                } else {
                    //Debug.Log("Savemanager.LoadActionBarData(): no usable set on this actionbutton");
                    // testing remove things that weren't saved, it will prevent duplicate abilities if they are moved
                    // this means if new abilities are added to a class/etc between play sessions they won't be on the bars
                    actionButtons[counter].ClearUseable();
                }
                counter++;
            }
        }

        private void LoadGamepadActionButtonData(List<ActionBarSaveData> actionBarSaveDatas, List<ActionButtonNode> actionButtons) {
            IUseable useable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in actionBarSaveDatas) {
                useable = null;
                if (actionBarSaveData.isItem == true) {
                    // find item in bag
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory");
                    //useable = systemDataFactory.GetResource<Item>(actionBarSaveData.DisplayName);
                    useable = systemItemManager.GetNewInstantiatedItem(actionBarSaveData.DisplayName);
                } else {
                    // find ability from system ability manager
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.DisplayName != null && actionBarSaveData.DisplayName != string.Empty) {
                        useable = systemDataFactory.GetResource<Ability>(actionBarSaveData.DisplayName).AbilityProperties;
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): saved action bar had no name");
                    }
                    if (actionBarSaveData.savedName != null && actionBarSaveData.savedName != string.Empty) {
                        IUseable savedUseable = systemDataFactory.GetResource<Ability>(actionBarSaveData.savedName).AbilityProperties;
                        if (savedUseable != null) {
                            actionButtons[counter].SavedUseable = savedUseable;
                        }
                    }
                }
                if (useable != null) {
                    actionButtons[counter].Useable = useable;
                } else {
                    //Debug.Log("Savemanager.LoadActionBarData(): no usable set on this actionbutton");
                    // testing remove things that weren't saved, it will prevent duplicate abilities if they are moved
                    // this means if new abilities are added to a class/etc between play sessions they won't be on the bars
                    actionButtons[counter].Useable = null;
                }
                counter++;
            }
        }

        public void VisitSceneNode(SceneNode sceneNode) {
            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            if (saveData.visited == false) {
                saveData.visited = true;
                sceneNodeSaveDataDictionary[saveData.SceneName] = saveData;
            }
        }

        public bool IsSceneNodeVisited(SceneNode sceneNode) {
            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            return saveData.visited;
        }

        public void SavePlayerLocation() {
            saveData.OverrideLocation = true;
            saveData.OverrideRotation = true;
            saveData.PlayerLocationX = unitController.transform.position.x;
            saveData.PlayerLocationY = unitController.transform.position.y;
            saveData.PlayerLocationZ = unitController.transform.position.z;
            saveData.PlayerRotationX = unitController.transform.forward.x;
            saveData.PlayerRotationY = unitController.transform.forward.y;
            saveData.PlayerRotationZ = unitController.transform.forward.z;

        }


        public void SaveGameData() {
            saveData.unitProfileName = unitController.UnitProfile.ResourceName;

            SavePlayerLocation();
            saveData.CurrentScene = levelManager.ActiveSceneName;
            saveData.GamepadActionButtonSet = actionBarManager.CurrentActionBarSet;

            SaveResourcePowerData();
            SaveAppearanceData();

            SaveQuestData();
            SaveAchievementData();

            SaveDialogData();
            SaveBehaviorData();
            SaveActionBarData();
            SaveInventorySlotData();
            SaveBankSlotData();
            SaveEquippedBagData();
            SaveEquippedBankBagData();
            SaveAbilityData();
            SaveSkillData();
            SaveRecipeData();
            SaveReputationData();
            SaveEquipmentData();
            SaveCurrencyData();
            SaveSceneNodeData();
            SaveStatusEffectData();
            SavePetData();
        }

        public void SaveResourcePowerData() {
            saveData.resourcePowerSaveData.Clear();
            foreach (PowerResource powerResource in unitController.CharacterStats.PowerResourceDictionary.Keys) {
                ResourcePowerSaveData resourcePowerData = new ResourcePowerSaveData();
                resourcePowerData.ResourceName = powerResource.ResourceName;
                resourcePowerData.amount = unitController.CharacterStats.PowerResourceDictionary[powerResource].currentValue;
                saveData.resourcePowerSaveData.Add(resourcePowerData);
            }
        }

        public void SaveAppearanceData() {
            unitController.UnitModelController.SaveAppearanceSettings(/*this,*/ saveData);
        }

        public void SaveQuestData() {
            //Debug.Log("Savemanager.SaveQuestData()");

            saveData.questSaveData.Clear();
            foreach (QuestSaveData questSaveData in unitController.CharacterQuestLog.QuestSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = questSaveData;
                if (unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.ContainsKey(questSaveData.QuestName)) {

                    List<QuestObjectiveSaveData> questObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary[questSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary[questSaveData.QuestName][typeName].Values) {
                            questObjectiveSaveDataList.Add(saveData);
                        }
                    }
                    finalSaveData.questObjectives = questObjectiveSaveDataList;
                }
                finalSaveData.inLog = unitController.CharacterQuestLog.HasQuest(questSaveData.QuestName);
                saveData.questSaveData.Add(finalSaveData);
            }
        }

        public void SaveAchievementData() {
            //Debug.Log("Savemanager.SaveAchievementData()");

            saveData.achievementSaveData.Clear();
            foreach (QuestSaveData achievementSaveData in unitController.CharacterQuestLog.AchievementSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = achievementSaveData;
                if (unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.ContainsKey(achievementSaveData.QuestName)) {

                    List<QuestObjectiveSaveData> achievementObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary[achievementSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary[achievementSaveData.QuestName][typeName].Values) {
                            achievementObjectiveSaveDataList.Add(saveData);
                        }

                    }

                    finalSaveData.questObjectives = achievementObjectiveSaveDataList;
                }
                finalSaveData.inLog = unitController.CharacterQuestLog.HasAchievement(achievementSaveData.QuestName);
                saveData.achievementSaveData.Add(finalSaveData);
            }
        }

        public void SaveDialogData() {
            //Debug.Log("Savemanager.SaveDialogData()");
            saveData.dialogSaveData.Clear();
            foreach (DialogSaveData dialogSaveData in unitController.CharacterSaveManager.DialogSaveDataDictionary.Values) {
                saveData.dialogSaveData.Add(dialogSaveData);
            }
        }

        public void SaveBehaviorData() {
            //Debug.Log("Savemanager.SaveQuestData()");
            saveData.behaviorSaveData.Clear();
            foreach (BehaviorSaveData behaviorSaveData in unitController.CharacterSaveManager.BehaviorSaveDataDictionary.Values) {
                saveData.behaviorSaveData.Add(behaviorSaveData);
            }
        }

        public void SaveSceneNodeData() {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            saveData.sceneNodeSaveData.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in sceneNodeSaveDataDictionary.Values) {
                saveData.sceneNodeSaveData.Add(sceneNodeSaveData);
            }
        }

        public void SaveStatusEffectData() {
            //Debug.Log("Savemanager.SaveSceneNodeData()");
            saveData.statusEffectSaveData.Clear();
            foreach (StatusEffectNode statusEffectNode in unitController.CharacterStats.StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ClassTrait == false
                    && statusEffectNode.StatusEffect.SaveEffect == true
                    && statusEffectNode.AbilityEffectContext.AbilityCaster == (unitController as IAbilityCaster)) {
                    StatusEffectSaveData statusEffectSaveData = new StatusEffectSaveData();
                    statusEffectSaveData.StatusEffectName = statusEffectNode.StatusEffect.DisplayName;
                    statusEffectSaveData.remainingSeconds = (int)statusEffectNode.GetRemainingDuration();
                    saveData.statusEffectSaveData.Add(statusEffectSaveData);
                }
            }
        }

        public void SaveActionBarData() {
            //Debug.Log("Savemanager.SaveActionBarData()");
            saveData.actionBarSaveData.Clear();
            saveData.gamepadActionBarSaveData.Clear();
            foreach (ActionButton actionButton in actionBarManager.GetMouseActionButtons()) {
                SaveActionButtonSaveData(actionButton, saveData.actionBarSaveData);
            }
            foreach (ActionButtonNode actionButtonNode in actionBarManager.GamepadActionButtons) {
                SaveActionButtonNodeSaveData(actionButtonNode, saveData.gamepadActionBarSaveData);
            }
        }

        private void SaveActionButtonSaveData(ActionButton actionButton, List<ActionBarSaveData> actionBarSaveDataList) {
            ActionBarSaveData actionBarSaveData = new ActionBarSaveData();
            actionBarSaveData.DisplayName = (actionButton.Useable == null ? string.Empty : (actionButton.Useable as IDescribable).DisplayName);
            actionBarSaveData.savedName = (actionButton.SavedUseable == null ? string.Empty : (actionButton.SavedUseable as IDescribable).DisplayName);
            actionBarSaveData.isItem = (actionButton.Useable == null ? false : (actionButton.Useable is Item ? true : false));
            actionBarSaveDataList.Add(actionBarSaveData);
        }

        private void SaveActionButtonNodeSaveData(ActionButtonNode actionButtonNode, List<ActionBarSaveData> actionBarSaveDataList) {
            ActionBarSaveData actionBarSaveData = new ActionBarSaveData();
            actionBarSaveData.DisplayName = (actionButtonNode.Useable == null ? string.Empty : (actionButtonNode.Useable as IDescribable).DisplayName);
            actionBarSaveData.savedName = (actionButtonNode.SavedUseable == null ? string.Empty : (actionButtonNode.SavedUseable as IDescribable).DisplayName);
            actionBarSaveData.isItem = (actionButtonNode.Useable == null ? false : (actionButtonNode.Useable is Item ? true : false));
            actionBarSaveDataList.Add(actionBarSaveData);
        }

        private InventorySlotSaveData GetSlotSaveData(InventorySlot inventorySlot) {
            InventorySlotSaveData saveData = new InventorySlotSaveData();
            if (inventorySlot.InstantiatedItem != null) {
                saveData = inventorySlot.InstantiatedItem.GetSlotSaveData();
            } else {
                saveData = saveManager.GetEmptySlotSaveData();
            }
            saveData.stackCount = (inventorySlot.InstantiatedItem == null ? 0 : inventorySlot.Count);
            return saveData;
        }

        public void SaveInventorySlotData() {
            //Debug.Log("Savemanager.SaveInventorySlotData()");
            saveData.inventorySlotSaveData.Clear();
            foreach (InventorySlot inventorySlot in unitController.CharacterInventoryManager.InventorySlots) {
                saveData.inventorySlotSaveData.Add(GetSlotSaveData(inventorySlot));
            }
        }

        public void SaveBankSlotData() {
            //Debug.Log("Savemanager.SaveBankSlotData()");
            saveData.bankSlotSaveData.Clear();
            foreach (InventorySlot inventorySlot in unitController.CharacterInventoryManager.BankSlots) {
                saveData.bankSlotSaveData.Add(GetSlotSaveData(inventorySlot));
            }
        }

        public void SaveReputationData() {
            //Debug.Log("Savemanager.SaveReputationData()");
            saveData.reputationSaveData.Clear();
            foreach (FactionDisposition factionDisposition in unitController.CharacterFactionManager.DispositionDictionary) {
                if (factionDisposition == null) {
                    Debug.Log("Savemanager.SaveReputationData(): no disposition");
                    continue;
                }
                if (factionDisposition.Faction == null) {
                    Debug.Log("Savemanager.SaveReputationData() no faction");
                    continue;
                }
                ReputationSaveData reputationSaveData = new ReputationSaveData();
                reputationSaveData.ReputationName = factionDisposition.Faction.ResourceName;
                reputationSaveData.Amount = factionDisposition.disposition;
                saveData.reputationSaveData.Add(reputationSaveData);
            }
        }

        public void SaveCurrencyData() {
            //Debug.Log("Savemanager.SaveCurrencyData()");
            saveData.currencySaveData.Clear();
            foreach (CurrencyNode currencyNode in unitController.CharacterCurrencyManager.CurrencyList.Values) {
                CurrencySaveData currencySaveData = new CurrencySaveData();
                currencySaveData.Amount = currencyNode.Amount;
                currencySaveData.CurrencyName = currencyNode.currency.ResourceName;
                saveData.currencySaveData.Add(currencySaveData);
            }
        }

        public void SaveEquippedBagData() {
            //Debug.Log("Savemanager.SaveEquippedBagData()");
            saveData.equippedBagSaveData.Clear();
            foreach (BagNode bagNode in unitController.CharacterInventoryManager.BagNodes) {
                //Debug.Log("Savemanager.SaveEquippedBagData(): got bagNode");
                saveData.equippedBagSaveData.Add(GetBagSaveData(bagNode));
            }
        }

        public void SaveEquippedBankBagData() {
            //Debug.Log("Savemanager.SaveEquippedBagData()");
            saveData.equippedBankBagSaveData.Clear();
            foreach (BagNode bagNode in unitController.CharacterInventoryManager.BankNodes) {
                //Debug.Log("Savemanager.SaveEquippedBagData(): got bagNode");
                saveData.equippedBankBagSaveData.Add(GetBagSaveData(bagNode));
            }
        }

        private EquippedBagSaveData GetBagSaveData(BagNode bagNode) {
            EquippedBagSaveData saveData = new EquippedBagSaveData();
            saveData.BagName = (bagNode.InstantiatedBag != null ? bagNode.InstantiatedBag.ResourceName : string.Empty);
            saveData.slotCount = (bagNode.InstantiatedBag != null ? bagNode.InstantiatedBag.Slots : 0);

            return saveData;
        }

        public void SaveAbilityData() {
            //Debug.Log("Savemanager.SaveAbilityData()");
            saveData.abilitySaveData.Clear();
            foreach (AbilityProperties baseAbility in unitController.CharacterAbilityManager.RawAbilityList.Values) {
                AbilitySaveData abilitySaveData = new AbilitySaveData();
                abilitySaveData.AbilityName = baseAbility.DisplayName;
                saveData.abilitySaveData.Add(abilitySaveData);
            }
        }

        public void SavePetData() {
            //Debug.Log("Savemanager.SaveAbilityData()");
            saveData.petSaveData.Clear();
            foreach (UnitProfile unitProfile in unitController.CharacterPetManager.UnitProfiles) {
                PetSaveData petSaveData = new PetSaveData();
                petSaveData.PetName = unitProfile.ResourceName;
                saveData.petSaveData.Add(petSaveData);
            }
        }

        public void SaveEquipmentData() {
            //Debug.Log("Savemanager.SaveEquipmentData()");
            saveData.equipmentSaveData.Clear();
            if (unitController.CharacterEquipmentManager != null) {
                foreach (EquipmentInventorySlot equipmentInventorySlot in unitController.CharacterEquipmentManager.CurrentEquipment.Values) {
                    EquipmentSaveData equipmentSaveData = new EquipmentSaveData();
                    equipmentSaveData.EquipmentName = (equipmentInventorySlot.InstantiatedEquipment == null ? string.Empty : equipmentInventorySlot.InstantiatedEquipment.ResourceName);
                    equipmentSaveData.DisplayName = (equipmentInventorySlot.InstantiatedEquipment == null ? string.Empty : equipmentInventorySlot.InstantiatedEquipment.DisplayName);
                    if (equipmentInventorySlot.InstantiatedEquipment != null) {
                        if (equipmentInventorySlot.InstantiatedEquipment.ItemQuality != null) {
                            equipmentSaveData.itemQuality = (equipmentInventorySlot.InstantiatedEquipment == null ? string.Empty : equipmentInventorySlot.InstantiatedEquipment.ItemQuality.ResourceName);
                        }
                        equipmentSaveData.dropLevel = equipmentInventorySlot.InstantiatedEquipment.DropLevel;
                        equipmentSaveData.randomSecondaryStatIndexes = (equipmentInventorySlot.InstantiatedEquipment == null ? null : equipmentInventorySlot.InstantiatedEquipment.RandomStatIndexes);
                    }
                    saveData.equipmentSaveData.Add(equipmentSaveData);
                }
            }
        }

        public void SaveSkillData() {
            //Debug.Log("Savemanager.SaveSkillData()");
            saveData.skillSaveData.Clear();
            foreach (string skillName in unitController.CharacterSkillManager.MySkillList.Keys) {
                SkillSaveData skillSaveData = new SkillSaveData();
                skillSaveData.SkillName = skillName;
                saveData.skillSaveData.Add(skillSaveData);
            }
        }

        public void SaveRecipeData() {
            //Debug.Log("Savemanager.SaveRecipeData()");
            saveData.recipeSaveData.Clear();
            foreach (string recipeName in unitController.CharacterRecipeManager.RecipeList.Keys) {
                RecipeSaveData recipeSaveData = new RecipeSaveData();
                recipeSaveData.RecipeName = recipeName;
                saveData.recipeSaveData.Add(recipeSaveData);
            }
        }
    }

}
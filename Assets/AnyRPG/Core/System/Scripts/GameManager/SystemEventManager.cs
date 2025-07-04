using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEventManager {

        private static Dictionary<string, Action<string, EventParamProperties>> singleEventDictionary = new Dictionary<string, Action<string, EventParamProperties>>();

        public event System.Action<UnitController> OnPlayerUnitSpawn = delegate { };
        public event System.Action<UnitController> OnPlayerUnitDespawn = delegate { };
        public event System.Action<UnitController, AbilityProperties> OnAbilityUsed = delegate { };
        public event System.Action<UnitController, AbilityProperties> OnAbilityListChanged = delegate { };
        public event System.Action<UnitController, int> OnLevelChanged = delegate { };
        public event System.Action<UnitController, CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<UnitController, ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<UnitController, string> OnInteractionStarted = delegate { };
        public event System.Action<UnitController, InteractableOptionComponent> OnInteractionWithOptionStarted = delegate { };
        public event System.Action<UnitController, Interactable> OnInteractionCompleted = delegate { };
        public event System.Action<UnitController, InteractableOptionComponent> OnInteractionWithOptionCompleted = delegate { };
        public event System.Action<UnitController, Item> OnItemCountChanged = delegate { };
        public event System.Action<UnitController, Dialog> OnDialogCompleted = delegate { };
        public event System.Action<IAbilityCaster, UnitController, int, string> OnTakeDamage = delegate { };
        public event System.Action<UnitController> OnReputationChange = delegate { };
        public event System.Action<UnitController, QuestBase> OnAcceptQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnRemoveQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnMarkQuestComplete = delegate { };
        public event System.Action<UnitController, QuestBase> OnQuestObjectiveStatusUpdated = delegate { };
        public event System.Action<UnitController, Skill> OnLearnSkill = delegate { };
        public event System.Action<UnitController, Skill> OnUnLearnSkill = delegate { };
        public event System.Action<UnitController, CraftAbilityProperties> OnSetCraftAbility = delegate { };
        public event System.Action OnCraftItem = delegate { };
        public event System.Action OnAddBag = delegate { };
        public event System.Action OnCurrencyChange = delegate { };
        public event System.Action<IUseable, int> OnSetGamepadActionButton = delegate { };
        public event System.Action<int> OnUnsetGamepadActionButton = delegate { };
        public event System.Action<IUseable, int> OnSetMouseActionButton = delegate { };
        public event System.Action<int> OnUnsetMouseActionButton = delegate { };
        public event System.Action<UnitProfile> OnRemoveActivePet = delegate { };

        // equipment manager
        public System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnAddEquipment = delegate { };
        public System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnRemoveEquipment = delegate { };

        public static void StartListening(string eventName, Action<string, EventParamProperties> listener) {
            Action<string, EventParamProperties> thisEvent;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {

                //Add more event to the existing one
                thisEvent += listener;

                //Update the Dictionary
                singleEventDictionary[eventName] = thisEvent;
            } else {
                //Add event to the Dictionary for the first time
                thisEvent += listener;

                singleEventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action<string, EventParamProperties> listener) {

            Action<string, EventParamProperties> thisEvent;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {

                //Remove event from the existing one
                thisEvent -= listener;

                //Update the Dictionary
                singleEventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, EventParamProperties eventParam) {
            Action<string, EventParamProperties> thisEvent = null;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {
                if (thisEvent != null) {
                    thisEvent.Invoke(eventName, eventParam);
                }
                // OR USE  instance.eventDictionary[eventName](eventParam);
            }
        }

        public void NotifyOnReputationChange(UnitController sourceUnitController) {
            OnReputationChange(sourceUnitController);
        }

        public void NotifyOnPlayerUnitSpawn(UnitController unitController) {
            OnPlayerUnitSpawn(unitController);
        }

        public void NotifyOnPlayerUnitDespawn(UnitController unitController) {
            OnPlayerUnitDespawn(unitController);
        }

        public void NotifyOnAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            OnAddEquipment(profile, equipment);
        }

        public void NotifyOnRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            OnRemoveEquipment(profile, equipment);
        }

        public void NotifyOnClassChange(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(sourceUnitController, newCharacterClass, oldCharacterClass);
        }

        public void NotifyOnSpecializationChange(UnitController sourceUnitController, ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(sourceUnitController, newClassSpecialization, oldClassSpecialization);
        }

        public void NotifyOnTakeDamage(IAbilityCaster source, UnitController target, int damage, string abilityName) {
            OnTakeDamage(source, target, damage, abilityName);
        }

        public void NotifyOnDialogCompleted(UnitController sourceUnitController, Dialog dialog) {
            OnDialogCompleted(sourceUnitController, dialog);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnInteractionStarted(UnitController sourceUnitController, string interactableName) {
            //Debug.Log("SystemEventManager.NotifyOnInteractionStarted(" + interactableName + ")");
            OnInteractionStarted(sourceUnitController, interactableName);
        }

        public void NotifyOnInteractionWithOptionStarted(UnitController sourceUnitController, InteractableOptionComponent interactableOption) {
            //Debug.Log("SystemEventManager.NotifyOnInteractionWithOptionStarted(" + interactableOption.DisplayName + ")");
            OnInteractionWithOptionStarted(sourceUnitController, interactableOption);
        }

        public void NotifyOnInteractionCompleted(UnitController sourceUnitController, Interactable interactable) {
            OnInteractionCompleted(sourceUnitController, interactable);
        }

        public void NotifyOnInteractionWithOptionCompleted(UnitController sourceUnitController, InteractableOptionComponent interactableOption) {
            OnInteractionWithOptionCompleted(sourceUnitController, interactableOption);
        }

        public void NotifyOnLevelChanged(UnitController sourceUnitController, int newLevel) {
            OnLevelChanged(sourceUnitController, newLevel);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnAbilityListChanged(UnitController sourceUnitController, AbilityProperties newAbility) {
            //Debug.Log($"SystemEventManager.NotifyOnAbilityListChanged({newAbility})");

            OnAbilityListChanged(sourceUnitController, newAbility);
            //OnPrerequisiteUpdated();
        }
        

        public void NotifyOnAbilityUsed(UnitController sourceUnitController, AbilityProperties ability) {
            //Debug.Log("SystemEventManager.NotifyAbilityused(" + ability.DisplayName + ")");
            OnAbilityUsed(sourceUnitController, ability);
        }

        public void NotifyOnItemCountChanged(UnitController sourceUnitController, Item item) {
            OnItemCountChanged(sourceUnitController, item);
        }

        public void NotifyOnAcceptQuest(UnitController sourceUnitController, QuestBase questBase) {
            OnAcceptQuest(sourceUnitController, questBase);
        }

        public void NotifyOnRemoveQuest(UnitController sourceUnitController, QuestBase questBase) {
            OnRemoveQuest(sourceUnitController, questBase);
        }

        public void NotifyOnMarkQuestComplete(UnitController sourceUnitController, QuestBase questBase) {
            OnMarkQuestComplete(sourceUnitController, questBase);
        }

        public void NotifyOnQuestObjectiveStatusUpdated(UnitController sourceUnitController, QuestBase questBase) {
            OnQuestObjectiveStatusUpdated(sourceUnitController, questBase);
        }

        public void NotifyOnLearnSkill(UnitController sourceUnitController, Skill skill) {
            OnLearnSkill(sourceUnitController, skill);
        }

        public void NotifyOnUnLearnSkill(UnitController sourceUnitController, Skill skill) {
            OnUnLearnSkill(sourceUnitController, skill);
        }

        public void NotifyOnSetCraftAbility(UnitController sourceUnitController, CraftAbilityProperties abilityProperties) {
            OnSetCraftAbility(sourceUnitController, abilityProperties);
        }

        public void NotifyOnCraftItem() {
            OnCraftItem();
        }

        public void NotifyOnAddBag() {
            OnAddBag();
        }

        public void NotifyOnCurrencyChange() {
            OnCurrencyChange();
        }

        public void NotifyOnUnsetGamepadActionButton(int buttonIndex) {
            OnUnsetGamepadActionButton(buttonIndex);
        }

        public void NotifyOnUnsetMouseActionButton(int buttonIndex) {
            OnUnsetMouseActionButton(buttonIndex);
        }

        public void NotifyOnSetMouseActionButton(IUseable useable, int buttonIndex) {
            OnSetMouseActionButton(useable, buttonIndex);
        }

        public void NotifyOnSetGamepadActionButton(IUseable useable, int buttonIndex) {
            OnSetGamepadActionButton(useable, buttonIndex);
        }

        public void NotifyOnRemoveActivePet(UnitProfile unitProfile) {
            OnRemoveActivePet(unitProfile);
        }
    }

    [System.Serializable]
    public class CustomParam {
        public EventParam eventParams = new EventParam();
        public ObjectConfigurationNode objectParam = new ObjectConfigurationNode();
    }

    [System.Serializable]
    public class EventParam {
        public string StringParam = string.Empty;
        public int IntParam = 0;
        public float FloatParam = 0f;
        public bool BoolParam = false;
    }

    [System.Serializable]
    public class EventParamProperties {
        public EventParam simpleParams = new EventParam();
        public ObjectConfigurationNode objectParam = new ObjectConfigurationNode();
    }

}
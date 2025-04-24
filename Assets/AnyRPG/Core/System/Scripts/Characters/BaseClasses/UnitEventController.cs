using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AnyRPG {

    public class UnitEventController : ConfiguredClass {

        public event System.Action<Interactable> OnSetTarget = delegate { };
        public event System.Action<Interactable> OnClearTarget = delegate { };
        public event System.Action OnAggroTarget = delegate { };
        public event System.Action OnAttack = delegate { };
        public event System.Action OnTakeDamage = delegate { };
        public event System.Action OnTakeFallDamage = delegate { };
        public event System.Action OnKillTarget = delegate { };
        public event System.Action OnInteract = delegate { };
        public event System.Action OnManualMovement = delegate { };
        public event System.Action OnJump = delegate { };
        public event System.Action<UnitController> OnReputationChange = delegate { };
        public event System.Action<UnitController> OnReviveComplete = delegate { };
        public event System.Action<UnitController> OnBeforeDie = delegate { };
        public event System.Action<CharacterStats> OnAfterDie = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };
        public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        public event System.Action<UnitController, CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<UnitController, ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<Faction, Faction> OnFactionChange = delegate { };
        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action<string> OnAddStatusEffectStack = delegate { };
        public event System.Action<StatusEffectProperties> OnCancelStatusEffect = delegate { };
        public event System.Action<IAbilityCaster, AbilityProperties, float> OnCastTimeChanged = delegate { };
        public event System.Action OnCastComplete = delegate { };
        public event System.Action OnCastCancel = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeActivateMountedState = delegate { };
        public event System.Action OnStartInteract = delegate { };
        public event System.Action OnStopInteract = delegate { };
        public event System.Action<UnitController, InteractableOptionComponent, int, int> OnStartInteractWithOption = delegate { };
        public event System.Action<InteractableOptionComponent> OnStopInteractWithOption = delegate { };
        public event System.Action OnDropCombat = delegate { };
        public event System.Action<UnitController> OnBeginCastOnEnemy = delegate { };
        public event System.Action<float, float, float, float> OnCalculateRunSpeed = delegate { };
        public event System.Action<AbilityEffectContext> OnImmuneToEffect = delegate { };
        public event System.Action<UnitController, int, int> OnGainXP = delegate { };
        public event System.Action<PowerResource, int> OnRecoverResource = delegate { };
        public event System.Action<int, int> OnPrimaryResourceAmountChanged = delegate { };
        public event System.Action OnEnterStealth = delegate { };
        public event System.Action OnLeaveStealth = delegate { };
        public event System.Action OnStatChanged = delegate { };
        public event System.Action OnReviveBegin = delegate { };
        public event System.Action OnCombatUpdate = delegate { };
        public event System.Action<Interactable> OnEnterCombat = delegate { };
        public event System.Action<UnitController, Interactable> OnHitEvent = delegate { };
        public event System.Action<Interactable, AbilityEffectContext> OnReceiveCombatMiss = delegate { };
        public event System.Action<UnitController, UnitController, float> OnKillEvent = delegate { };
        //public event System.Action<InstantiatedEquipment, InstantiatedEquipment, int> OnEquipmentChanged = delegate { };
        public event System.Action<AbilityProperties> OnAbilityActionCheckFail = delegate { };
        public event System.Action<string> OnCombatMessage = delegate { };
        public event System.Action<string, bool> OnBeginAction = delegate { };
        public event System.Action<AbilityProperties, Interactable, bool> OnBeginAbility = delegate { };
        public event System.Action OnBeginAbilityCoolDown = delegate { };
        public event System.Action OnUnlearnAbilities = delegate { };
        public event System.Action<AbilityProperties> OnActivateTargetingMode = delegate { };
        public event System.Action<UnitController, AbilityProperties> OnLearnAbility = delegate { };
        public event System.Action<bool> OnUnlearnAbility = delegate { };
        public event System.Action<AbilityProperties> OnAttemptPerformAbility = delegate { };
        public event System.Action<UnitController, string> OnMessageFeedMessage = delegate { };
        public event System.Action<AbilityProperties> OnLearnedCheckFail = delegate { };
        public event System.Action<AbilityProperties> OnCombatCheckFail = delegate { };
        public event System.Action<AbilityProperties> OnStealthCheckFail = delegate { };
        public event System.Action<AbilityProperties, IAbilityCaster> OnPowerResourceCheckFail = delegate { };
        public event System.Action<AbilityProperties> OnPerformAbility = delegate { };
        public event System.Action<UnitController> OnDespawn = delegate { };
        public event System.Action<string> OnBeginChatMessage = delegate { };
        public event System.Action OnInitializeAnimator = delegate { };
        public event System.Action<string> OnAnimatorSetTrigger = delegate { };
        public event System.Action OnAnimatorReviveComplete = delegate { };
        public event System.Action<bool> OnAnimatorStartCasting = delegate { };
        public event System.Action<bool> OnAnimatorEndCasting = delegate { };
        public event System.Action<bool> OnAnimatorStartActing = delegate { };
        public event System.Action<bool> OnAnimatorEndActing = delegate { };
        public event System.Action<bool> OnAnimatorStartAttacking = delegate { };
        public event System.Action<bool> OnAnimatorEndAttacking = delegate { };
        public event System.Action OnAnimatorStartLevitated = delegate { };
        public event System.Action<bool> OnAnimatorEndLevitated = delegate { };
        public event System.Action OnAnimatorStartStunned = delegate { };
        public event System.Action<bool> OnAnimatorEndStunned = delegate { };
        public event System.Action OnAnimatorStartRevive = delegate { };
        public event System.Action OnAnimatorDeath = delegate { };
        public event System.Action<string, AnimationClip> OnSetAnimationClipOverride = delegate { };
        public event System.Action<AnimatedAction> OnPerformAnimatedActionAnimation = delegate { };
        public event System.Action<AbilityProperties, int> OnPerformAbilityCastAnimation = delegate { };
        public event System.Action<AbilityProperties, int> OnPerformAbilityActionAnimation = delegate { };
        public event System.Action OnAnimatorClearAction = delegate { };
        public event System.Action OnAnimatorClearAbilityAction = delegate { };
        public event System.Action OnAnimatorClearAbilityCast = delegate { };
        public event System.Action<AbilityProperties, int> OnSpawnAbilityObjects = delegate { };
        public event System.Action OnDespawnAbilityObjects = delegate { };
        public event System.Action<Interactable, Interactable, LengthEffectProperties, AbilityEffectContext> OnSpawnAbilityEffectPrefabs = delegate { };
        public event System.Action<Interactable, Interactable, ProjectileEffectProperties, AbilityEffectContext> OnSpawnProjectileEffectPrefabs = delegate { };
        public event System.Action<Interactable, Interactable, ChanneledEffectProperties, AbilityEffectContext> OnSpawnChanneledEffectPrefabs = delegate { };
        public event System.Action<UnitController, Interactable> OnEnterInteractableTrigger = delegate { };
        public event System.Action<UnitController, Interactable> OnExitInteractableTrigger = delegate { };
        public event System.Action<UnitController, Interactable> OnEnterInteractableRange = delegate { };
        public event System.Action<UnitController, Interactable> OnExitInteractableRange = delegate { };
        public event System.Action<UnitController, QuestBase> OnAcceptQuest = delegate { };
        //public event System.Action<UnitController, QuestBase> OnRemoveQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnAbandonQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnTurnInQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnMarkQuestComplete = delegate { };
        public event System.Action<UnitController, QuestBase> OnQuestObjectiveStatusUpdated = delegate { };
        public event System.Action<UnitController, Skill> OnLearnSkill = delegate { };
        public event System.Action<UnitController, Skill> OnUnLearnSkill = delegate { };
        public event System.Action<string, string, string, QuestObjectiveSaveData> OnSetQuestObjectiveCurrentAmount = delegate { };
        public event System.Action<int, bool, int> OnPlaceInStack = delegate { };
        public event System.Action<int, bool, int> OnPlaceInEmpty = delegate { };
        public event System.Action<InstantiatedItem> OnGetNewInstantiatedItem = delegate { };
        public event System.Action<InstantiatedItem> OnRequestDeleteItem = delegate { };
        public event System.Action<InstantiatedItem> OnDeleteItem = delegate { };
        public event System.Action<InstantiatedEquipment, EquipmentSlotProfile> OnRequestEquipToSlot = delegate { };
        //public event System.Action<EquipmentSlotProfile> OnRequestUnequipFromList = delegate { };
        public event System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnAddEquipment = delegate { };
        public event System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnRemoveEquipment = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnAddItemToInventorySlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnAddItemToBankSlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnRemoveItemFromInventorySlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnRemoveItemFromBankSlot = delegate { };
        public event System.Action<InventorySlot, InventorySlot, bool, bool> OnRequestDropItemFromInventorySlot = delegate { };
        public event System.Action<CraftAbilityProperties> OnSetCraftAbility = delegate { };
        public event System.Action OnCraftItem = delegate { };
        public event System.Action OnRemoveFirstCraftingQueueItem = delegate { };
        public event System.Action OnClearCraftingQueue = delegate { };
        public event System.Action<Recipe> OnAddToCraftingQueue = delegate { };
        public event System.Action<int> OnRequestMoveFromBankToInventory = delegate { };
        public event System.Action<int> OnRequestMoveFromInventoryToBank = delegate { };
        public event System.Action<int> OnRequestUseItem = delegate { };
        public event System.Action OnRebuildModelAppearance = delegate { };
        public event System.Action<InstantiatedEquipment, InstantiatedEquipment> OnRequestSwapInventoryEquipment = delegate { };
        public event System.Action<InstantiatedEquipment, int> OnRequestUnequipToSlot = delegate { };
        public event System.Action<InstantiatedBag, InstantiatedBag> OnRequestSwapBags = delegate { };
        public event System.Action<InstantiatedBag, int, bool> OnRequestUnequipBagToSlot = delegate { };
        public event System.Action<InstantiatedBag, bool> OnRequestUnequipBag = delegate { };
        public event System.Action<InstantiatedBag> OnRemoveBag = delegate { };
        public event System.Action<InstantiatedBag, BagNode> OnAddBag = delegate { };
        public event System.Action<InstantiatedBag, int, bool> OnRequestMoveBag = delegate { };
        public event System.Action<InstantiatedBag, int, bool> OnRequestAddBag = delegate { };
        public event System.Action<Vector3> OnSetGroundTarget = delegate { };


        //public event System.Action<BaseAbilityProperties, Interactable> OnTargetInAbilityRangeFail = delegate { };


        // unit controller of controlling unit
        private UnitController unitController;

        public UnitEventController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        #region EventNotifications

        public void NotifyOnDespawn(UnitController despawnController) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDespawn()");

            OnDespawn(despawnController);
        }

        public void NotifyOnPerformAbility(AbilityProperties abilityProperties) {
            OnPerformAbility(abilityProperties);
        }

        public void NotifyOnPowerResourceCheckFail(AbilityProperties abilityProperties, IAbilityCaster abilityCaster) {
            OnPowerResourceCheckFail(abilityProperties, abilityCaster);
        }

        public void NotifyOnStealthCheckFail(AbilityProperties abilityProperties) {
            OnStealthCheckFail(abilityProperties);
        }

        public void NotifyOnCombatCheckFail(AbilityProperties abilityProperties) {
            OnCombatCheckFail(abilityProperties);
        }

        public void NotifyOnLearnedCheckFail(AbilityProperties abilityProperties) {
            OnLearnedCheckFail(abilityProperties);
        }

        public void NotifyOnMessageFeedMessage(string message) {
            OnMessageFeedMessage(unitController, message);
        }

        public void NotifyOnAttemptPerformAbility(AbilityProperties abilityProperties) {
            OnAttemptPerformAbility(abilityProperties);
        }

        public void NotifyOnUnlearnAbility(bool updateActionBars) {
            OnUnlearnAbility(updateActionBars);
        }

        public void NotifyOnLearnAbility(AbilityProperties abilityProperties) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnLearnAbility({abilityProperties.ResourceName})");

            OnLearnAbility(unitController, abilityProperties);
        }

        public void NotifyOnActivateTargetingMode(AbilityProperties abilityProperties) {
            OnActivateTargetingMode(abilityProperties);
        }

        public void NotifyOnUnlearnAbilities() {
            OnUnlearnAbilities();
        }

        public void NotifyOnBeginAction(string actionName, bool playerInitiated) {
            OnBeginAction(actionName, playerInitiated);
        }

        public void NotifyOnBeginAbility(AbilityProperties baseAbility, Interactable target, bool playerInitiated) {
            OnBeginAbility(baseAbility, target, playerInitiated);
        }

        public void NotifyOnBeginAbilityCoolDown() {
            OnBeginAbilityCoolDown();
        }

        public void NotifyOnCombatMessage(string message) {
            OnCombatMessage(message);
        }

        public void NotifyOnAbilityActionCheckFail(AbilityProperties baseAbilityProperties) {
            OnAbilityActionCheckFail(baseAbilityProperties);
        }

        /*
        public void NotifyOnEquipmentChanged(InstantiatedEquipment newEquipment, InstantiatedEquipment oldEquipment, int slotIndex) {
            OnEquipmentChanged(newEquipment, oldEquipment, slotIndex);
        }
        */

        public void NotifyOnKillEvent(UnitController killedCharacter, float creditPercent) {
            OnKillEvent(unitController, killedCharacter, creditPercent);
        }

        public void NotifyOnReceiveCombatMiss(Interactable target, AbilityEffectContext abilityEffectContext) {
            OnReceiveCombatMiss(target, abilityEffectContext);
        }

        public void NotifyOnHitEvent(UnitController source, Interactable target) {
            OnHitEvent(source, target);
        }

        public void NotifyOnEnterCombat(Interactable target) {
            OnEnterCombat(target);
        }

        public void NotifyOnCombatUpdate() {
            OnCombatUpdate();
        }

        public void NotifyOnReviveBegin() {
            OnReviveBegin();
        }

        public void NotifyOnStatChanged() {
            OnStatChanged();
        }

        public void NotifyOnEnterStealth() {
            OnEnterStealth();
        }

        public void NotifyOnLeaveStealth() {
            OnLeaveStealth();
        }

        public void NotifyOnPrimaryResourceAmountChanged(int maxAmount, int currentAmount) {
            OnPrimaryResourceAmountChanged(maxAmount, currentAmount);
        }

        public void NotifyOnRecoverResource(PowerResource powerResource, int amount) {
            OnRecoverResource(powerResource, amount);
        }

        public void NotifyOnGainXP(int gainedXP, int currentXP) {
            OnGainXP(unitController, gainedXP, currentXP);
        }

        public void NotifyOnImmuneToEffect(AbilityEffectContext abilityEffectContext) {
            OnImmuneToEffect(abilityEffectContext);
        }

        public void NotifyOnCalculateRunSpeed(float oldRunSpeed, float currentRunSpeed, float oldSprintSpeed, float currentSprintSpeed) {
            OnCalculateRunSpeed(oldRunSpeed, currentRunSpeed, oldSprintSpeed, currentSprintSpeed);
        }

        public void NotifyOnBeginCastOnEnemy(UnitController unitController) {
            OnBeginCastOnEnemy(unitController);
        }

        public void NotifyOnDropCombat() {
            OnDropCombat();
        }

        public void NotifyOnStartInteract() {
            OnStartInteract();
        }

        public void NotifyOnStopInteract() {
            OnStopInteract();
        }

        public void NotifyOnStartInteractWithOption(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            OnStartInteractWithOption(unitController, interactableOptionComponent, componentIndex, choiceIndex);
        }

        public void NotifyOnStopInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            OnStopInteractWithOption(interactableOptionComponent);
        }

        public void NotifyOnAggroTarget() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAggroTarget()");
            OnAggroTarget();
        }

        public void NotifyOnAttack() {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAttack()");
            OnAttack();
        }

        public void NotifyOnTakeDamage() {
            unitController.UnitAnimator.HandleTakeDamage();
            OnTakeDamage();
        }

        public void NotifyOnTakeFallDamage() {
            OnTakeFallDamage();
        }

        public void NotifyOnKillTarget() {
            OnKillTarget();
        }

        public void NotifyOnManualMovement() {
            OnManualMovement();
        }

        public void NotifyOnSetTarget(Interactable interactable) {
            OnSetTarget(interactable);
        }

        public void NotifyOnClearTarget(Interactable interactable) {
            OnClearTarget(interactable);
        }

        public void NotifyOnUnitDestroy(UnitProfile unitProfile) {
            OnUnitDestroy(unitProfile);
        }

        /*
        public void NotifyOnInteract() {
            OnInteract();
        }
        */

        public void NotifyOnJump() {
            OnJump();
        }

        public void NotifyOnCombatMiss() {
            unitController.UnitComponentController.PlayEffectSound(systemConfigurationManager.WeaponMissAudioClip);
        }

        public void NotifyOnReputationChange() {
            // minimap indicator can change color if reputation changed
            if (unitController.UnitControllerMode == UnitControllerMode.Preview) {
                return;
            }
            unitController.CharacterUnit.CallMiniMapStatusUpdateHandler();
            OnReputationChange(unitController);
            unitController.UnitComponentController.HighlightController.UpdateColors();
        }

        public void NotifyOnBeforeDie(UnitController targetUnitController) {
            unitController.UnitComponentController.StopMovementSound();
            unitController.UnitComponentController.HighlightController.UpdateColors();
            OnBeforeDie(targetUnitController);

        }

        public void NotifyOnAfterDie(CharacterStats characterStats) {
            OnAfterDie(characterStats);
        }

        public void NotifyOnReviveComplete() {
            unitController.FreezeRotation();
            unitController.InitializeNamePlate();
            unitController.CharacterUnit.HandleReviveComplete();
            unitController.UnitComponentController.HighlightController.UpdateColors();
            OnReviveComplete(unitController);
        }

        public void NotifyOnLevelChanged(int newLevel) {
            OnLevelChanged(newLevel);
        }

        public void NotifyOnUnitTypeChange(UnitType newUnitType, UnitType oldUnitType) {
            OnUnitTypeChange(newUnitType, oldUnitType);
        }
        public void NotifyOnRaceChange(CharacterRace newCharacterRace, CharacterRace oldCharacterRace) {
            OnRaceChange(newCharacterRace, oldCharacterRace);
        }
        public void NotifyOnClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(unitController, newCharacterClass, oldCharacterClass);
        }
        public void NotifyOnSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(unitController, newClassSpecialization, oldClassSpecialization);
        }
        public void NotifyOnFactionChange(Faction newFaction, Faction oldFaction) {
            OnFactionChange(newFaction, oldFaction);
        }
        public void NotifyOnNameChange(string newName) {
            OnNameChange(newName);
        }
        public void NotifyOnTitleChange(string newTitle) {
            OnTitleChange(newTitle);
        }
        public void NotifyOnResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            OnResourceAmountChanged(powerResource, maxAmount, currentAmount);
        }
        public void NotifyOnStatusEffectAdd(StatusEffectNode statusEffectNode) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnStatusEffectAdd({statusEffectNode.StatusEffect.DisplayName})");

            OnStatusEffectAdd(statusEffectNode);
        }
        public void NotifyOnCastTimeChanged(IAbilityCaster source, AbilityProperties baseAbility, float castPercent) {
            OnCastTimeChanged(source, baseAbility, castPercent);
        }
        public void NotifyOnCastComplete() {
            OnCastComplete();
        }
        public void NotifyOnCastCancel() {
            OnCastCancel();
        }
        public void NotifyOnActivateMountedState(UnitController mountUnitController) {
            OnActivateMountedState(mountUnitController);
        }
        public void NotifyOnDeActivateMountedState() {
            OnDeActivateMountedState();
        }
        public void NotifyOnBeginChatMessage(string message) {
            //Debug.Log($"{gameObject.name}.NotifyOnMessageFeed(" + message + ")");
            OnBeginChatMessage(message);
        }

        public void NotifyOnInitializeAnimator() {
            OnInitializeAnimator();
        }

        public void NotifyOnAnimatorSetTrigger(string triggerName) {
            OnAnimatorSetTrigger(triggerName);
        }

        public void NotifyOnAnimatorReviveComplete(){
            OnAnimatorReviveComplete();
        }

        public void NotifyOnAnimatorStartCasting(bool swapAnimator){
            OnAnimatorStartCasting(swapAnimator);
        }

        public void NotifyOnAnimatorEndCasting(bool swapAnimator) {
            OnAnimatorEndCasting(swapAnimator);
        }

        public void NotifyOnAnimatorStartActing(bool swapAnimator) {
            OnAnimatorStartActing(swapAnimator);
        }

        public void NotifyOnAnimatorEndActing(bool swapAnimator) {
            OnAnimatorEndActing(swapAnimator);
        }

        public void NotifyOnAnimatorStartAttacking(bool swapAnimator) {
            OnAnimatorStartAttacking(swapAnimator);
        }

        public void NotifyOnAnimatorEndAttacking(bool swapAnimator) {
            OnAnimatorEndAttacking(swapAnimator);
        }

        public void NotifyOnAnimatorStartLevitated() {
            OnAnimatorStartLevitated();
        }

        public void NotifyOnAnimatorEndLevitated(bool swapAnimator) {
            OnAnimatorEndLevitated(swapAnimator);
        }

        public void NotifyOnAnimatorStartStunned() {
            OnAnimatorStartStunned();
        }

        public void NotifyOnAnimatorEndStunned(bool swapAnimator) {
            OnAnimatorEndStunned(swapAnimator);
        }

        public void NotifyOnAnimatorStartRevive() {
            OnAnimatorStartRevive();
        }

        public void NotifyOnAnimatorDeath() {
            OnAnimatorDeath();
        }

        public void NotifyOnSetAnimationClipOverride(string originalClipName, AnimationClip newAnimationClip) {
            OnSetAnimationClipOverride(originalClipName, newAnimationClip);
        }

        public void NotifyOnPerformAnimatedActionAnimation(AnimatedAction animatedAction) {
            OnPerformAnimatedActionAnimation(animatedAction);
        }

        public void NotifyOnPerformAbilityCastAnimation(AbilityProperties abilityProperties, int clipIndex) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnPerformAbilityCastAnimation()");

            OnPerformAbilityCastAnimation(abilityProperties, clipIndex);
        }

        public void NotifyOnPerformAbilityActionAnimation(AbilityProperties abilityProperties, int clipIndex) {
            OnPerformAbilityActionAnimation(abilityProperties, clipIndex);
        }

        public void NotifyOnAnimatorClearAction() {
            OnAnimatorClearAction();
        }

        public void NotifyOnAnimatorClearAbilityAction() {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnClearAbilityAction()");

            OnAnimatorClearAbilityAction();
        }

        public void NotifyOnAnimatorClearAbilityCast() {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnClearAbilityCast()");

            OnAnimatorClearAbilityCast();
        }

        public void NotifyOnSpawnAbilityObjects(AbilityProperties abilityProperties, int index) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnSpawnAbilityObjects({abilityProperties.ResourceName}, {index})");

            OnSpawnAbilityObjects(abilityProperties, index);
        }

        public void NotifyOnDespawnAbilityObjects() {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnDespawnAbilityObjects()");

            OnDespawnAbilityObjects();
        }

        public void NotifyOnSpawnAbilityEffectPrefabs(Interactable target, Interactable originalTarget, LengthEffectProperties lengthEffectProperties, AbilityEffectContext abilityEffectInput) {
            OnSpawnAbilityEffectPrefabs(target, originalTarget, lengthEffectProperties, abilityEffectInput);
        }

        public void NotifyOnEnterInteractableTrigger(Interactable interactable) {
            Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnEnterInteractableTrigger({interactable.gameObject.name})");

            OnEnterInteractableTrigger(unitController, interactable);
        }

        public void NotifyOnExitInteractableTrigger(Interactable interactable) {
            Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnExitInteractableTrigger({interactable.gameObject.name})");

            OnExitInteractableTrigger(unitController, interactable);
        }

        public void NotifyOnEnterInteractableRange(Interactable interactable) {
            OnEnterInteractableRange(unitController, interactable);
        }

        public void NotifyOnExitInteractableRange(Interactable interactable) {
            OnExitInteractableRange(unitController, interactable);
        }

        public void NotifyOnAcceptQuest(QuestBase questBase) {
            //Debug.Log($"{unitController.gameObject.name}.UniteventController.NotifyOnAcceptQuest({questBase.ResourceName})");

            OnAcceptQuest(unitController, questBase);
        }

        /*
        public void NotifyOnRemoveQuest(QuestBase questBase) {
            OnRemoveQuest(unitController, questBase);
        }
        */

        public void NotifyOnMarkQuestComplete(QuestBase questBase) {
            OnMarkQuestComplete(unitController, questBase);
        }

        public void NotifyOnQuestObjectiveStatusUpdated(QuestBase questBase) {
            OnQuestObjectiveStatusUpdated(unitController, questBase);
        }

        public void NotifyOnLearnSkill(Skill newSkill) {
            OnLearnSkill(unitController, newSkill);
        }

        public void NotifyOnUnLearnSkill(Skill oldSkill) {
            OnUnLearnSkill(unitController, oldSkill);
        }

        public void NotifyOnSetQuestObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, QuestObjectiveSaveData saveData) {
            OnSetQuestObjectiveCurrentAmount(questName, objectiveType, objectiveName, saveData);
        }

        public void NotifyOnAbandonQuest(QuestBase oldQuest) {
            OnAbandonQuest(unitController, oldQuest);
        }

        public void NotifyOnTurnInQuest(QuestBase oldQuest) {
            OnTurnInQuest(unitController, oldQuest);
        }

        public void NotifyOnPlaceInStack(InstantiatedItem instantiatedItem, bool addToBank, int slotIndex) {
            OnPlaceInStack(instantiatedItem.InstanceId, addToBank, slotIndex);
        }

        public void NotifyOnPlaceInEmpty(InstantiatedItem instantiatedItem, bool addToBank, int slotIndex) {
            OnPlaceInEmpty(instantiatedItem.InstanceId, addToBank, slotIndex);
        }

        public void NotifyOnGetNewInstantiatedItem(InstantiatedItem instantiatedItem) {
            OnGetNewInstantiatedItem(instantiatedItem);
        }

        public void NotifyOnRequestDeleteItem(InstantiatedItem instantiatedItem) {
            OnRequestDeleteItem(instantiatedItem);
        }

        public void NotifyOnDeleteItem(InstantiatedItem instantiatedItem) {
            OnDeleteItem(instantiatedItem);
        }

        public void NotifyOnRequestEquipToSlot(InstantiatedEquipment newEquipment, EquipmentSlotProfile equipmentSlotProfile) {
            OnRequestEquipToSlot(newEquipment, equipmentSlotProfile);
        }

        /*
        public void NotifyOnRequestUnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            OnRequestUnequipFromList(equipmentSlotProfile);
        }
        */

        public void NotifyOnRemoveEquipment(EquipmentSlotProfile equipmentSlotProfile, InstantiatedEquipment instantiatedEquipment) {
            OnRemoveEquipment(equipmentSlotProfile, instantiatedEquipment);
        }

        public void NotifyOnAddEquipment(EquipmentSlotProfile equipmentSlotProfile, InstantiatedEquipment instantiatedEquipment) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnAddEquipment({equipmentSlotProfile.ResourceName}, {instantiatedEquipment.Item.ResourceName})");

            OnAddEquipment(equipmentSlotProfile, instantiatedEquipment);
        }

        public void NotifyOnAddItemToInventorySlot(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"UnitEventController.NotifyOnAddItemToInventorySlot({item.Item.ResourceName})");

            OnAddItemToInventorySlot(slot, item);
        }

        public void NotifyOnRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem item) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRemoveItemFromInventorySlot({item.Item.ResourceName})");

            OnRemoveItemFromInventorySlot(slot, item);
        }

        public void NotifyOnAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            OnAddItemToBankSlot(slot, item);
        }

        public void NotifyOnRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            OnRemoveItemFromBankSlot(slot, item);
        }

        public void NotifyOnRequestDropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot, bool fromSlotIsInventory, bool toSlotIsInventory) {
            OnRequestDropItemFromInventorySlot(fromSlot, toSlot, fromSlotIsInventory, toSlotIsInventory);
        }

        public void NotifyOnSetCraftAbility(CraftAbilityProperties craftAbility) {
            OnSetCraftAbility(craftAbility);
        }

        public void NotifyOnCraftItem() {
            OnCraftItem();
        }

        public void NotifyOnRemoveFirstCraftingQueueItem() {
            OnRemoveFirstCraftingQueueItem();
        }

        public void NotifyOnClearCraftingQueue() {
            OnClearCraftingQueue();
        }

        public void NotifyOnAddToCraftingQueue(Recipe recipe) {
            OnAddToCraftingQueue(recipe);
        }

        public void NotifyOnRequestMoveFromBankToInventory(int slotIndex) {
            OnRequestMoveFromBankToInventory(slotIndex);
        }

        public void NotifyOnRequestMoveFromInventoryToBank(int slotIndex) {
            OnRequestMoveFromInventoryToBank(slotIndex);
        }

        public void NotifyOnRequestUseItem(int slotIndex) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestUseItem({slotIndex})");

            OnRequestUseItem(slotIndex);
        }

        public void NotifyOnRebuildModelAppearance() {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRebuildModelAppearance()");

            OnRebuildModelAppearance();
        }

        public void NotifyOnRequestSwapInventoryEquipment(InstantiatedEquipment oldEquipment, InstantiatedEquipment newEquipment) {
            OnRequestSwapInventoryEquipment(oldEquipment, newEquipment);
        }

        public void NotifyOnRequestUnequipToSlot(InstantiatedEquipment instantiatedEquipment, int inventorySlotId) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestUnequipToSlot({instantiatedEquipment.Item.ResourceName}, {inventorySlotId})");
            OnRequestUnequipToSlot(instantiatedEquipment, inventorySlotId);
        }

        public void NotifyOnRequestSwapBags(InstantiatedBag oldInstantiatedBag, InstantiatedBag newInstantiatedBag) {
            OnRequestSwapBags(oldInstantiatedBag, newInstantiatedBag);
        }

        public void NotifyOnRequestUnequipBagToSlot(InstantiatedBag instantiatedBag, int slotIndex, bool isBankSlot) {
            OnRequestUnequipBagToSlot(instantiatedBag, slotIndex, isBankSlot);
        }

        public void NotifyOnRemoveBag(InstantiatedBag instantiatedBag) {
            OnRemoveBag(instantiatedBag);
        }

        public void NotifyOnAddBag(InstantiatedBag instantiatedBag, BagNode bagNode) {
            OnAddBag(instantiatedBag, bagNode);
        }

        public void NotifyOnRequestMoveBag(InstantiatedBag bag, int nodeIndex, bool isBankNode) {
            OnRequestMoveBag(bag, nodeIndex, isBankNode);
        }

        public void NotifyOnRequestAddBagFromInventory(InstantiatedBag instantiatedBag, int nodeIndex, bool isBankNode) {
            Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnRequestAddBagFromInventory({instantiatedBag.Item.ResourceName}, {nodeIndex}, {isBankNode})");
            OnRequestAddBag(instantiatedBag, nodeIndex, isBankNode);
        }

        public void NotifyOnRequestUnequipBag(InstantiatedBag instantiatedBag, bool isBank) {
            OnRequestUnequipBag(instantiatedBag, isBank);
        }

        public void NotifyOnAddStatusEffectStack(string resourceName) {
            OnAddStatusEffectStack(resourceName);
        }

        public void NotifyOnCancelStatusEffect(StatusEffectProperties statusEffect) {
            OnCancelStatusEffect(statusEffect);
        }

        public void NotifyOnSpawnProjectileEffectPrefabs(Interactable target, Interactable originalTarget, ProjectileEffectProperties projectileEffectProperties, AbilityEffectContext abilityEffectContext) {
            OnSpawnProjectileEffectPrefabs(target, originalTarget, projectileEffectProperties, abilityEffectContext);
        }

        public void NotifyOnSpawnChanneledEffectPrefabs(Interactable target, Interactable originalTarget, ChanneledEffectProperties channeledEffectProperties, AbilityEffectContext abilityEffectContext) {
            OnSpawnChanneledEffectPrefabs(target, originalTarget, channeledEffectProperties, abilityEffectContext);
        }

        public void NotifyOnSetGroundTarget(Vector3 newGroundTarget) {
            OnSetGroundTarget(newGroundTarget);
        }

        #endregion


    }

}
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
        public event System.Action OnReputationChange = delegate { };
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<UnitController> OnBeforeDie = delegate { };
        public event System.Action<CharacterStats> OnAfterDie = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<UnitType, UnitType> OnUnitTypeChange = delegate { };
        public event System.Action<CharacterRace, CharacterRace> OnRaceChange = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<Faction, Faction> OnFactionChange = delegate { };
        public event System.Action<string> OnNameChange = delegate { };
        public event System.Action<string> OnTitleChange = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action<IAbilityCaster, AbilityProperties, float> OnCastTimeChanged = delegate { };
        public event System.Action OnCastComplete = delegate { };
        public event System.Action OnCastCancel = delegate { };
        public event System.Action<UnitProfile> OnUnitDestroy = delegate { };
        public event System.Action<UnitController> OnActivateMountedState = delegate { };
        public event System.Action OnDeActivateMountedState = delegate { };
        public event System.Action<string> OnMessageFeed = delegate { };
        public event System.Action OnStartInteract = delegate { };
        public event System.Action OnStopInteract = delegate { };
        public event System.Action<InteractableOptionComponent> OnStartInteractWithOption = delegate { };
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
        public event System.Action<Equipment, Equipment, int> OnEquipmentChanged = delegate { };
        public event System.Action<AbilityProperties> OnAbilityActionCheckFail = delegate { };
        public event System.Action<string> OnCombatMessage = delegate { };
        public event System.Action<string, bool> OnBeginAction = delegate { };
        public event System.Action<AbilityProperties, Interactable, bool> OnBeginAbility = delegate { };
        public event System.Action OnBeginAbilityCoolDown = delegate { };
        public event System.Action OnUnlearnAbilities = delegate { };
        public event System.Action<AbilityProperties> OnActivateTargetingMode = delegate { };
        public event System.Action<AbilityProperties> OnLearnAbility = delegate { };
        public event System.Action<bool> OnUnlearnAbility = delegate { };
        public event System.Action<AbilityProperties> OnAttemptPerformAbility = delegate { };
        public event System.Action<string> OnMessageFeedMessage = delegate { };
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
        public event System.Action<UnitController, Interactable> OnEnterInteractableTrigger = delegate { };


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
            OnMessageFeedMessage(message);
        }

        public void NotifyOnAttemptPerformAbility(AbilityProperties abilityProperties) {
            OnAttemptPerformAbility(abilityProperties);
        }

        public void NotifyOnUnlearnAbility(bool updateActionBars) {
            OnUnlearnAbility(updateActionBars);
        }

        public void NotifyOnLearnAbility(AbilityProperties abilityProperties) {
            //Debug.Log($"{unitController.gameObject.name}.UnitEventController.NotifyOnLearnAbility({abilityProperties.ResourceName})");

            OnLearnAbility(abilityProperties);
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

        public void NotifyOnEquipmentChanged(Equipment newEquipment, Equipment oldEquipment, int slotIndex) {
            OnEquipmentChanged(newEquipment, oldEquipment, slotIndex);
        }

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

        public void NotifyOnStartInteractWithOption(InteractableOptionComponent interactableOptionComponent) {
            OnStartInteractWithOption(interactableOptionComponent);
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
            OnReputationChange();
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
            OnReviveComplete();
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
            OnClassChange(newCharacterClass, oldCharacterClass);
        }
        public void NotifyOnSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(newClassSpecialization, oldClassSpecialization);
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
            //Debug.Log($"{gameObject.name}.NotifyOnStatusEffectAdd()");
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
        public void NotifyOnMessageFeed(string message) {
            //Debug.Log($"{gameObject.name}.NotifyOnMessageFeed(" + message + ")");
            OnMessageFeed(message);
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
            OnAnimatorClearAbilityCast();
        }

        public void NotifyOnSpawnAbilityObjects(AbilityProperties abilityProperties, int index) {
            OnSpawnAbilityObjects(abilityProperties, index);
        }

        public void NotifyOnDespawnAbilityObjects() {
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

            OnEnterInteractableTrigger(unitController, interactable);
        }

        #endregion


    }

}
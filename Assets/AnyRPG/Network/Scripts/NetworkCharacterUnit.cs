using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace AnyRPG {
    public class NetworkCharacterUnit : NetworkInteractable {

        public event System.Action OnCompleteCharacterRequest = delegate { };

        public readonly SyncVar<string> unitProfileName = new SyncVar<string>();

        public readonly SyncVar<int> unitLevel = new SyncVar<int>();

        public readonly SyncVar<UnitControllerMode> unitControllerMode = new SyncVar<UnitControllerMode>();

        public readonly SyncVar<CharacterAppearanceData> characterAppearanceData = new SyncVar<CharacterAppearanceData>();

        public readonly SyncVar<string> characterName = new SyncVar<string>(new SyncTypeSettings(ReadPermission.ExcludeOwner));

        
        private UnitProfile unitProfile = null;
        private UnitController unitController = null;

        public UnitController UnitController { get => unitController; }

        protected override void Awake() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.Awake() position: { gameObject.transform.position}");
            base.Awake();

            /*
            NetworkTransform networkTransform = GetComponent<NetworkTransform>();
            if (networkTransform != null) {
                networkTransform.OnDataReceived += HandleDataReceived;
            }
            */
            characterName.OnChange += HandleNameSync;
            unitControllerMode.Value = UnitControllerMode.Preview;
        }

        private void HandleDataReceived(NetworkTransform.TransformData prev, NetworkTransform.TransformData next) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleDataReceived({prev.Position}, {next.Position})");
        }

        protected override void Configure() {
            base.Configure();
            unitController = GetComponent<UnitController>();
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartClient()");

            Configure();
            if (systemGameManager == null) {
                return;
            }
            CompleteCharacterRequest(base.IsOwner);
            SubscribeToClientUnitEvents();
        }

        public override void OnStopClient() {
            base.OnStopClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStopClient()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientUnitEvents();
            systemGameManager.NetworkManagerClient.ProcessStopNetworkUnitClient(unitController);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartServer()");

            Configure();
            if (systemGameManager == null) {
                return;
            }
            CompleteCharacterRequest(false);
            SubscribeToServerUnitEvents();
        }

        public override void OnStopServer() {
            base.OnStopServer();
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStopServer()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            UnsubscribeFromServerUnitEvents();
            systemGameManager.NetworkManagerServer.ProcessStopNetworkUnitServer(unitController);
        }

        public void SubscribeToClientUnitEvents() {
            if (unitController == null) {
                // something went wrong
                return;
            }

            if (base.IsOwner) {
                unitController.UnitEventController.OnBeginAction += HandleBeginAction;
                unitController.UnitEventController.OnBeginAbility += HandleBeginAbilityLocal;
                unitController.UnitEventController.OnSetTarget += HandleSetTargetClient;
                unitController.UnitEventController.OnClearTarget += HandleClearTargetClient;
            }
            //unitController.UnitEventController.OnDespawn += HandleDespawnClient;
        }

        public void UnsubscribeFromClientUnitEvents() {
            if (unitController == null) {
                return;
            }
            if (base.IsOwner) {
                unitController.UnitEventController.OnBeginAction -= HandleBeginAction;
                unitController.UnitEventController.OnBeginAbility -= HandleBeginAbilityLocal;
                unitController.UnitEventController.OnSetTarget -= HandleSetTargetClient;
                unitController.UnitEventController.OnClearTarget -= HandleClearTargetClient;
            }
            //unitController.UnitEventController.OnDespawn -= HandleDespawnClient;
        }

        public void SubscribeToServerUnitEvents() {
            if (unitController == null) {
                // something went wrong
                return;
            }

            unitController.UnitEventController.OnBeginChatMessage += HandleBeginChatMessageServer;
            unitController.UnitEventController.OnPerformAnimatedActionAnimation += HandlePerformAnimatedActionServer;
            unitController.UnitEventController.OnPerformAbilityCastAnimation += HandlePerformAbilityCastAnimationServer;
            unitController.UnitEventController.OnPerformAbilityActionAnimation += HandlePerformAbilityActionAnimationServer;
            unitController.UnitEventController.OnAnimatorClearAction += HandleClearActionClient;
            unitController.UnitEventController.OnAnimatorClearAbilityAction += HandleClearAnimatedAbilityClient;
            unitController.UnitEventController.OnAnimatorClearAbilityCast += HandleClearCastingClient;
            //unitController.UnitEventController.OnAnimatorDeath += HandleAnimatorDeathClient;
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChangedServer;
            unitController.UnitEventController.OnBeforeDie += HandleBeforeDieServer;
            unitController.UnitEventController.OnEnterCombat += HandleEnterCombatServer;
            unitController.UnitEventController.OnDropCombat += HandleDropCombat;
            unitController.UnitEventController.OnSpawnAbilityObjects += HandleSpawnAbilityObjectsServer;
            unitController.UnitEventController.OnDespawnAbilityObjects += HandleDespawnAbilityObjects;
            unitController.UnitEventController.OnSpawnAbilityEffectPrefabs += HandleSpawnAbilityEffectPrefabsServer;
            unitController.UnitEventController.OnGainXP += HandleGainXPServer;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnDespawn += HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTriggerServer;
        }

        public void UnsubscribeFromServerUnitEvents() {
            if (unitController == null) {
                return;
            }
            unitController.UnitEventController.OnBeginChatMessage -= HandleBeginChatMessageServer;
            unitController.UnitEventController.OnPerformAnimatedActionAnimation -= HandlePerformAnimatedActionServer;
            unitController.UnitEventController.OnPerformAbilityCastAnimation -= HandlePerformAbilityCastAnimationServer;
            unitController.UnitEventController.OnPerformAbilityActionAnimation -= HandlePerformAbilityActionAnimationServer;
            unitController.UnitEventController.OnAnimatorClearAction -= HandleClearActionClient;
            unitController.UnitEventController.OnAnimatorClearAbilityAction -= HandleClearAnimatedAbilityClient;
            unitController.UnitEventController.OnAnimatorClearAbilityCast -= HandleClearCastingClient;
            //unitController.UnitEventController.OnAnimatorDeath -= HandleAnimatorDeathClient;
            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChangedServer;
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDieServer;
            unitController.UnitEventController.OnEnterCombat -= HandleEnterCombatServer;
            unitController.UnitEventController.OnDropCombat -= HandleDropCombat;
            unitController.UnitEventController.OnSpawnAbilityObjects -= HandleSpawnAbilityObjectsServer;
            unitController.UnitEventController.OnDespawnAbilityObjects -= HandleDespawnAbilityObjects;
            unitController.UnitEventController.OnSpawnAbilityEffectPrefabs -= HandleSpawnAbilityEffectPrefabsServer;
            unitController.UnitEventController.OnGainXP -= HandleGainXPServer;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnDespawn -= HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTriggerServer;

        }

        /*
        private void HandleEnterInteractableTriggerServer(Interactable triggerInteractable) {
            NetworkInteractable networkTarget = null;
            if (triggerInteractable != null) {
                networkTarget = triggerInteractable.GetComponent<NetworkInteractable>();
            }

        }
        */

        [ObserversRpc]
        public void HandleEnterInteractableTriggerClient(NetworkInteractable networkInteractable) {
            Interactable triggerInteractable = null;
            if (networkInteractable != null) {
                triggerInteractable = networkInteractable.Interactable;
            }
            unitController.UnitEventController.NotifyOnEnterInteractableTrigger(triggerInteractable);
        }

        /*
        public void HandleDespawnClient(UnitController controller) {
        }
        */


        public void HandleDespawn(UnitController controller) {
            HandleDespawnClient();
        }

        [ObserversRpc]
        public void HandleDespawnClient() {
            unitController.Despawn(0, false, true);
        }


        [ObserversRpc]
        public void HandleLevelChanged(int newLevel) {
            unitController.CharacterStats.SetLevel(newLevel);
        }

        public void HandleGainXPServer(UnitController controller, int gainedXP, int currentXP) {
            HandleGainXP(gainedXP, currentXP);
        }

        [ObserversRpc]
        private void HandleGainXP(int gainedXP, int currentXP) {
            unitController.CharacterStats.SetXP(currentXP);
            unitController.UnitEventController.NotifyOnGainXP(gainedXP, currentXP);
        }

        public void HandleSpawnAbilityEffectPrefabsServer(Interactable target, Interactable originalTarget, LengthEffectProperties lengthEffectProperties, AbilityEffectContext abilityEffectInput) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityEffectPrefabsServer()");

            NetworkInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<NetworkInteractable>();
            }
            NetworkInteractable networkOriginalTarget = null;
            if (target != null) {
                networkOriginalTarget = originalTarget.GetComponent<NetworkInteractable>();
            }
            //HandleSpawnAbilityEffectPrefabsClient(networkTarget, networkOriginalTarget, lengthEffectProperties.ResourceName, abilityEffectInput);
            HandleSpawnAbilityEffectPrefabsClient(networkTarget, networkOriginalTarget, lengthEffectProperties.ResourceName);
        }

        [ObserversRpc]
        //public void HandleSpawnAbilityEffectPrefabsClient(NetworkInteractable networkTarget, NetworkInteractable networkOriginalTarget, string abilityEffectName, AbilityEffectContext abilityEffectContext) {
        public void HandleSpawnAbilityEffectPrefabsClient(NetworkInteractable networkTarget, NetworkInteractable networkOriginalTarget, string abilityEffectName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityObjectsClient()");

            AbilityEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
            LengthEffectProperties lengthEffectProperties = abilityEffect.AbilityEffectProperties as LengthEffectProperties;
            if (abilityEffect == null || lengthEffectProperties == null) {
                return;
            }
            Interactable target = null;
            Interactable originalTarget = null;
            if (networkTarget != null) {
                target = networkTarget.Interactable;
            }
            if (networkOriginalTarget != null) {
                originalTarget = networkOriginalTarget.Interactable;
            }
            unitController.CharacterAbilityManager.SpawnAbilityEffectPrefabs(target, originalTarget, lengthEffectProperties, new AbilityEffectContext(unitController));
        }

        public void HandleSpawnAbilityObjectsServer(AbilityProperties ability, int index) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityObjectsServer({ability.ResourceName}, {index})");

            HandleSpawnAbilityObjectsClient(ability.ResourceName, index);
        }

        [ObserversRpc]
        public void HandleSpawnAbilityObjectsClient(string abilityName, int index) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityObjectsClient()");

            Ability ability = systemGameManager.SystemDataFactory.GetResource<Ability>(abilityName);
            if (ability != null) {
                unitController.CharacterAbilityManager.SpawnAbilityObjectsInternal(ability.abilityProperties, index);
            }
        }

        [ObserversRpc]
        public void HandleDespawnAbilityObjects() {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityObjects()");

            unitController.CharacterAbilityManager.DespawnAbilityObjects();
        }

        [ObserversRpc]
        public void HandleDropCombat() {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleEnterCombatClient()");

            unitController.CharacterCombat.TryToDropCombat();
        }

        private void HandleEnterCombatServer(Interactable targetInteractable) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleEnterCombatServer(" + (targetInteractable == null ? "null" : targetInteractable.gameObject.name) + ")");

            NetworkInteractable networkInteractable = null;
            if (targetInteractable != null) {
                networkInteractable = targetInteractable.GetComponent<NetworkInteractable>();
            }
            HandleEnterCombatClient(networkInteractable);
        }

        [ObserversRpc]
        public void HandleEnterCombatClient(NetworkInteractable networkInteractable) {
            Debug.Log($"{gameObject.name}.HandleEnterCombatClient()");
            
            if (networkInteractable != null) {
                unitController.CharacterCombat.EnterCombat(networkInteractable.Interactable);
            }
        }


        private void HandleBeforeDieServer(UnitController targetUnitController) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleBeforeDieServer(" + (targetUnitController == null ? "null" : targetUnitController.gameObject.name) + ")");

            HandleBeforeDieClient();
        }

        [ObserversRpc]
        public void HandleBeforeDieClient() {
            Debug.Log($"{gameObject.name}.HandleBeforeDieClient()");

            unitController.CharacterStats.Die();
        }

        private void HandleClearTargetClient(Interactable oldTarget) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleClearTargetClient(" + (oldTarget == null ? "null" : oldTarget.gameObject.name) + ")");

            /*
            NetworkInteractable networkInteractable = null;
            if (oldTarget != null) {
                networkInteractable = oldTarget.GetComponent<NetworkInteractable>();
            }
            HandleClearTargetServer(networkInteractable);
            */
            HandleClearTargetServer();
        }

        [ServerRpc]
        private void HandleClearTargetServer(/*NetworkInteractable networkInteractable*/) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleClearTargetServer(" + (networkInteractable == null ? "null" : networkInteractable.gameObject.name) + ")");
            
            //unitController.SetTarget((networkInteractable == null ? null : networkInteractable.interactable));
            
            unitController.ClearTarget();
        }


        private void HandleSetTargetClient(Interactable target) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSetTargetClient(" + (target == null ? "null" : target.gameObject.name) + ")");

            NetworkInteractable networkInteractable = null;
            if (target != null) {
                networkInteractable = target.GetComponent<NetworkInteractable>();
            }
            HandleSetTargetServer(networkInteractable);
        }

        [ServerRpc]
        private void HandleSetTargetServer(NetworkInteractable networkInteractable) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSetTargetServer(" + (networkInteractable == null ? "null" : networkInteractable.gameObject.name) + ")");

            unitController.SetTarget((networkInteractable == null ? null : networkInteractable.Interactable));
        }

        private void HandleUnitNameChange(string characterName) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleUnitNameChange({characterName})");

            HandleUnitNameChangeServer(characterName);
        }

        [ServerRpc]
        private void HandleUnitNameChangeServer(string characterName) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleUnitNameChangeServer({characterName})");

            this.characterName.Value = characterName;
        }

        private void HandleNameSync(string oldValue, string newValue, bool asServer) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleNameSync({oldValue}, {newValue}, {asServer})");

            unitController.BaseCharacter.ChangeCharacterName(newValue);
        }

        private void CompleteCharacterRequest(bool isOwner) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner})");

            if (base.Owner != null ) {
                Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) owner clientId: {base.Owner.ClientId} {base.OwnerId}");
            }

            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName.Value);
            CharacterConfigurationRequest characterConfigurationRequest;
            if (isOwner && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId.Value)) {
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId.Value, isOwner);
            } else if (base.OwnerId == -1 && networkManagerServer.ServerModeActive == true && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId.Value) == true) {
                Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) owner is -1");
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId.Value, isOwner);
            } else {
                Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) falling back to creating new config request");
                characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
                characterConfigurationRequest.characterName = characterName.Value;
                characterConfigurationRequest.unitLevel = unitLevel.Value;
                characterConfigurationRequest.unitControllerMode = unitControllerMode.Value;
                characterConfigurationRequest.characterAppearanceData = characterAppearanceData.Value;
                CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
                characterRequestData.spawnRequestId = clientSpawnRequestId.Value;
                characterRequestData.isServer = base.IsServerOnlyStarted;
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, characterRequestData, isOwner);
            }

            if (base.IsOwner && unitController != null) {
                unitController.UnitEventController.OnNameChange += HandleUnitNameChange;

                // OnNameChange is not called during initialization, so we have to pass the proper name to the network manually
                HandleUnitNameChange(unitController.BaseCharacter.CharacterName);
            }
            OnCompleteCharacterRequest();
        }

        public void HandleResourceAmountChangedServer(PowerResource powerResource, int oldValue, int newValue) {
            HandleResourceAmountChangedClient(powerResource.resourceName, oldValue, newValue);
        }

        [ObserversRpc]
        public void HandleResourceAmountChangedClient(string powerResourceName, int oldValue, int newValue) {
            unitController.CharacterStats.SetResourceAmount(powerResourceName, newValue);
        }

        public void HandleBeginChatMessageServer(string messageText) {
            HandleBeginChatMessageClient(messageText);
        }

        [ObserversRpc]
        public void HandleBeginChatMessageClient(string messageText) {
            unitController.BeginChatMessage(messageText);
        }

        public void HandlePerformAbilityCastAnimationServer(AbilityProperties baseAbility, int clipIndex) {
            HandlePerformAbilityCastAnimationClient(baseAbility.ResourceName, clipIndex);
        }

        [ObserversRpc]
        public void HandlePerformAbilityCastAnimationClient(string abilityName, int clipIndex) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandlePerformAbilityCastAnimationClient({abilityName}, {clipIndex})");
            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility == null) {
                return;
            }
            unitController.UnitAnimator.PerformAbilityCast(baseAbility.AbilityProperties, clipIndex);
        }

        public void HandlePerformAbilityActionAnimationServer(AbilityProperties baseAbility, int clipIndex) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandlePerformAbilityActionAnimationServer({baseAbility.ResourceName}, {clipIndex})");

            HandlePerformAbilityActionAnimationClient(baseAbility.ResourceName, clipIndex);
        }

        [ObserversRpc]
        public void HandlePerformAbilityActionAnimationClient(string abilityName, int clipIndex) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandlePerformAbilityActionAnimationClient({abilityName})");

            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility == null) {
                return;
            }
            unitController.UnitAnimator.PerformAbilityAction(baseAbility.AbilityProperties, clipIndex);
        }


        public void HandlePerformAnimatedActionServer(AnimatedAction animatedAction) {
            HandlePerformAnimatedActionClient(animatedAction.ResourceName);
        }

        [ObserversRpc]
        public void HandlePerformAnimatedActionClient(string actionName) {
            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction == null) {
                return;
            }
            unitController.UnitAnimator.PerformAnimatedAction(animatedAction);
        }

        [ObserversRpc]
        public void HandleClearActionClient() {
            unitController.UnitAnimator.ClearAction();
        }

        [ObserversRpc]
        public void HandleClearAnimatedAbilityClient() {
            unitController.UnitAnimator.ClearAnimatedAbility();
        }

        [ObserversRpc]
        public void HandleClearCastingClient() {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleClearCastingClient()");

            unitController.UnitAnimator.ClearCasting();
        }

        /*
        [ObserversRpc]
        public void HandleAnimatorDeathClient() {
            unitController.UnitAnimator.HandleDie();
        }
        */

        public void HandleBeginAbilityLocal(AbilityProperties abilityProperties, Interactable target, bool playerInitiated) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleBeginAbilityLocal({abilityProperties.ResourceName})");


            NetworkInteractable targetNetworkInteractable = null;
            if (target != null) {
                targetNetworkInteractable = target.GetComponent<NetworkInteractable>();
            }
            HandleBeginAbilityServer(abilityProperties.ResourceName, targetNetworkInteractable, playerInitiated);
        }

        [ServerRpc]
        public void HandleBeginAbilityServer(string abilityName, NetworkInteractable targetNetworkInteractable, bool playerInitiated) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleBeginAbilityServer({abilityName})");

            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility == null) {
                return;
            }
            Interactable targetInteractable = null;
            if (targetNetworkInteractable != null) {
                targetInteractable = targetNetworkInteractable.GetComponent<Interactable>();
            }
            unitController.CharacterAbilityManager.BeginAbilityInternal(baseAbility.AbilityProperties, targetInteractable, playerInitiated);
        }

        [ServerRpc]
        public void HandleBeginAction(string actionName, bool playerInitiated) {
            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction == null) {
                return;
            }
            unitController.UnitActionManager.BeginActionInternal(animatedAction, playerInitiated);
        }

       

        protected override void OnDisable() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnDisable()");
        }

    }
}


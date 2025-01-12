using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class NetworkCharacterUnit : SpawnedNetworkObject {

        public event System.Action OnCompleteCharacterRequest = delegate { };

        public readonly SyncVar<string> unitProfileName = new SyncVar<string>();

        public readonly SyncVar<int> unitLevel = new SyncVar<int>();

        public readonly SyncVar<UnitControllerMode> unitControllerMode = new SyncVar<UnitControllerMode>();

        public readonly SyncVar<CharacterAppearanceData> characterAppearanceData = new SyncVar<CharacterAppearanceData>();

        public readonly SyncVar<string> characterName = new SyncVar<string>(new SyncTypeSettings(ReadPermission.ExcludeOwner));

        
        private UnitProfile unitProfile = null;
        private UnitController unitController = null;

        // game manager references
        SystemGameManager systemGameManager = null;
        SystemDataFactory systemDataFactory = null;

        private void Awake() {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.Awake() position: { gameObject.transform.position}");

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

        private void Configure() {
            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            systemDataFactory = systemGameManager.SystemDataFactory;
            unitController = GetComponent<UnitController>();
            //if (base.IsOwner && unitController != null) {
            //    unitController.UnitEventController.OnNameChange += HandleUnitNameChange;
            //}
        }

       

        private void HandleUnitNameChange(string characterName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleUnitNameChange({characterName})");

            HandleUnitNameChangeServer(characterName);
        }

        [ServerRpc]
        private void HandleUnitNameChangeServer(string characterName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleUnitNameChangeServer({characterName})");

            this.characterName.Value = characterName;
        }

        private void HandleNameSync(string oldValue, string newValue, bool asServer) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleNameSync({oldValue}, {newValue}, {asServer})");

            unitController.BaseCharacter.ChangeCharacterName(newValue);
        }

        private void CompleteCharacterRequest(bool isOwner) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner})");

            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName.Value);
            CharacterConfigurationRequest characterConfigurationRequest;
            if (isOwner && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId.Value)) {
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId.Value, isOwner);
            } else {
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

        public void HandleBeginChatMessageServer(string messageText) {
            HandleBeginChatMessageClient(messageText);
        }

        [ObserversRpc]
        public void HandleBeginChatMessageClient(string messageText) {
            unitController.BeginChatMessage(messageText);
        }

        public void HandlePerformCastingAbilityAnimationServer(BaseAbilityProperties baseAbility, int clipIndex) {
            HandlePerformAnimatedActionClient(baseAbility.ResourceName);
        }

        [ObserversRpc]
        public void HandlePerformCastingAbilityAnimationClient(string abilityName, int clipIndex) {
            BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
            if (baseAbility == null) {
                return;
            }
            unitController.UnitAnimator.PerformCastingAbility(baseAbility.AbilityProperties, clipIndex);
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
        public void HandleClearCastingClient() {
            unitController.UnitAnimator.ClearCasting();
        }


        public void SubscribeToServerUnitEvents() {
            if (unitController == null) {
                // something went wrong
                return;
            }

            unitController.UnitEventController.OnBeginChatMessage += HandleBeginChatMessageServer;
            unitController.UnitAnimator.OnPerformAnimatedActionAnimation += HandlePerformAnimatedActionServer;
            unitController.UnitAnimator.OnPerformCastingAbilityAnimation += HandlePerformCastingAbilityAnimationServer;
            unitController.UnitAnimator.OnClearAction += HandleClearActionClient;
            unitController.UnitAnimator.OnClearCasting += HandleClearCastingClient;
        }

        public void UnsubscribeFromServerUnitEvents() {
            if (unitController == null) {
                return;
            }
            unitController.UnitEventController.OnBeginChatMessage -= HandleBeginChatMessageServer;
            unitController.UnitAnimator.OnPerformAnimatedActionAnimation -= HandlePerformAnimatedActionServer;
            unitController.UnitAnimator.OnPerformCastingAbilityAnimation -= HandlePerformCastingAbilityAnimationServer;
            unitController.UnitAnimator.OnClearAction -= HandleClearActionClient;
            unitController.UnitAnimator.OnClearCasting -= HandleClearCastingClient;

        }

        public void SubscribeToClientUnitEvents() {
            if (unitController == null) {
                // something went wrong
                return;
            }

            if (base.IsOwner) {
                unitController.UnitEventController.OnBeginAction += HandleBeginAction;
                unitController.UnitEventController.OnBeginAbility += HandleBeginAbilityLocal;
            }
        }

        public void UnsubscribeFromClientUnitEvents() {
            if (unitController == null) {
                return;
            }
            if (base.IsOwner) {
                unitController.UnitEventController.OnBeginAction -= HandleBeginAction;
                unitController.UnitEventController.OnBeginAbility -= HandleBeginAbilityLocal;
            }
        }

        public void HandleBeginAbilityLocal(BaseAbilityProperties abilityProperties, Interactable target, bool playerInitiated) {
            NetworkCharacterUnit targetNetworkCharacterUnit = target.GetComponent<NetworkCharacterUnit>();
            HandleBeginAbilityServer(abilityProperties.ResourceName, targetNetworkCharacterUnit, playerInitiated);
        }

        [ServerRpc]
        public void HandleBeginAbilityServer(string abilityName, NetworkCharacterUnit target, bool playerInitiated) {
            BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
            if (baseAbility == null) {
                return;
            }
            Interactable targetInteractable = null;
            if (target != null) {
                targetInteractable = target.GetComponent<Interactable>();
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

        //[ObserversRpc]
        public void HandleSetAnimationClipOverride(string originalAnimationClipName, AnimationClip animationClip) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSetAnimationClipOverride()");

            //unitController.UnitAnimator.SetAnimationClipOverride(originalAnimationClipName, animationClip);
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
            //systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, spawnRequestId, base.isOwner);
        }

        public override void OnStopClient() {
            base.OnStopClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStopClient()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientUnitEvents();
            systemGameManager.NetworkManagerClient.ProcessStopClient(unitController);
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
            //systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, serverRequestId, false);
        }

        public override void OnStopServer() {
            base.OnStopServer();
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStopServer()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            UnsubscribeFromServerUnitEvents();
            systemGameManager.NetworkManagerServer.ProcessStopServer(unitController);
        }

        void OnDisable() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnDisable()");
        }

    }
}


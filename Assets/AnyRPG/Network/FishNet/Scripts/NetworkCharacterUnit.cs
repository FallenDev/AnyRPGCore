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
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStopClient()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientUnitEvents();
            systemGameManager.NetworkManagerClient.ProcessStopNetworkUnitClient(unitController);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartServer()");

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
                unitController.UnitEventController.OnRequestEquipToSlot += HandleRequestEquipToSlot;
                //unitController.UnitEventController.OnRequestUnequipFromList += HandleRequestUnequipFromList;
                unitController.UnitEventController.OnRequestDropItemFromInventorySlot += HandleRequestDropItemFromInventorySlot;
                unitController.UnitEventController.OnRequestMoveFromBankToInventory += HandleRequestMoveFromBankToInventory;
                unitController.UnitEventController.OnRequestMoveFromInventoryToBank += HandleRequestMoveFromInventoryToBank;
                unitController.UnitEventController.OnRequestUseItem += HandleRequestUseItem;
                unitController.UnitEventController.OnRequestSwapInventoryEquipment += HandleRequestSwapInventoryEquipment;
                unitController.UnitEventController.OnRequestUnequipToSlot += HandleRequestUnequipToSlot;
                unitController.UnitEventController.OnRequestSwapBags += HandleRequestSwapBags;
                unitController.UnitEventController.OnRequestUnequipBagToSlot += HandleRequestUnequipBagToSlot;
                unitController.UnitEventController.OnRequestUnequipBag += HandleRequestUnequipBag;
                unitController.UnitEventController.OnRequestMoveBag += HandleRequestMoveBag;
                unitController.UnitEventController.OnRequestAddBag += HandleRequestAddBagFromInventory;
                unitController.UnitEventController.OnSetGroundTarget += HandleSetGroundTarget;
                unitController.UnitEventController.OnRequestCancelStatusEffect += HandleRequestCancelStatusEffect;
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
                unitController.UnitEventController.OnRequestEquipToSlot -= HandleRequestEquipToSlot;
                //unitController.UnitEventController.OnRequestUnequipFromList -= HandleRequestUnequipFromList;
                unitController.UnitEventController.OnRequestDropItemFromInventorySlot -= HandleRequestDropItemFromInventorySlot;
                unitController.UnitEventController.OnRequestMoveFromBankToInventory -= HandleRequestMoveFromBankToInventory;
                unitController.UnitEventController.OnRequestMoveFromInventoryToBank -= HandleRequestMoveFromInventoryToBank;
                unitController.UnitEventController.OnRequestUseItem -= HandleRequestUseItem;
                unitController.UnitEventController.OnRequestSwapInventoryEquipment -= HandleRequestSwapInventoryEquipment;
                unitController.UnitEventController.OnRequestUnequipToSlot -= HandleRequestUnequipToSlot;
                unitController.UnitEventController.OnRequestSwapBags -= HandleRequestSwapBags;
                unitController.UnitEventController.OnRequestUnequipBagToSlot -= HandleRequestUnequipBagToSlot;
                unitController.UnitEventController.OnRequestUnequipBag -= HandleRequestUnequipBag;
                unitController.UnitEventController.OnRequestMoveBag -= HandleRequestMoveBag;
                unitController.UnitEventController.OnRequestAddBag -= HandleRequestAddBagFromInventory;
                unitController.UnitEventController.OnSetGroundTarget -= HandleSetGroundTarget;
                unitController.UnitEventController.OnRequestCancelStatusEffect -= HandleRequestCancelStatusEffect;
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
            unitController.UnitEventController.OnSpawnProjectileEffectPrefabs += HandleSpawnProjectileEffectPrefabsServer;
            unitController.UnitEventController.OnSpawnChanneledEffectPrefabs += HandleSpawnChanneledEffectPrefabsServer;
            unitController.UnitEventController.OnGainXP += HandleGainXPServer;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnDespawn += HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTriggerServer;
            unitController.UnitEventController.OnClassChange += HandleClassChangeServer;
            unitController.UnitEventController.OnSpecializationChange += HandleSpecializationChangeServer;
            unitController.UnitEventController.OnFactionChange += HandleFactionChangeServer;
            unitController.UnitEventController.OnEnterInteractableRange += HandleEnterInteractableRangeServer;
            unitController.UnitEventController.OnExitInteractableRange += HandleExitInteractableRangeServer;
            unitController.UnitEventController.OnAcceptQuest += HandleAcceptQuestServer;
            unitController.UnitEventController.OnAbandonQuest += HandleAbandonQuestServer;
            unitController.UnitEventController.OnTurnInQuest += HandleTurnInQuestServer;
            unitController.UnitEventController.OnMarkQuestComplete += HandleMarkQuestCompleteServer;
            //unitController.UnitEventController.OnRemoveQuest += HandleRemoveQuestServer;
            unitController.UnitEventController.OnLearnSkill += HandleLearnSkillServer;
            unitController.UnitEventController.OnUnLearnSkill += HandleUnLearnSkillServer;
            unitController.UnitEventController.OnSetQuestObjectiveCurrentAmount += HandleSetQuestObjectiveCurrentAmount;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated += HandleQuestObjectiveStatusUpdatedServer;
            //unitController.UnitEventController.OnStartInteractWithOption += HandleStartInteractWithOption;
            unitController.UnitEventController.OnGetNewInstantiatedItem += HandleGetNewInstantiatedItem;
            unitController.UnitEventController.OnDeleteItem += HandleDeleteItemServer;
            unitController.UnitEventController.OnAddEquipment += HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment += HandleRemoveEquipment;
            unitController.UnitEventController.OnAddItemToInventorySlot += HandleAddItemToInventorySlot;
            unitController.UnitEventController.OnRemoveItemFromInventorySlot += HandleRemoveItemFromInventorySlot;
            unitController.UnitEventController.OnAddItemToBankSlot += HandleAddItemToBankSlot;
            unitController.UnitEventController.OnRemoveItemFromBankSlot += HandleRemoveItemFromBankSlot;
            //unitController.UnitEventController.OnPlaceInEmpty += HandlePlaceInEmpty;
            unitController.UnitEventController.OnSetCraftAbility += HandleSetCraftAbilityServer;
            unitController.UnitEventController.OnCraftItem += HandleCraftItemServer;
            unitController.UnitEventController.OnRemoveFirstCraftingQueueItem += HandleRemoveFirstCraftingQueueItemServer;
            unitController.UnitEventController.OnClearCraftingQueue += HandleClearCraftingQueueServer;
            unitController.UnitEventController.OnAddToCraftingQueue += HandleAddToCraftingQueueServer;
            unitController.UnitEventController.OnCastTimeChanged += HandleCastTimeChanged;
            unitController.UnitEventController.OnCastComplete += HandleCastComplete;
            unitController.UnitEventController.OnCastCancel += HandleCastCancel;
            unitController.UnitEventController.OnRebuildModelAppearance += HandleRebuildModelAppearanceServer;
            unitController.UnitEventController.OnRemoveBag += HandleRemoveBagServer;
            unitController.UnitEventController.OnAddBag += HandleAddBagServer;
            unitController.UnitEventController.OnStatusEffectAdd += HandleStatusEffectAddServer;
            unitController.UnitEventController.OnAddStatusEffectStack += HandleAddStatusEffectStackServer;
            unitController.UnitEventController.OnCancelStatusEffect += HandleCancelStatusEffectServer;
            unitController.UnitEventController.OnCombatMessage += HandleCombatMessageServer;
            unitController.UnitEventController.OnReceiveCombatTextEvent += HandleReceiveCombatTextEventServer;
            unitController.UnitEventController.OnTakeDamage += HandleTakeDamageServer;
            unitController.UnitEventController.OnImmuneToEffect += HandleImmuneToEffectServer;
            unitController.UnitEventController.OnRecoverResource += HandleRecoverResourceServer;
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
            unitController.UnitEventController.OnSpawnProjectileEffectPrefabs -= HandleSpawnProjectileEffectPrefabsServer;
            unitController.UnitEventController.OnSpawnChanneledEffectPrefabs -= HandleSpawnChanneledEffectPrefabsServer;
            unitController.UnitEventController.OnGainXP -= HandleGainXPServer;
            unitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            unitController.UnitEventController.OnDespawn -= HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger -= HandleEnterInteractableTriggerServer;
            unitController.UnitEventController.OnClassChange -= HandleClassChangeServer;
            unitController.UnitEventController.OnSpecializationChange -= HandleSpecializationChangeServer;
            unitController.UnitEventController.OnFactionChange -= HandleFactionChangeServer;
            unitController.UnitEventController.OnEnterInteractableRange -= HandleEnterInteractableRangeServer;
            unitController.UnitEventController.OnExitInteractableRange -= HandleExitInteractableRangeServer;
            unitController.UnitEventController.OnAcceptQuest -= HandleAcceptQuestServer;
            unitController.UnitEventController.OnAbandonQuest -= HandleAbandonQuestServer;
            unitController.UnitEventController.OnTurnInQuest -= HandleTurnInQuestServer;
            unitController.UnitEventController.OnMarkQuestComplete -= HandleMarkQuestCompleteServer;
            //unitController.UnitEventController.OnRemoveQuest -= HandleRemoveQuestServer;
            unitController.UnitEventController.OnLearnSkill -= HandleLearnSkillServer;
            unitController.UnitEventController.OnUnLearnSkill -= HandleUnLearnSkillServer;
            unitController.UnitEventController.OnSetQuestObjectiveCurrentAmount -= HandleSetQuestObjectiveCurrentAmount;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated -= HandleQuestObjectiveStatusUpdatedServer;
            //unitController.UnitEventController.OnStartInteractWithOption -= HandleStartInteractWithOptionServer;
            unitController.UnitEventController.OnGetNewInstantiatedItem -= HandleGetNewInstantiatedItem;
            unitController.UnitEventController.OnDeleteItem -= HandleDeleteItemServer;
            unitController.UnitEventController.OnAddEquipment -= HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment -= HandleRemoveEquipment;
            unitController.UnitEventController.OnAddItemToInventorySlot -= HandleAddItemToInventorySlot;
            unitController.UnitEventController.OnRemoveItemFromInventorySlot -= HandleRemoveItemFromInventorySlot;
            unitController.UnitEventController.OnAddItemToBankSlot -= HandleAddItemToBankSlot;
            unitController.UnitEventController.OnRemoveItemFromBankSlot -= HandleRemoveItemFromBankSlot;
            unitController.UnitEventController.OnSetCraftAbility -= HandleSetCraftAbilityServer;
            unitController.UnitEventController.OnCraftItem -= HandleCraftItemServer;
            unitController.UnitEventController.OnRemoveFirstCraftingQueueItem -= HandleRemoveFirstCraftingQueueItemServer;
            unitController.UnitEventController.OnClearCraftingQueue -= HandleClearCraftingQueueServer;
            unitController.UnitEventController.OnAddToCraftingQueue -= HandleAddToCraftingQueueServer;
            unitController.UnitEventController.OnCastTimeChanged -= HandleCastTimeChanged;
            unitController.UnitEventController.OnCastComplete -= HandleCastComplete;
            unitController.UnitEventController.OnCastCancel -= HandleCastCancel;
            unitController.UnitEventController.OnRebuildModelAppearance -= HandleRebuildModelAppearanceServer;
            unitController.UnitEventController.OnRemoveBag -= HandleRemoveBagServer;
            unitController.UnitEventController.OnAddBag -= HandleAddBagServer;
            unitController.UnitEventController.OnStatusEffectAdd -= HandleStatusEffectAddServer;
            unitController.UnitEventController.OnAddStatusEffectStack -= HandleAddStatusEffectStackServer;
            unitController.UnitEventController.OnCancelStatusEffect -= HandleCancelStatusEffectServer;
            unitController.UnitEventController.OnCombatMessage -= HandleCombatMessageServer;
            unitController.UnitEventController.OnReceiveCombatTextEvent -= HandleReceiveCombatTextEventServer;
            unitController.UnitEventController.OnTakeDamage -= HandleTakeDamageServer;
            unitController.UnitEventController.OnImmuneToEffect -= HandleImmuneToEffectServer;
            unitController.UnitEventController.OnRecoverResource -= HandleRecoverResourceServer;
        }

        public void HandleRecoverResourceServer(PowerResource resource, int amount, CombatMagnitude magnitude, AbilityEffectContext context) {
            HandleRecoverResourceClient(resource.ResourceName, amount, magnitude, context.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleRecoverResourceClient(string resourceName, int amount, CombatMagnitude magnitude, SerializableAbilityEffectContext context) {
            PowerResource powerResource = systemDataFactory.GetResource<PowerResource>(resourceName);
            if (powerResource == null) {
                return;
            }
            unitController.UnitEventController.NotifyOnRecoverResource(powerResource, amount, magnitude, new AbilityEffectContext(unitController, null, context, systemGameManager));
        }

        public void HandleImmuneToEffectServer(AbilityEffectContext context) {
            HandleImmuneToEffectClient(context.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleImmuneToEffectClient(SerializableAbilityEffectContext context) {
            unitController.UnitEventController.NotifyOnImmuneToEffect(new AbilityEffectContext(unitController, null, context, systemGameManager));
        }

        public void HandleTakeDamageServer(IAbilityCaster sourceCaster, UnitController target, int amount, CombatTextType combatTextType, CombatMagnitude combatMagnitude, string abilityName, AbilityEffectContext context) {
            
            UnitController sourceUnitController = sourceCaster as UnitController;
            NetworkCharacterUnit networkCharacterUnit = null;
            if (sourceUnitController != null) {
                networkCharacterUnit = sourceUnitController.GetComponent<NetworkCharacterUnit>();
            }
            HandleTakeDamageClient(networkCharacterUnit, amount, combatTextType, combatMagnitude, abilityName, context.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleTakeDamageClient(NetworkCharacterUnit sourceNetworkCharacterUnit, int amount, CombatTextType combatTextType, CombatMagnitude combatMagnitude, string abilityName, SerializableAbilityEffectContext context) {
            IAbilityCaster sourceCaster = null;
            if (sourceNetworkCharacterUnit == null) {
                sourceCaster = systemGameManager.SystemAbilityController;
            } else {
                sourceCaster = sourceNetworkCharacterUnit.UnitController;
            }
            unitController.UnitEventController.NotifyOnTakeDamage(sourceCaster, unitController, amount, combatTextType, combatMagnitude, abilityName, new AbilityEffectContext(unitController, null, context, systemGameManager));
        }

        public void HandleReceiveCombatTextEventServer(UnitController targetUnitController, int amount, CombatTextType type, CombatMagnitude magnitude, AbilityEffectContext context) {
            NetworkCharacterUnit networkCharacterUnit = null;
            if (targetUnitController != null) {
                networkCharacterUnit = targetUnitController.GetComponent<NetworkCharacterUnit>();
            }
            ReceiveCombatTextEventClient(networkCharacterUnit, amount, type, magnitude, context.GetSerializableContext());
        }

        [ObserversRpc]
        public void ReceiveCombatTextEventClient(NetworkCharacterUnit targetNetworkCharacterUnit, int amount, CombatTextType type, CombatMagnitude magnitude, SerializableAbilityEffectContext context) {
            if (targetNetworkCharacterUnit != null) {
                unitController.UnitEventController.NotifyOnReceiveCombatTextEvent(targetNetworkCharacterUnit.unitController, amount, type, magnitude, new AbilityEffectContext(unitController, null, context, systemGameManager));
            }
        }

        public void HandleCombatMessageServer(string message) {
            HandleCombatMessageClient(message);
        }

        [ObserversRpc]
        public void HandleCombatMessageClient(string message) {
            unitController.UnitEventController.NotifyOnCombatMessage(message);
        }

        public void HandleCancelStatusEffectServer(StatusEffectProperties properties) {
            CancelStatusEffectClient(properties.ResourceName);
        }

        [ObserversRpc]
        public void CancelStatusEffectClient(string resourceName) {
            StatusEffect statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffect;
            if (statusEffect == null) {
                return;
            }
            unitController.CharacterStats.CancelStatusEffect(statusEffect.StatusEffectProperties);
        }

        public void HandleAddStatusEffectStackServer(string resourceName) {
            AddStatusEffectStackClient(resourceName);
        }

        [ObserversRpc]
        public void AddStatusEffectStackClient(string resourceName) {
            StatusEffect statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffect;
            if (statusEffect == null) {
                return;
            }
            unitController.CharacterStats.AddStatusEffectStack(statusEffect.StatusEffectProperties);
        }

        public void HandleStatusEffectAddServer(StatusEffectNode statusEffectNode) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleStatusEffectAddServer({statusEffectNode.StatusEffect.ResourceName})");

            NetworkCharacterUnit sourceNetworkCharacterUnit = statusEffectNode.AbilityEffectContext.AbilityCaster?.AbilityManager.UnitGameObject.GetComponent<NetworkCharacterUnit>();
            
            AddStatusEffectClient(statusEffectNode.StatusEffect.ResourceName, sourceNetworkCharacterUnit);
        }

        [ObserversRpc]
        public void AddStatusEffectClient(string resourceName, NetworkCharacterUnit sourceNetworkCharacterUnit) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.AddStatusEffectClient({resourceName}, {sourceNetworkCharacterUnit?.gameObject.name})");

            StatusEffect statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffect;
            if (statusEffect == null) {
                return;
            }
            IAbilityCaster abilityCaster = null;
            if (sourceNetworkCharacterUnit != null) {
                abilityCaster = sourceNetworkCharacterUnit.UnitController;
            } else {
                abilityCaster = systemGameManager.SystemAbilityController;
            }
            unitController.CharacterStats.AddNewStatusEffect(statusEffect.StatusEffectProperties, abilityCaster, new AbilityEffectContext(abilityCaster));
        }

        public void HandleAddBagServer(InstantiatedBag instantiatedBag, BagNode node) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleAddBagServer({instantiatedBag.Bag.ResourceName}, {node.NodeIndex})");

            HandleAddBagClient(instantiatedBag.InstanceId, node.NodeIndex, node.IsBankNode);
        }

        [ObserversRpc]
        public void HandleAddBagClient(int itemInstanceId, int nodeIndex, bool isBankNode) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleAddBagClient({itemInstanceId}, {nodeIndex}, {isBankNode})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                BagNode bagNode = null;
                if (isBankNode && unitController.CharacterInventoryManager.BankNodes.Count > nodeIndex) {
                    bagNode = unitController.CharacterInventoryManager.BankNodes[nodeIndex];
                } else if (isBankNode == false && unitController.CharacterInventoryManager.BagNodes.Count > nodeIndex) {
                    bagNode = unitController.CharacterInventoryManager.BagNodes[nodeIndex];
                } else {
                    // invalid index
                    return;
                }
                unitController.CharacterInventoryManager.AddBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, bagNode);
            }
        }

        public void HandleRemoveBagServer(InstantiatedBag bag) {
            HandleRemoveBagClient(bag.InstanceId);
        }

        [ObserversRpc]
        public void HandleRemoveBagClient(int itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.RemoveBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, true);
            }
        }

        public void HandleRebuildModelAppearanceServer() {
            HandleRebuildModelAppearanceClient();
        }

        [ObserversRpc]
        public void HandleRebuildModelAppearanceClient() {
            unitController.UnitModelController.RebuildModelAppearance();
        }

        public void HandleCastTimeChanged(IAbilityCaster abilityCaster, AbilityProperties abilityProperties, float castPercent) {
            HandleCastTimeChangedClient(abilityProperties.ResourceName, castPercent);
        }

        [ObserversRpc]
        public void HandleCastTimeChangedClient(string abilityName, float castPercent) {
            Ability ability = systemDataFactory.GetResource<Ability>(abilityName);
            unitController.UnitEventController.NotifyOnCastTimeChanged(unitController, ability.AbilityProperties, castPercent);
        }

        public void HandleCastComplete() {
            HandleCastCompleteClient();
        }

        [ObserversRpc]
        public void HandleCastCompleteClient() {
            unitController.UnitEventController.NotifyOnCastComplete();
        }

        public void HandleCastCancel() {
            HandleCastCancelClient();
        }

        [ObserversRpc]
        public void HandleCastCancelClient() {
            unitController.UnitEventController.NotifyOnCastCancel();
        }

        public void HandleAddToCraftingQueueServer(Recipe recipe) {
            HandleAddToCraftingQueueClient(recipe.ResourceName);
        }

        [ObserversRpc]
        public void HandleAddToCraftingQueueClient(string recipeName) {
            Recipe recipe = systemDataFactory.GetResource<Recipe>(recipeName);
            if (recipe == null) {
                return;
            }
            unitController.CharacterCraftingManager.AddToCraftingQueue(recipe);
        }

        public void HandleClearCraftingQueueServer() {
            HandleClearCraftingQueueClient();
        }

        [ObserversRpc]
        public void HandleClearCraftingQueueClient() {
            unitController.CharacterCraftingManager.ClearCraftingQueue();
        }

        public void HandleRemoveFirstCraftingQueueItemServer() {
            HandleRemoveFirstCraftingQueueItemClient();
        }

        [ObserversRpc]
        public void HandleRemoveFirstCraftingQueueItemClient() {
            unitController.CharacterCraftingManager.RemoveFirstQueueItem();
        }

        public void HandleCraftItemServer() {
            HandleCraftItemClient();
        }

        [ObserversRpc]
        public void HandleCraftItemClient() {
            unitController.UnitEventController.NotifyOnCraftItem();
        }

        public void HandleSetCraftAbilityServer(CraftAbilityProperties abilityProperties) {
            HandleSetCraftAbilityClient(abilityProperties.ResourceName);
        }

        [ObserversRpc]
        public void HandleSetCraftAbilityClient(string craftAbilityName) {
            CraftAbility craftAbility = systemDataFactory.GetResource<Ability>(craftAbilityName) as CraftAbility;
            if (craftAbility == null) {
                return;
            }
            unitController.CharacterCraftingManager.SetCraftAbility(craftAbility.CraftAbilityProperties);
        }

        public void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"{unitController.gameObject.name}.NetworkCharacterUnit.HandleAddItemToInventorySlot({item.Item.ResourceName}({item.InstanceId}))");

            int slotIndex = slot.GetCurrentInventorySlotIndex(unitController);
            AddItemToInventorySlotClient(slotIndex, item.InstanceId);
        }

        [ObserversRpc]
        public void AddItemToInventorySlotClient(int slotIndex, int itemInstanceId) {
            Debug.Log($"{unitController.gameObject.name}.NetworkCharacterUnit.AddItemToInventorySlotClient({slotIndex}, {itemInstanceId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.AddInventoryItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem item) {
            Debug.Log($"{unitController.gameObject.name}.NetworkCharacterUnit.HandleRemoveItemFromInventorySlot({item.Item.ResourceName})");

            RemoveItemFromInventorySlotClient(slot.GetCurrentInventorySlotIndex(unitController), item.InstanceId);

        }

        [ObserversRpc]
        public void RemoveItemFromInventorySlotClient(int slotIndex, int itemInstanceId) {
            Debug.Log($"{unitController.gameObject.name}.NetworkCharacterUnit.RemoveItemFromInventorySlotClient({slotIndex}, {itemInstanceId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.RemoveInventoryItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            AddItemToBankSlotClient(slot.GetCurrentBankSlotIndex(unitController), item.InstanceId);
        }

        [ObserversRpc]
        public void AddItemToBankSlotClient(int slotIndex, int itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.AddBankItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            RemoveItemFromBankSlotClient(slot.GetCurrentBankSlotIndex(unitController), item.InstanceId);
        }

        [ObserversRpc]
        public void RemoveItemFromBankSlotClient(int slotIndex, int itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.RemoveBankItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            HandleRemoveEquipmentClient(profile.ResourceName, equipment.InstanceId);
        }

        [ObserversRpc]
        public void HandleRemoveEquipmentClient(string equipmentSlotProfileName, int itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].RemoveItem(systemItemManager.InstantiatedItems[itemInstanceId]);
            }

        }

        public void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleAddEquipment({profile.ResourceName}, {equipment.Equipment.ResourceName})");

            HandleAddEquipmentClient(profile.ResourceName, equipment.InstanceId);
        }

        [ObserversRpc]
        public void HandleAddEquipmentClient(string equipmentSlotProfileName, int itemInstanceId) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleAddEquipmentClient({equipmentSlotProfileName}, {itemInstanceId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].AddItem(systemItemManager.InstantiatedItems[itemInstanceId]);
            }
        }

        /*
        public void HandleRequestUnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            RequestUnequipFromList(equipmentSlotProfile.ResourceName);
        }

        [ServerRpc]
        public void RequestUnequipFromList(string equipmentSlotProfileName) {
            EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
            if (equipmentSlotProfile == null) {
                return;
            }
            unitController.CharacterEquipmentManager.UnequipFromList(equipmentSlotProfile);
        }
        */

        private void HandleRequestMoveFromBankToInventory(int slotIndex) {
            RequestMoveFromBankToInventory(slotIndex);
        }

        [ServerRpc]
        private void RequestMoveFromBankToInventory(int slotIndex) {
            unitController.CharacterInventoryManager.MoveFromBankToInventory(slotIndex);
        }

        public void HandleRequestUseItem(int slotIndex) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleRequestUseItemClient({slotIndex})");

            RequestUseItemClient(slotIndex);
        }

        [ServerRpc]
        private void RequestUseItemClient(int slotIndex) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.RequestUseItemClient({slotIndex})");

            unitController.CharacterInventoryManager.UseItem(slotIndex);
        }

        public void HandleRequestSwapInventoryEquipment(InstantiatedEquipment oldEquipment, InstantiatedEquipment newEquipment) {
            RequestSwapInventoryEquipment(oldEquipment.InstanceId, newEquipment.InstanceId);
        }

        [ServerRpc]
        public void RequestSwapInventoryEquipment(int oldEquipmentInstanceId, int newEquipmentInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(oldEquipmentInstanceId) && systemItemManager.InstantiatedItems[oldEquipmentInstanceId] is InstantiatedEquipment) {
                unitController.CharacterEquipmentManager.SwapInventoryEquipment(systemItemManager.InstantiatedItems[oldEquipmentInstanceId] as InstantiatedEquipment, systemItemManager.InstantiatedItems[newEquipmentInstanceId] as InstantiatedEquipment);
            }
        }

        public void HandleRequestUnequipToSlot(InstantiatedEquipment equipment, int inventorySlotId) {
            RequestUnequipToSlot(equipment.InstanceId, inventorySlotId);
        }

        [ServerRpc]
        public void RequestUnequipToSlot(int itemInstanceId, int inventorySlotId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                unitController.CharacterEquipmentManager.UnequipToSlot(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedEquipment, inventorySlotId);
            }
        }

        public void HandleRequestSwapBags(InstantiatedBag oldBag, InstantiatedBag newBag) {
            RequestSwapBags(oldBag.InstanceId, newBag.InstanceId);
        }

        [ServerRpc]
        public void RequestSwapBags(int oldBagInstanceId, int newBagInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(oldBagInstanceId)
                && systemItemManager.InstantiatedItems[oldBagInstanceId] is InstantiatedBag
                && systemItemManager.InstantiatedItems.ContainsKey(newBagInstanceId)
                && systemItemManager.InstantiatedItems[newBagInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.SwapEquippedOrUnequippedBags(systemItemManager.InstantiatedItems[oldBagInstanceId] as InstantiatedBag, systemItemManager.InstantiatedItems[newBagInstanceId] as InstantiatedBag);
            }
        }

        public void HandleRequestUnequipBagToSlot(InstantiatedBag bag, int slotIndex, bool isBank) {
            RequestUnequipBagToSlot(bag.InstanceId, slotIndex, isBank);
        }

        [ServerRpc]
        public void RequestUnequipBagToSlot(int itemInstanceId, int slotIndex, bool isBank) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.UnequipBagToSlot(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, slotIndex, isBank);
            }
        }

        public void HandleRequestUnequipBag(InstantiatedBag bag, bool isBank) {
            RequestUnequipBag(bag.InstanceId, isBank);
        }

        [ServerRpc]
        public void RequestUnequipBag(int itemInstanceId, bool isBank) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.UnequipBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, isBank);
            }
        }

        public void HandleRequestMoveBag(InstantiatedBag bag, int nodeIndex, bool isBankNode) {
            RequestMoveBag(bag.InstanceId, nodeIndex, isBankNode);
        }

        [ServerRpc]
        public void RequestMoveBag(int itemInstanceId, int nodeIndex, bool isBankNode) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.MoveBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, nodeIndex, isBankNode);
            }
        }

        public void HandleSetGroundTarget(Vector3 vector) {
            SetGroundTargetServer(vector);
        }

        [ServerRpc]
        public void SetGroundTargetServer(Vector3 vector) {
            unitController.CharacterAbilityManager.SetGroundTarget(vector);
        }

        public void HandleRequestAddBagFromInventory(InstantiatedBag instantiatedBag, int nodeIndex, bool isBankNode) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleRequestAddBagFromInventory({instantiatedBag.InstanceId}, {nodeIndex}, {isBankNode})");
            RequestAddBagFromInventory(instantiatedBag.InstanceId, nodeIndex, isBankNode);
        }

        [ServerRpc]
        public void RequestAddBagFromInventory(int itemInstanceId, int nodeIndex, bool isBankNode) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.RequestAddBagFromInventory({itemInstanceId}, {nodeIndex}, {isBankNode})");
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.AddBagFromInventory(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, nodeIndex, isBankNode);
            }
        }

        public void HandleRequestCancelStatusEffect(StatusEffectProperties properties) {
            RequestCancelStatusEffect(properties.ResourceName);
        }

        [ServerRpc]
        public void RequestCancelStatusEffect(string resourceName) {
            StatusEffect statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffect;
            if (statusEffect == null) {
                return;
            }
            unitController.CharacterStats.CancelStatusEffect(statusEffect.StatusEffectProperties);
        }


        public void HandleRequestMoveFromInventoryToBank(int slotIndex) {
            RequestMoveFromInventoryToBank(slotIndex);
        }

        [ServerRpc]
        private void RequestMoveFromInventoryToBank(int slotIndex) {
            unitController.CharacterInventoryManager.MoveFromInventoryToBank(slotIndex);
        }

        private void HandleRequestDropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot, bool fromSlotIsInventory, bool toSlotIsInventory) {
            int fromSlotIndex;
            if (fromSlotIsInventory) {
                fromSlotIndex = fromSlot.GetCurrentInventorySlotIndex(unitController);
            } else {
                fromSlotIndex = fromSlot.GetCurrentBankSlotIndex(unitController);
            }
            int toSlotIndex;
            if (toSlotIsInventory) {
                toSlotIndex = toSlot.GetCurrentInventorySlotIndex(unitController);
            } else {
                toSlotIndex = toSlot.GetCurrentBankSlotIndex(unitController);
            }
            RequestDropItemFromInventorySlot(fromSlotIndex, toSlotIndex, fromSlotIsInventory, toSlotIsInventory);
        }

        [ServerRpc]
        private void RequestDropItemFromInventorySlot(int fromSlotId, int toSlotId, bool fromSlotIsInventory, bool toSlotIsInventory) {
            unitController.CharacterInventoryManager.DropItemFromInventorySlot(fromSlotId, toSlotId, fromSlotIsInventory, toSlotIsInventory);
        }


        public void HandleRequestEquipToSlot(InstantiatedEquipment equipment, EquipmentSlotProfile profile) {
            RequestEquipToSlot(equipment.InstanceId, profile.ResourceName);
        }

        [ServerRpc]
        public void RequestEquipToSlot(int itemInstanceId, string equipmentSlotProfileName) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.EquipToSlot(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedEquipment, equipmentSlotProfile);
            }
        }


        public void HandleDeleteItemServer(InstantiatedItem item) {
            HandleDeleteItemClient(item.InstanceId);
        }

        [ObserversRpc]
        public void HandleDeleteItemClient(int itemInstanceId) {
            unitController.CharacterInventoryManager.DeleteItem(itemInstanceId);
        }

        public void HandleGetNewInstantiatedItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleGetNewInstantiatedItem({instantiatedItem.InstanceId})");
            
            InventorySlotSaveData inventorySlotSaveData = instantiatedItem.GetSlotSaveData();
            HandleGetNewInstantiatedItemClient(instantiatedItem.InstanceId, inventorySlotSaveData);
        }

        [ObserversRpc]
        public void HandleGetNewInstantiatedItemClient(int itemInstanceId, InventorySlotSaveData inventorySlotSaveData) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleGetNewInstantiatedItemClient{itemInstanceId}, {inventorySlotSaveData.ItemName}");
            
            unitController.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData(itemInstanceId, inventorySlotSaveData.ItemName, inventorySlotSaveData);
        }

        public void HandleMarkQuestCompleteServer(UnitController controller, QuestBase questBase) {
            HandleMarkQuestCompleteClient(questBase.DisplayName);
        }

        [ObserversRpc]
        public void HandleMarkQuestCompleteClient(string questName) {
            Quest questBase = systemDataFactory.GetResource<Quest>(questName);
            if (questBase == null) {
                return;
            }
            questBase.MarkComplete(unitController, true, false);
        }

        public void HandleQuestObjectiveStatusUpdatedServer(UnitController controller, QuestBase questBase) {
            HandleQuestObjectiveStatusUpdatedClient(questBase.ResourceName);
        }

        [ObserversRpc]
        public void HandleQuestObjectiveStatusUpdatedClient(string questName) {
            Quest questBase = systemDataFactory.GetResource<Quest>(questName);
            if (questBase == null) {
                return;
            }
            unitController.UnitEventController.NotifyOnQuestObjectiveStatusUpdated(questBase);
        }

        [ObserversRpc]
        public void HandleSetQuestObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, QuestObjectiveSaveData saveData) {
            unitController.CharacterQuestLog.SetQuestObjectiveCurrentAmount(questName, objectiveType, objectiveName, saveData);
        }

        public void HandleLearnSkillServer(UnitController sourceUnitController, Skill skill) {
            HandleLearnSkillClient(skill.ResourceName);
        }

        [ObserversRpc]
        public void HandleLearnSkillClient(string skillName) {
            Skill skill = systemDataFactory.GetResource<Skill>(skillName);
            if (skill != null) {
                unitController.CharacterSkillManager.LearnSkill(skill);
            }
        }

        public void HandleUnLearnSkillServer(UnitController sourceUnitController, Skill skill) {
            HandleUnLearnSkillClient(skill.ResourceName);
        }

        [ObserversRpc]
        public void HandleUnLearnSkillClient(string skillName) {
            Skill skill = systemDataFactory.GetResource<Skill>(skillName);
            if (skill != null) {
                unitController.CharacterSkillManager.UnLearnSkill(skill);
            }
        }

        public void HandleAcceptQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleAcceptQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleAcceptQuestClient(string questName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleAcceptQuestClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.AcceptQuest(quest);
            }
        }

        public void HandleAbandonQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleAbandonQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleAbandonQuestClient(string questName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleAbandonQuestClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.AbandonQuest(quest);
            }
        }

        public void HandleTurnInQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleTurnInQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleTurnInQuestClient(string questName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleTurnInQuestClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.TurnInQuest(quest);
            }
        }

        public void HandleRemoveQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleRemoveQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleRemoveQuestClient(string questName) {
            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.RemoveQuest(quest);
            }
        }

        private void HandleEnterInteractableRangeServer(UnitController controller, Interactable interactable) {

            NetworkInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<NetworkInteractable>();
            }
            HandleEnterInteractableRangeClient(networkInteractable);
        }

        [ObserversRpc]
        private void HandleEnterInteractableRangeClient(NetworkInteractable networkInteractable) {
            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            unitController.UnitEventController.NotifyOnEnterInteractableRange(interactable);
        }

        private void HandleExitInteractableRangeServer(UnitController controller, Interactable interactable) {

            NetworkInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<NetworkInteractable>();
            }
            HandleExitInteractableRangeClient(networkInteractable);
        }

        [ObserversRpc]
        private void HandleExitInteractableRangeClient(NetworkInteractable networkInteractable) {
            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            unitController.UnitEventController.NotifyOnExitInteractableRange(interactable);
        }


        public void HandleSpecializationChangeServer(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            HandleSpecializationChangeClient(newSpecialization.ResourceName);
        }

        [ObserversRpc]
        public void HandleSpecializationChangeClient(string newSpecializationName) {
            ClassSpecialization newSpecialization = systemDataFactory.GetResource<ClassSpecialization>(newSpecializationName);
            unitController.BaseCharacter.ChangeClassSpecialization(newSpecialization);
        }

        public void HandleFactionChangeServer(Faction newFaction, Faction oldFaction) {
            HandleFactionChangeClient(newFaction.ResourceName);
        }

        [ObserversRpc]
        public void HandleFactionChangeClient(string newFactionName) {
            Faction newFaction = systemDataFactory.GetResource<Faction>(newFactionName);
            if (newFaction == null) {
                return;
            }
            unitController.BaseCharacter.ChangeCharacterFaction(newFaction);
        }


        public void HandleClassChangeServer(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            HandleClassChangeClient(newCharacterClass.ResourceName);
        }

        [ObserversRpc]
        public void HandleClassChangeClient(string newCharacterClassName) {
            CharacterClass newCharacterClass = systemDataFactory.GetResource<CharacterClass>(newCharacterClassName);
            unitController.BaseCharacter.ChangeCharacterClass(newCharacterClass);
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

        public void HandleSpawnAbilityEffectPrefabsServer(Interactable target, Interactable originalTarget, LengthEffectProperties lengthEffectProperties, AbilityEffectContext abilityEffectContext) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityEffectPrefabsServer()");

            NetworkInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<NetworkInteractable>();
            }
            NetworkInteractable networkOriginalTarget = null;
            if (originalTarget != null) {
                networkOriginalTarget = originalTarget.GetComponent<NetworkInteractable>();
            }
            HandleSpawnAbilityEffectPrefabsClient(networkTarget, networkOriginalTarget, lengthEffectProperties.ResourceName, abilityEffectContext.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleSpawnAbilityEffectPrefabsClient(NetworkInteractable networkTarget, NetworkInteractable networkOriginalTarget, string abilityEffectName, SerializableAbilityEffectContext serializableAbilityEffectContext) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnAbilityObjectsClient({networkTarget?.gameObject.name}, {networkOriginalTarget?.gameObject.name}, {abilityEffectName})");

            AbilityEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
            if (abilityEffect == null) {
                return;
            }
            FixedLengthEffectProperties fixedLengthEffectProperties = abilityEffect.AbilityEffectProperties as FixedLengthEffectProperties;
            if (fixedLengthEffectProperties == null) {
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
            unitController.CharacterAbilityManager.SpawnAbilityEffectPrefabs(target, originalTarget, fixedLengthEffectProperties, new AbilityEffectContext(unitController, originalTarget, serializableAbilityEffectContext, systemGameManager));
        }

        public void HandleSpawnProjectileEffectPrefabsServer(Interactable target, Interactable originalTarget, ProjectileEffectProperties projectileEffectProperties, AbilityEffectContext abilityEffectContext) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnProjectileEffectPrefabsServer({target?.gameObject.name}, {originalTarget?.gameObject.name}, {projectileEffectProperties.ResourceName})");

            NetworkInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<NetworkInteractable>();
            }
            NetworkInteractable networkOriginalTarget = null;
            if (target != null) {
                networkOriginalTarget = originalTarget.GetComponent<NetworkInteractable>();
            }
            HandleSpawnProjectileEffectPrefabsClient(networkTarget, networkOriginalTarget, projectileEffectProperties.ResourceName, abilityEffectContext.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleSpawnProjectileEffectPrefabsClient(NetworkInteractable networkTarget, NetworkInteractable networkOriginalTarget, string abilityEffectName, SerializableAbilityEffectContext serializableAbilityEffectContext) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnProjectileEffectPrefabsClient({networkTarget?.gameObject.name}, {networkOriginalTarget?.gameObject.name}, {abilityEffectName})");

            ProjectileEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName) as ProjectileEffect;
            if (abilityEffect == null) {
                return;
            }
            ProjectileEffectProperties projectileEffectProperties = abilityEffect.AbilityEffectProperties as ProjectileEffectProperties;
            if (projectileEffectProperties == null) {
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
            unitController.CharacterAbilityManager.SpawnProjectileEffectPrefabs(target, originalTarget, projectileEffectProperties, new AbilityEffectContext(unitController, originalTarget, serializableAbilityEffectContext, systemGameManager));
        }

        public void HandleSpawnChanneledEffectPrefabsServer(Interactable target, Interactable originalTarget, ChanneledEffectProperties channeledEffectProperties, AbilityEffectContext abilityEffectContext) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnChanneledEffectPrefabsServer({target?.gameObject.name}, {originalTarget?.gameObject.name}, {channeledEffectProperties.ResourceName})");

            NetworkInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<NetworkInteractable>();
            }
            NetworkInteractable networkOriginalTarget = null;
            if (target != null) {
                networkOriginalTarget = originalTarget.GetComponent<NetworkInteractable>();
            }
            HandleSpawnChanneledEffectPrefabsClient(networkTarget, networkOriginalTarget, channeledEffectProperties.ResourceName, abilityEffectContext.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleSpawnChanneledEffectPrefabsClient(NetworkInteractable networkTarget, NetworkInteractable networkOriginalTarget, string abilityEffectName, SerializableAbilityEffectContext serializableAbilityEffectContext) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleSpawnProjectileEffectPrefabsClient({networkTarget?.gameObject.name}, {networkOriginalTarget?.gameObject.name}, {abilityEffectName})");

            ChanneledEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName) as ChanneledEffect;
            if (abilityEffect == null) {
                return;
            }
            ChanneledEffectProperties channeledEffectProperties = abilityEffect.AbilityEffectProperties as ChanneledEffectProperties;
            if (channeledEffectProperties == null) {
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
            unitController.CharacterAbilityManager.SpawnChanneledEffectPrefabs(target, originalTarget, channeledEffectProperties, new AbilityEffectContext(unitController, originalTarget, serializableAbilityEffectContext, systemGameManager));
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
                unitController.CharacterAbilityManager.SpawnAbilityObjectsInternal(ability.AbilityProperties, index);
            }
        }

        [ObserversRpc]
        public void HandleDespawnAbilityObjects() {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleDespawnAbilityObjects()");

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
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleBeforeDieServer(" + (targetUnitController == null ? "null" : targetUnitController.gameObject.name) + ")");

            HandleBeforeDieClient();
        }

        [ObserversRpc]
        public void HandleBeforeDieClient() {
            //Debug.Log($"{gameObject.name}.HandleBeforeDieClient()");

            unitController.CharacterStats.Die();
        }

        private void HandleClearTargetClient(Interactable oldTarget) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleClearTargetClient(" + (oldTarget == null ? "null" : oldTarget.gameObject.name) + ")");

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
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner})");

            /*
            if (base.Owner != null ) {
                Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) owner accountId: {base.OwnerId}");
            }
            */

            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName.Value);
            CharacterConfigurationRequest characterConfigurationRequest;
            if (networkManagerServer.ServerModeActive == false) {
                if (isOwner && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId.Value)) {
                    //Debug.Log("this is happening on the client, and we own the object, so we requested it");
                    systemGameManager.CharacterManager.AddServerSpawnRequestId(clientSpawnRequestId.Value, serverSpawnRequestId.Value);
                    systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId.Value, isOwner);
                } else {
                    //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) falling back to creating new config request");
                    //Debug.Log("this is happening on the client, and we do not own the object, so we need to create a new config request");
                    characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
                    characterConfigurationRequest.characterName = characterName.Value;
                    characterConfigurationRequest.unitLevel = unitLevel.Value;
                    characterConfigurationRequest.unitControllerMode = unitControllerMode.Value;
                    characterConfigurationRequest.characterAppearanceData = characterAppearanceData.Value;
                    CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
                    characterRequestData.clientSpawnRequestId = clientSpawnRequestId.Value;
                    characterRequestData.serverSpawnRequestId = serverSpawnRequestId.Value;
                    characterRequestData.isServer = false;
                    systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, characterRequestData, isOwner);
                }
            } else {
                if (systemGameManager.CharacterManager.HasUnitSpawnRequest(serverSpawnRequestId.Value) == true) {
                    //Debug.Log("this is happening on the server, which will always have a requestor (unit spawn node, networkManager, etc.)");
                    systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, serverSpawnRequestId.Value, isOwner);
                } else {
                    Debug.Log("this is happening on the server, which should always have a requestor, but we could not find one!");
                }
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
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandlePerformAbilityCastAnimationServer({baseAbility.ResourceName}, {clipIndex})");

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
            unitController.CharacterAbilityManager.BeginAbility(baseAbility.AbilityProperties, targetInteractable, playerInitiated);
        }

        [ServerRpc]
        public void HandleBeginAction(string actionName, bool playerInitiated) {
            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction == null) {
                return;
            }
            unitController.UnitActionManager.BeginActionInternal(animatedAction, playerInitiated);
        }

    }
}


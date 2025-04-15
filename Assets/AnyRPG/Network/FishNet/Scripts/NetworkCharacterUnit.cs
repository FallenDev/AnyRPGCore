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
                unitController.UnitEventController.OnRequestEquipEquipment += HandleRequestEquipEquipment;
                unitController.UnitEventController.OnRequestUnequipFromList += HandleRequestUnequipFromList;
                unitController.UnitEventController.OnRequestDropItemFromInventorySlot += HandleRequestDropItemFromInventorySlot;
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
                unitController.UnitEventController.OnRequestEquipEquipment -= HandleRequestEquipEquipment;
                unitController.UnitEventController.OnRequestUnequipFromList -= HandleRequestUnequipFromList;
                unitController.UnitEventController.OnRequestDropItemFromInventorySlot -= HandleRequestDropItemFromInventorySlot;
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
            unitController.UnitEventController.OnClassChange += HandleClassChangeServer;
            unitController.UnitEventController.OnSpecializationChange += HandleSpecializationChangeServer;
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
            unitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            unitController.UnitEventController.OnDespawn -= HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger -= HandleEnterInteractableTriggerServer;
            unitController.UnitEventController.OnClassChange -= HandleClassChangeServer;
            unitController.UnitEventController.OnSpecializationChange -= HandleSpecializationChangeServer;
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
            //Debug.Log($"{unitController.gameObject.name}.NetworkCharacterUnit.HandleRemoveItemFromInventorySlot({item.Item.ResourceName})");

            RemoveItemFromInventorySlotClient(slot.GetCurrentInventorySlotIndex(unitController), item.InstanceId);

        }

        [ObserversRpc]
        public void RemoveItemFromInventorySlotClient(int slotIndex, int itemInstanceId) {
            //Debug.Log($"{unitController.gameObject.name}.NetworkCharacterUnit.RemoveItemFromInventorySlotClient({slotIndex}, {itemInstanceId})");

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
            HandleAddEquipmentClient(profile.ResourceName, equipment.InstanceId);
        }

        [ObserversRpc]
        public void HandleAddEquipmentClient(string equipmentSlotProfileName, int itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].AddItem(systemItemManager.InstantiatedItems[itemInstanceId]);
            }
        }

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

        private void HandleRequestDropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot) {
            RequestDropItemFromInventorySlot(fromSlot.GetCurrentInventorySlotIndex(unitController), toSlot.GetCurrentInventorySlotIndex(unitController));
        }

        [ServerRpc]
        private void RequestDropItemFromInventorySlot(int fromSlotId, int toSlotId) {
            unitController.CharacterInventoryManager.DropItemFromInventorySlot(fromSlotId, toSlotId);
        }


        public void HandleRequestEquipEquipment(InstantiatedEquipment equipment, EquipmentSlotProfile profile) {
            RequestEquipEquipment(equipment.InstanceId, profile.ResourceName);
        }

        [ServerRpc]
        public void RequestEquipEquipment(int itemInstanceId, string equipmentSlotProfileName) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.EquipEquipment(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedEquipment, equipmentSlotProfile);
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
                unitController.CharacterAbilityManager.SpawnAbilityObjectsInternal(ability.AbilityProperties, index);
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
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner})");

            /*
            if (base.Owner != null ) {
                Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) owner clientId: {base.OwnerId}");
            }
            */

            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName.Value);
            CharacterConfigurationRequest characterConfigurationRequest;
            if (isOwner && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId.Value)) {
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId.Value, isOwner);
            } else if (base.OwnerId == -1 && networkManagerServer.ServerModeActive == true && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId.Value) == true) {
                //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) owner is -1");
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId.Value, isOwner);
            } else {
                //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner}) falling back to creating new config request");
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

    }
}


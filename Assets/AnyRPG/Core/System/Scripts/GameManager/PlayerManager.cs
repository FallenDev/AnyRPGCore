using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PlayerManager : ConfiguredMonoBehaviour, ICharacterRequestor {

        [SerializeField]
        private float maxMovementSpeed = 20f;

        [SerializeField]
        private LayerMask defaultGroundMask;

        [SerializeField]
        private GameObject playerConnectionParent = null;

        [SerializeField]
        private GameObject playerConnectionPrefab = null;

        [SerializeField]
        private GameObject playerUnitParent = null;

        [SerializeField]
        private GameObject aiUnitParent = null;

        [SerializeField]
        private GameObject effectPrefabParent = null;

        private string currentPlayerName = string.Empty;

        [Tooltip("If true, the system will enable the nav mesh agent for character navigation if a nav mesh exists in the scene")]
        [SerializeField]
        private bool autoDetectNavMeshes = false;

        [SerializeField]
        private bool autoSpawnPlayerOnLevelLoad = false;

        /// <summary>
        /// The invisible gameobject that stores all the player scripts. A reference to an instantiated playerPrefab
        /// </summary>
        private GameObject playerConnectionObject = null;

        private PlayerCharacterSaveData playerCharacterSaveData = null;

        private PlayerUnitMovementController playerUnitMovementController = null;

        private PlayerController playerController = null;

        // The actual movable rendered unit in the game world that we will be moving around
        //private GameObject playerUnitObject = null;

        private bool playerUnitSpawned = false;

        private bool playerConnectionSpawned = false;

        // a reference to the 'main' unit.  This should be the main character when spawned, and null when not spawned
        private UnitController unitController = null;

        // a reference to the active unit.  This could change in cases of both mind control and mounted states
        private UnitController activeUnitController = null;

        // for network mode
        private List<UnitController> activePlayers = new List<UnitController>();

        // track if subscription to target ready should happen
        // only used when loading new level or respawning
        private bool subscribeToTargetReady = false;

        private Coroutine waitForPlayerReadyCoroutine = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemEventManager systemEventManager = null;
        protected UIManager uIManager = null;
        protected LevelManager levelManager = null;
        protected CameraManager cameraManager = null;
        protected SystemAbilityController systemAbilityController = null;
        protected ClassChangeManager classChangeManager = null;

        protected LogManager logManager = null;
        protected CastTargettingManager castTargettingManager = null;
        protected CombatTextManager combatTextManager = null;
        protected InventoryManager inventoryManager = null;
        protected ActionBarManager actionBarManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected ObjectPooler objectPooler = null;
        protected ControlsManager controlsManager = null;
        protected NetworkManagerClient networkManagerClient = null;
        protected CharacterManager characterManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected PlayerManagerServer playerManagerServer = null;

        public GameObject PlayerConnectionObject { get => playerConnectionObject; set => playerConnectionObject = value; }
        public float MaxMovementSpeed { get => maxMovementSpeed; set => maxMovementSpeed = value; }
        public bool PlayerUnitSpawned { get => playerUnitSpawned; }
        public bool PlayerConnectionSpawned { get => playerConnectionSpawned; }
        public GameObject AIUnitParent { get => aiUnitParent; set => aiUnitParent = value; }
        public GameObject EffectPrefabParent { get => effectPrefabParent; set => effectPrefabParent = value; }
        public GameObject PlayerUnitParent { get => playerUnitParent; set => playerUnitParent = value; }
        public LayerMask DefaultGroundMask { get => defaultGroundMask; set => defaultGroundMask = value; }
        public PlayerUnitMovementController PlayerUnitMovementController { get => playerUnitMovementController; set => playerUnitMovementController = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }
        public UnitController ActiveUnitController { get => activeUnitController; }
        public PlayerController PlayerController { get => playerController; set => playerController = value; }
        public PlayerCharacterSaveData PlayerCharacterSaveData { get => playerCharacterSaveData; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            saveManager = systemGameManager.SaveManager;
            systemEventManager = systemGameManager.SystemEventManager;
            uIManager = systemGameManager.UIManager;
            combatTextManager = uIManager.CombatTextManager;
            actionBarManager = uIManager.ActionBarManager;
            messageFeedManager = uIManager.MessageFeedManager;
            levelManager = systemGameManager.LevelManager;
            cameraManager = systemGameManager.CameraManager;
            systemAbilityController = systemGameManager.SystemAbilityController;
            logManager = systemGameManager.LogManager;
            castTargettingManager = systemGameManager.CastTargettingManager;
            inventoryManager = systemGameManager.InventoryManager;
            objectPooler = systemGameManager.ObjectPooler;
            controlsManager = systemGameManager.ControlsManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            characterManager = systemGameManager.CharacterManager;
            systemDataFactory = systemGameManager.SystemDataFactory;

            PerformRequiredPropertyChecks();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            networkManagerServer = systemGameManager.NetworkManagerServer;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void PerformRequiredPropertyChecks() {
            if (aiUnitParent == null) {
                Debug.LogError("PlayerManager.Awake(): the ai unit parent is null.  Please set it in the inspector");
            }
            if (effectPrefabParent == null) {
                Debug.LogError("PlayerManager.Awake(): the effect prefab parent is null.  Please set it in the inspector");
            }
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            systemEventManager.OnLevelChanged += PlayLevelUpEffects;
            SystemEventManager.StartListening("OnPlayerDeath", HandlePlayerDeath);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
            systemEventManager.OnLevelChanged -= PlayLevelUpEffects;
            SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void SetCharacterSaveData(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log("PlayerManager.SetCharacterSaveData()");

            this.playerCharacterSaveData = playerCharacterSaveData;
        }

        /// <summary>
        /// called when network client is stopped on the player unit
        /// </summary>
        public void ProcessStopClient() {
            //Debug.Log("PlayerManager.ProcessStopClient()");
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            DespawnPlayerUnit();
        }


        public void ProcessExitToMainMenu() {
            //Debug.Log("PlayerManager.ProcessExitToMainMenu()");
            DespawnPlayerUnit();
            DespawnPlayerConnection();
            saveManager.ClearSystemManagedCharacterData();
        }

        public void SetPlayerName(string newName) {
            //Debug.Log("PlayerManager.SetPlayerName()");
            if (newName != null && newName != string.Empty) {
                unitController.BaseCharacter.ChangeCharacterName(newName);
            }

            SystemEventManager.TriggerEvent("OnPlayerNameChanged", new EventParamProperties());
            uIManager.PlayerUnitFrameController.SetTarget(UnitController.NamePlateController);
        }

        public void SetPlayerCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.SetPlayerCharacterClass(newCharacterClass.ResourceName);
                return;
            }
            if (newCharacterClass != null) {
                playerManagerServer.SetPlayerCharacterClass(newCharacterClass, 0);
            }
        }

        public void SetPlayerCharacterSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (newClassSpecialization != null) {
                unitController.BaseCharacter.ChangeClassSpecialization(newClassSpecialization);
            }
        }

        public void LearnSkill(Skill skill) {
            //Debug.Log("PlayerManager.SetPlayerCharacterClass(" + characterClassName + ")");
            if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.LearnSkill(skill.ResourceName);
                return;
            }
            if (skill != null) {
                playerManagerServer.LearnSkill(skill, 0);
            }
        }

        public void SetPlayerFaction(Faction newFaction) {
            //Debug.Log("PlayerManager.SetPlayerFaction(" + factionName + ")");
            if (newFaction != null) {
                unitController.BaseCharacter.ChangeCharacterFaction(newFaction);
            }
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("PlayerManager.HandleLevelLoad()");

            SceneNode activeSceneNode = levelManager.GetActiveSceneNode();
            
            if (activeSceneNode == null) {
                if (levelManager.IsMainMenu()) {
                    return;
                }
            }

            if (autoSpawnPlayerOnLevelLoad == false) {
                return;
            }

            //Debug.Log("PlayerManager.OnLevelLoad(): we have a scene node");
            // fix to allow character to spawn after cutscene is viewed on next level load - and another fix to prevent character from spawning on a pure cutscene
            if (activeSceneNode != null) {
                if ((activeSceneNode.AutoPlayCutscene != null && (activeSceneNode.AutoPlayCutscene.Viewed == false || activeSceneNode.AutoPlayCutscene.Repeatable == true))
                    || activeSceneNode.SuppressCharacterSpawn) {
                    //Debug.Log("PlayerManager.OnLevelLoad(): character spawn is suppressed");
                    return;
                }
            }

            // server does not spawn players
            if (systemGameManager.GameMode == GameMode.Network  && networkManagerServer.ServerModeActive == true) {
                return;
            }

            LoadSceneRequest spawnSettings = SpawnPlayerUnit();
            //cameraManager.MainCameraController.SetTargetPositionRaw(spawnLocation, activeUnitController.transform.forward);
            cameraManager.MainCameraController.SetTargetPositionRaw(spawnSettings.spawnLocation, spawnSettings.spawnForwardDirection);
        }

        public void PlayLevelUpEffects(UnitController sourceUnitController, int newLevel) {
            //Debug.Log("PlayerManager.PlayLevelUpEffect()");
            if (systemConfigurationManager.LevelUpEffect == null) {
                return;
            }
            // 0 to allow playing this effect for different reasons than levelup
            if (newLevel == 0 || newLevel != 1) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext();

                systemConfigurationManager.LevelUpEffect.AbilityEffectProperties.Cast(systemAbilityController, sourceUnitController, sourceUnitController, abilityEffectContext);
            }
        }

        public void PlayDeathEffect() {
            //Debug.Log("PlayerManager.PlayDeathEffect()");
            if (PlayerUnitSpawned == false || systemConfigurationManager.DeathEffect == null) {
                return;
            }
            AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
            systemConfigurationManager.DeathEffect.AbilityEffectProperties.Cast(systemAbilityController, unitController, unitController, abilityEffectContext);
        }

        /*
        public void Initialize() {
            //Debug.Log("PlayerManager.Initialize()");
            SpawnPlayerConnection();
            SpawnPlayerUnit();
        }
        */

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            //DespawnPlayerUnit();
            if (playerController != null) {
                playerController.ProcessLevelUnload();
            }
        }

        public void DespawnPlayerUnit() {
            //Debug.Log("PlayerManager.DespawnPlayerUnit()");
            if (!playerUnitSpawned) {
                //Debug.Log("Player Unit is not spawned.  Nothing to despawn.  returning");
                return;
            }
            UnsubscribeFromPlayerInventoryEvents();
            UnsubscribeFromPlayerEvents();
            unitController.Despawn(0f, false, true);
        }

        public void HandlePlayerDeath(string eventName, EventParamProperties eventParam) {
            //Debug.Log("PlayerManager.KillPlayer()");
            PlayDeathEffect();
        }

        public void RespawnPlayer() {
            //Debug.Log("PlayerManager.RespawnPlayer()");
            DespawnPlayerUnit();
            SpawnPlayerUnit();

            if (unitController.CharacterStats.IsAlive == false) {
                unitController.CharacterStats.ReviveComplete();
            }
        }

        public void RevivePlayerUnit() {
            //Debug.Log("PlayerManager.RevivePlayerUnit()");
            unitController.CharacterStats.Revive();
        }

        public void SubscribeToTargetReady() {
            //Debug.Log($"PlayerManager.SubscribeToTargetReady()");

            activeUnitController.OnCameraTargetReady += HandleTargetReady;
            subscribeToTargetReady = false;
        }

        public void UnsubscribeFromTargetReady() {
            if (activeUnitController != null) {
                activeUnitController.OnCameraTargetReady -= HandleTargetReady;
            }
        }

        public void HandleTargetReady() {
            //Debug.Log($"PlayerManager.HandleTargetReady()");

            waitForPlayerReadyCoroutine = StartCoroutine(WaitForPlayerReady());
        }

        private IEnumerator WaitForPlayerReady() {
            //Debug.Log("PlayerManager.WaitForPlayerReady()");
            //private IEnumerator WaitForCamera(int frameNumber) {
            yield return null;
            //Debug.Log($"{gameObject.name}.UnitFrameController.WaitForCamera(): about to render " + namePlateController.Interactable.GetInstanceID() + "; initial frame: " + frameNumber + "; current frame: " + lastWaitFrame);
            //if (lastWaitFrame != frameNumber) {
            if (activeUnitController.IsBuilding() == true) {
                //Debug.Log($"{gameObject.name}.UnitFrameController.WaitForCamera(): a new wait was started. initial frame: " + frameNumber +  "; current wait: " + lastWaitFrame);
            } else {
                //Debug.Log($"{gameObject.name}.UnitFrameController.WaitForCamera(): rendering");
                waitForPlayerReadyCoroutine = null;
                UnsubscribeFromTargetReady();
                cameraManager.ShowPlayers();
            }
        }

        public LoadSceneRequest SpawnPlayerUnit() {
            //Debug.Log("PlayerManager.SpawnPlayerUnit()");

            cameraManager.HidePlayers();
            subscribeToTargetReady = true;
            return SpawnPlayerUnit(networkManagerClient.ClientId);
        }

        public LoadSceneRequest SpawnPlayerUnit(int clientId) {
            //Debug.Log($"PlayerManager.SpawnPlayerUnit({clientId})");

            if (activeUnitController != null) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Player Unit already exists");
                return null;
            }

            // spawn the player unit and set references
            LoadSceneRequest loadSceneRequest = levelManager.GetLoadSceneSettings(clientId);

            if (systemGameManager.GameMode == GameMode.Network && networkManagerClient.ClientMode == NetworkClientMode.Lobby) {
                // load lobby player
                UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(networkManagerClient.LobbyGame.PlayerList[networkManagerClient.ClientId].unitProfileName);
                if (unitProfile == null) {
                    return null;
                }
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
                characterConfigurationRequest.characterName = networkManagerClient.Username;
                CharacterRequestData characterRequestData = new CharacterRequestData(this,
                    systemGameManager.GameMode,
                    characterConfigurationRequest);
                characterManager.SpawnLobbyGamePlayer(networkManagerClient.LobbyGame.gameId, characterRequestData, playerUnitParent.transform, loadSceneRequest.spawnLocation, loadSceneRequest.spawnForwardDirection);
            } else {
                // load MMO or local player
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(systemDataFactory, playerCharacterSaveData.SaveData);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
                characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(playerCharacterSaveData.SaveData);
                CharacterRequestData characterRequestData = new CharacterRequestData(this,
                    systemGameManager.GameMode,
                    characterConfigurationRequest);
                characterManager.SpawnPlayer(playerCharacterSaveData, characterRequestData, playerUnitParent.transform, loadSceneRequest.spawnLocation, loadSceneRequest.spawnForwardDirection);
            }
            return loadSceneRequest;
        }

        private bool OwnPlayer(UnitController unitController, CharacterRequestData characterRequestData) {
            //if (characterRequestData.requestMode == GameMode.Local) {
            //    return true;
            //}

            // network mode, so ask if unitController is owned by us
            //return networkManager.OwnPlayer(unitController);

            // testing - for now this can always return true because we will not perform configuration on things we didn't request anyway
            //return true;
            return characterManager.HasUnitSpawnRequest(characterRequestData.spawnRequestId);
        }

        public void ConfigureSpawnedCharacter(UnitController unitController, CharacterRequestData characterRequestData) {
            //Debug.Log($"PlayerManager.ConfigureSpawnedCharacter({unitController.gameObject.name})");

            //if (OwnPlayer(unitController, characterRequestData) == true) {
                //SetUnitController(unitController);
            //}

            if (levelManager.NavMeshAvailable == true && autoDetectNavMeshes) {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Enabling NavMeshAgent()");
                unitController.EnableAgent();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.useMeshNav = true;
                }
            } else {
                //Debug.Log("PlayerManager.SpawnPlayerUnit(): Disabling NavMeshAgent()");
                unitController.DisableAgent();
                if (playerUnitMovementController != null) {
                    playerUnitMovementController.useMeshNav = false;
                }
            }

            //unitController.UnitModelController.SetInitialSavedAppearance(playerCharacterSaveData.SaveData);
            if (subscribeToTargetReady) {
                SubscribeToTargetReady();
            }
        }

        public void PostInit(UnitController unitController, CharacterRequestData characterRequestData) {
            //Debug.Log($"PlayerManager.PostInit({unitController.gameObject.name})");

            if (unitController.UnitModelController.ModelCreated == false) {
                // do UMA spawn stuff to wait for UMA to spawn
                SubscribeToModelReady();
            } else {
                // handle spawn immediately since this is a non UMA unit and waiting should not be necessary
                HandlePlayerUnitSpawn();
            }

            if (systemGameManager.GameMode == GameMode.Local || networkManagerClient.ClientMode == NetworkClientMode.MMO) {
                // load player data from saveManager
                saveManager.LoadSaveDataToCharacter(playerCharacterSaveData.SaveData);
            }

            //SubscribeToPlayerInventoryEvents();
            //SubscribeToPlayerEvents();


            if (PlayerPrefs.HasKey("ShowNewPlayerHints") == false) {
                if (controlsManager.GamePadModeActive == true) {
                    uIManager.gamepadHintWindow.OpenWindow();
                } else {
                    uIManager.keyboardHintWindow.OpenWindow();
                }
            }
        }

        public void SetActiveUnitController(UnitController unitController) {
            //Debug.Log("PlayerManager.SetActiveUnitController(" + unitController.gameObject.name + ")");
            activeUnitController = unitController;

            // this should not be needed, baseCharacter should always point to the proper unit
            //activeCharacter.SetUnitController(activeUnitController);
        }

        public void SetUnitController(UnitController unitController) {
            Debug.Log("PlayerManager.SetUnitController(" + (unitController == null ? "null" : unitController.gameObject.name) + ")");

            this.unitController = unitController;
            activeUnitController = unitController;

            if (unitController == null) {
                playerManagerServer.RemoveActivePlayer(0);
                playerUnitSpawned = false;
                return;
            }
            SubscribeToPlayerEvents();
            SubscribeToPlayerInventoryEvents();
            unitController.CharacterUnit.SetCharacterStatsCapabilities();
            //playerManagerServer.AddActivePlayer(0, unitController);
        }

        public void HandleModelReady() {
            Debug.Log("PlayerManager.HandleModelReady()");
            UnsubscribeFromModelReady();

            HandlePlayerUnitSpawn();
        }

        private void HandlePlayerUnitSpawn() {
            //Debug.Log("PlayerManager.HandlePlayerUnitSpawn()");
            playerUnitSpawned = true;

            // inform any subscribers that we just spawned a player unit
            systemEventManager.NotifyOnPlayerUnitSpawn(unitController);

            playerController.SubscribeToUnitEvents();

            if (systemConfigurationManager.UseThirdPartyMovementControl == false) {
                playerUnitMovementController.Init();
            } else {
                DisableMovementControllers();
            }
        }

        public void DisableMovementControllers() {
            Debug.Log("PlayerManager.DisableMovementControllers()");
            playerUnitMovementController.enabled = false;
            playerUnitMovementController.MovementStateController.enabled = false;
        }

        public void EnableMovementControllers() {
            Debug.Log("PlayerManager.EnableMovementControllers()");
            playerUnitMovementController.enabled = true;
            playerUnitMovementController.MovementStateController.enabled = true;
            playerUnitMovementController.Init();
        }

        public void SubscribeToModelReady() {
            Debug.Log("PlayerManager.SubscribeToModelReady()");

            //activeUnitController.UnitModelController.OnModelUpdated += HandleModelReady;
            activeUnitController.UnitModelController.OnModelCreated += HandleModelReady;
        }

        public void UnsubscribeFromModelReady() {
            Debug.Log("PlayerManager.UnsubscribeFromModelReady()");

            //activeUnitController.UnitModelController.OnModelUpdated -= HandleModelReady;
            activeUnitController.UnitModelController.OnModelCreated -= HandleModelReady;
        }

        public void SpawnPlayerConnection(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");

            SetCharacterSaveData(playerCharacterSaveData);

            SpawnPlayerConnectionObject();

        }

        public void SpawnPlayerConnection() {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");

            SpawnPlayerConnectionObject();

        }

        public void SpawnPlayerConnectionObject() {
            //Debug.Log("PlayerManager.SpawnPlayerConnection()");

            if (playerConnectionObject != null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is not null.  exiting.");
                return;
            }

            playerConnectionObject = objectPooler.GetPooledObject(playerConnectionPrefab, playerConnectionParent.transform);
            playerController = playerConnectionObject.GetComponent<PlayerController>();
            playerController.Configure(systemGameManager);
            playerUnitMovementController = playerConnectionObject.GetComponent<PlayerUnitMovementController>();
            playerUnitMovementController.Configure(systemGameManager);

            SystemEventManager.TriggerEvent("OnBeforePlayerConnectionSpawn", new EventParamProperties());
            playerConnectionSpawned = true;
            SystemEventManager.TriggerEvent("OnPlayerConnectionSpawn", new EventParamProperties());

            // this goes here so action bars can get abilities on them when the player is initialized
            //SubscribeToPlayerEvents();
        }

        public void DespawnPlayerConnection() {
            if (playerConnectionObject == null) {
                //Debug.Log("PlayerManager.SpawnPlayerConnection(): The Player Connection is null.  exiting.");
                return;
            }
            SystemEventManager.TriggerEvent("OnPlayerConnectionDespawn", new EventParamProperties());
            objectPooler.ReturnObjectToPool(playerConnectionObject);
            playerConnectionObject = null;
            playerUnitMovementController = null;
            playerConnectionSpawned = false;
        }

        public void SubscribeToPlayerInventoryEvents() {
            Debug.Log("PlayerManager.SubscribeToPlayerInventoryEvents()");

            unitController.CharacterInventoryManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
            unitController.CharacterInventoryManager.OnAddBankBagNode += HandleAddBankBagNode;
            unitController.CharacterInventoryManager.OnAddInventorySlot += HandleAddInventorySlot;
            unitController.CharacterInventoryManager.OnAddBankSlot += HandleAddBankSlot;
            unitController.CharacterInventoryManager.OnRemoveInventorySlot += HandleRemoveInventorySlot;
            unitController.CharacterInventoryManager.OnRemoveBankSlot += HandleRemoveBankSlot;
        }

        public void UnsubscribeFromPlayerInventoryEvents() {
            unitController.CharacterInventoryManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
            unitController.CharacterInventoryManager.OnAddBankBagNode -= HandleAddBankBagNode;
            unitController.CharacterInventoryManager.OnAddInventorySlot -= HandleAddInventorySlot;
            unitController.CharacterInventoryManager.OnAddBankSlot -= HandleAddBankSlot;
            unitController.CharacterInventoryManager.OnRemoveInventorySlot -= HandleRemoveInventorySlot;
            unitController.CharacterInventoryManager.OnRemoveBankSlot -= HandleRemoveBankSlot;
        }

        public void SubscribeToPlayerEvents() {
            Debug.Log("PlayerManager.SubscribeToPlayerEvents()");

            unitController.UnitEventController.OnImmuneToEffect += HandleImmuneToEffect;
            unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
            //unitController.UnitEventController.OnAfterDie += HandleAfterDie;
            unitController.UnitEventController.OnReviveBegin += HandleReviveBegin;
            unitController.UnitEventController.OnReviveComplete += HandleReviveComplete;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnGainXP += HandleGainXP;
            unitController.UnitEventController.OnStatusEffectAdd += HandleStatusEffectAdd;
            unitController.UnitEventController.OnRecoverResource += HandleRecoverResource;
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitController.UnitEventController.OnCalculateRunSpeed += HandleCalculateRunSpeed;
            unitController.UnitEventController.OnEnterCombat += HandleEnterCombat;
            unitController.UnitEventController.OnDropCombat += HandleDropCombat;
            unitController.UnitEventController.OnCombatUpdate += HandleCombatUpdate;
            unitController.UnitEventController.OnReceiveCombatMiss += HandleCombatMiss;
            unitController.UnitEventController.OnEquipmentChanged += HandleEquipmentChanged;
            unitController.UnitEventController.OnUnlearnAbilities += HandleUnlearnClassAbilities;
            unitController.UnitEventController.OnLearnedCheckFail += HandleLearnedCheckFail;
            unitController.UnitEventController.OnPowerResourceCheckFail += HandlePowerResourceCheckFail;
            unitController.UnitEventController.OnCombatCheckFail += HandleCombatCheckFail;
            unitController.UnitEventController.OnStealthCheckFail += HandleStealthCheckFail;
            unitController.UnitEventController.OnAbilityActionCheckFail += HandleAbilityActionCheckFail;
            unitController.UnitEventController.OnPerformAbility += HandlePerformAbility;
            unitController.UnitEventController.OnBeginAbilityCoolDown += HandleBeginAbilityCoolDown;
            //unitController.UnitEventController.OnTargetInAbilityRangeFail += HandleTargetInAbilityRangeFail;
            unitController.UnitEventController.OnReputationChange += HandleReputationChange;
            unitController.UnitEventController.OnUnlearnAbility += HandleUnlearnAbility;
            unitController.UnitEventController.OnLearnAbility += HandleLearnAbility;
            unitController.UnitEventController.OnActivateTargetingMode += HandleActivateTargetingMode;
            unitController.UnitEventController.OnCombatMessage += HandleCombatMessage;
            unitController.UnitEventController.OnMessageFeedMessage += HandleMessageFeedMessage;
            unitController.UnitEventController.OnEnterInteractableRange += HandleEnterInteractableRange;
            unitController.UnitEventController.OnExitInteractableRange += HandleExitInteractableRange;
            unitController.UnitEventController.OnAcceptQuest += HandleAcceptQuest;
            unitController.UnitEventController.OnAbandonQuest += HandleRemoveQuest;
            unitController.UnitEventController.OnTurnInQuest += HandleRemoveQuest;
            unitController.UnitEventController.OnMarkQuestComplete += HandleMarkQuestComplete;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated += HandleQuestObjectiveStatusUpdated;
            unitController.UnitEventController.OnLearnSkill += HandleLearnSkill;
            unitController.UnitEventController.OnUnLearnSkill += HandleUnLearnSkill;
            unitController.UnitEventController.OnStartInteractWithOption += HandleStartInteractWithOption;
        }

        public void UnsubscribeFromPlayerEvents() {
            Debug.Log("PlayerManager.UnsubscribeFromPlayerEvents()");

            unitController.UnitEventController.OnImmuneToEffect -= HandleImmuneToEffect;
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDie;
            //unitController.UnitEventController.OnAfterDie -= HandleAfterDie;
            unitController.UnitEventController.OnReviveBegin -= HandleReviveBegin;
            unitController.UnitEventController.OnReviveComplete -= HandleReviveComplete;
            unitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            unitController.UnitEventController.OnGainXP -= HandleGainXP;
            unitController.UnitEventController.OnStatusEffectAdd -= HandleStatusEffectAdd;
            unitController.UnitEventController.OnRecoverResource -= HandleRecoverResource;
            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChanged;
            unitController.UnitEventController.OnCalculateRunSpeed -= HandleCalculateRunSpeed;
            unitController.UnitEventController.OnEnterCombat -= HandleEnterCombat;
            unitController.UnitEventController.OnDropCombat -= HandleDropCombat;
            unitController.UnitEventController.OnCombatUpdate -= HandleCombatUpdate;
            unitController.UnitEventController.OnReceiveCombatMiss -= HandleCombatMiss;
            unitController.UnitEventController.OnEquipmentChanged -= HandleEquipmentChanged;
            unitController.UnitEventController.OnUnlearnAbilities -= HandleUnlearnClassAbilities;
            unitController.UnitEventController.OnLearnedCheckFail -= HandleLearnedCheckFail;
            unitController.UnitEventController.OnPowerResourceCheckFail -= HandlePowerResourceCheckFail;
            unitController.UnitEventController.OnCombatCheckFail -= HandleCombatCheckFail;
            unitController.UnitEventController.OnStealthCheckFail -= HandleStealthCheckFail;
            unitController.UnitEventController.OnAbilityActionCheckFail -= HandleAbilityActionCheckFail;
            unitController.UnitEventController.OnPerformAbility -= HandlePerformAbility;
            unitController.UnitEventController.OnBeginAbilityCoolDown -= HandleBeginAbilityCoolDown;
            //unitController.UnitEventController.OnTargetInAbilityRangeFail -= HandleTargetInAbilityRangeFail;
            unitController.UnitEventController.OnReputationChange -= HandleReputationChange;
            unitController.UnitEventController.OnUnlearnAbility -= HandleUnlearnAbility;
            unitController.UnitEventController.OnLearnAbility -= HandleLearnAbility;
            unitController.UnitEventController.OnActivateTargetingMode -= HandleActivateTargetingMode;
            unitController.UnitEventController.OnCombatMessage -= HandleCombatMessage;
            unitController.UnitEventController.OnMessageFeedMessage -= HandleMessageFeedMessage;
            unitController.UnitEventController.OnEnterInteractableRange -= HandleEnterInteractableRange;
            unitController.UnitEventController.OnExitInteractableRange -= HandleExitInteractableRange;
            unitController.UnitEventController.OnAcceptQuest -= HandleAcceptQuest;
            unitController.UnitEventController.OnAbandonQuest -= HandleRemoveQuest;
            unitController.UnitEventController.OnTurnInQuest -= HandleRemoveQuest;
            unitController.UnitEventController.OnMarkQuestComplete -= HandleMarkQuestComplete;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated -= HandleQuestObjectiveStatusUpdated;
            unitController.UnitEventController.OnLearnSkill -= HandleLearnSkill;
            unitController.UnitEventController.OnUnLearnSkill -= HandleUnLearnSkill;
            unitController.UnitEventController.OnStartInteractWithOption -= HandleStartInteractWithOption;
        }

        public void HandleStartInteractWithOption(UnitController sourceUnitController, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            interactableOptionComponent.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
        }

        public void HandleLearnSkill(UnitController sourceUnitController, Skill skill) {
            systemEventManager.NotifyOnLearnSkill(unitController, skill);
        }

        public void HandleUnLearnSkill(UnitController sourceUnitController, Skill skill) {
            systemEventManager.NotifyOnUnLearnSkill(unitController, skill);
        }

        public void HandleQuestObjectiveStatusUpdated(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnQuestObjectiveStatusUpdated(sourceUnitController, questBase);
        }

        public void HandleMarkQuestComplete(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnMarkQuestComplete(sourceUnitController, questBase);
        }

        public void HandleRemoveQuest(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnRemoveQuest(sourceUnitController, questBase);
        }

        public void HandleAcceptQuest(UnitController sourceUnitController, QuestBase questBase) {
            systemEventManager.NotifyOnAcceptQuest(sourceUnitController, questBase);
        }

        public void HandleEnterInteractableRange(UnitController controller, Interactable interactable) {
            playerController.AddInteractable(interactable);
        }

        public void HandleExitInteractableRange(UnitController controller, Interactable interactable) {
            playerController.RemoveInteractable(interactable);
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            inventoryManager.AddInventoryBagNode(bagNode);
        }

        public void HandleAddBankBagNode(BagNode bagNode) {
            inventoryManager.AddBankBagNode(bagNode);
        }

        public void HandleAddInventorySlot(InventorySlot inventorySlot) {
            Debug.Log("PlayerManager.HandleAddInventorySlot()");

            inventoryManager.AddInventorySlot(inventorySlot);
        }

        public void HandleAddBankSlot(InventorySlot inventorySlot) {
            inventoryManager.AddBankSlot(inventorySlot);
        }

        public void HandleRemoveInventorySlot(InventorySlot inventorySlot) {
            inventoryManager.RemoveInventorySlot(inventorySlot);
        }

        public void HandleRemoveBankSlot(InventorySlot inventorySlot) {
            inventoryManager.RemoveBankSlot(inventorySlot);
        }

        public void HandleBeginAbilityCoolDown() {
            SystemEventManager.TriggerEvent("OnBeginAbilityCooldown", new EventParamProperties());
        }

        public void HandleCombatMessage(string messageText) {
            logManager.WriteCombatMessage(messageText);
        }

        public void HandleMessageFeedMessage(string messageText) {
            messageFeedManager.WriteMessage(messageText);
        }

        public void HandleCombatMiss(Interactable targetObject, AbilityEffectContext abilityEffectContext) {
            combatTextManager.SpawnCombatText(targetObject, 0, CombatTextType.miss, CombatMagnitude.normal, abilityEffectContext);
        }

        public void HandleActivateTargetingMode(AbilityProperties baseAbility) {
            castTargettingManager.EnableProjector(baseAbility);
        }

        public void HandleAbilityActionCheckFail(AbilityProperties baseAbility) {
            if (PlayerUnitSpawned == true && logManager != null) {
                logManager.WriteCombatMessage("Cannot use " + (baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName) + ". Waiting for another ability to finish.");
            }
        }

        public void HandleLearnAbility(UnitController sourceUnitController, AbilityProperties baseAbility) {
            //Debug.Log($"PlayerManager.HandleLearnAbility({baseAbility.ResourceName})");

            systemEventManager.NotifyOnAbilityListChanged(sourceUnitController, baseAbility);
            baseAbility.NotifyOnLearn(unitController);
        }

        public void HandleUnlearnAbility(bool updateActionBars) {
            if (updateActionBars) {
                actionBarManager.RemoveStaleActions();
            }
        }

        public void HandleCombatUpdate() {
            activeUnitController.CharacterCombat.HandleAutoAttack();
        }

        public void HandleDropCombat() {
            //Debug.Log("PlayerManager.HandleDropCombat()");

            if (logManager != null) {
                logManager.WriteCombatMessage("Left combat");
            }
        }

        public void HandleEnterCombat(Interactable interactable) {
            if (logManager != null) {
                logManager.WriteCombatMessage("Entered combat with " + interactable.DisplayName);
            }
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            //Debug.Log("PlayerManager.HandleReputationChange");

            systemEventManager.NotifyOnReputationChange(sourceUnitController);
        }

        public void HandleTargetInAbilityRangeFail(AbilityProperties baseAbility, Interactable target) {
            if (baseAbility != null && logManager != null) {
                logManager.WriteCombatMessage(target.name + " is out of range of " + (baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName));
            }
        }

        public void HandlePerformAbility(AbilityProperties ability) {
            systemEventManager.NotifyOnAbilityUsed(unitController, ability);
            ability.NotifyOnAbilityUsed(unitController);
        }

        public void HandleCombatCheckFail(AbilityProperties ability) {
            logManager.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while out of combat");
        }

        public void HandleStealthCheckFail(AbilityProperties ability) {
            logManager.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while while stealthed");
        }

        public void HandlePowerResourceCheckFail(AbilityProperties ability, IAbilityCaster abilityCaster) {
            logManager.WriteCombatMessage("Not enough " + ability.PowerResource.DisplayName + " to perform " + ability.DisplayName + " at a cost of " + ability.GetResourceCost(abilityCaster));
        }

        public void HandleLearnedCheckFail(AbilityProperties ability) {
            logManager.WriteCombatMessage("You have not learned the ability " + ability.DisplayName + " yet");
        }

        public void HandleUnlearnClassAbilities() {
            //Debug.Log("PlayerManager.HandleUnlearnClassAbilities()");
            // now perform a single action bar update
            actionBarManager.RemoveStaleActions();
        }

        public void HandleEquipmentChanged(InstantiatedEquipment newItem, InstantiatedEquipment oldItem, int slotIndex) {
            if (PlayerUnitSpawned) {
                if (slotIndex != -1) {
                    unitController.CharacterInventoryManager.AddInventoryItem(oldItem, slotIndex);
                } else if (oldItem != null) {
                    unitController.CharacterInventoryManager.AddItem(oldItem, false);
                }
            }
            systemEventManager.NotifyOnEquipmentChanged(newItem, oldItem);
        }

        /// <summary>
        /// trigger events with new speed information, mostly for third party controllers to pick up the new values
        /// </summary>
        /// <param name="oldRunSpeed"></param>
        /// <param name="currentRunSpeed"></param>
        /// <param name="oldSprintSpeed"></param>
        /// <param name="currentSprintSpeed"></param>
        public void HandleCalculateRunSpeed(float oldRunSpeed, float currentRunSpeed, float oldSprintSpeed, float currentSprintSpeed) {
            if (currentRunSpeed != oldRunSpeed) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = currentRunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);
                eventParam.simpleParams.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
            if (currentSprintSpeed != oldSprintSpeed) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
        }

        public void HandleRecoverResource(PowerResource powerResource, int amount) {
            if (logManager != null) {
                logManager.WriteCombatMessage("You gain " + amount + " " + powerResource.DisplayName);
            }
        }

        public void HandleResourceAmountChanged(PowerResource powerResource, int amount, int amount2) {
            actionBarManager.UpdateVisuals();
        }

        public void HandleStatusEffectAdd(StatusEffectNode statusEffectNode) {
            if (statusEffectNode == null) {
                return;
            }

            if (statusEffectNode.StatusEffect.ClassTrait == false && activeUnitController != null) {
                if (statusEffectNode.AbilityEffectContext.savedEffect == false) {
                    if (activeUnitController.CharacterUnit != null) {
                        combatTextManager.SpawnCombatText(activeUnitController, statusEffectNode.StatusEffect, true);
                    }
                }
            }

            statusEffectNode.StatusEffect.NotifyOnApply(unitController);
        }

        public void HandleGainXP(UnitController unitController, int gainedXP, int currentXP) {
            if (logManager != null) {
                logManager.WriteSystemMessage("You gain " + gainedXP + " experience");
            }
            if (activeUnitController != null) {
                if (combatTextManager != null) {
                    combatTextManager.SpawnCombatText(activeUnitController, gainedXP, CombatTextType.gainXP, CombatMagnitude.normal, null);
                }
            }
            SystemEventManager.TriggerEvent("OnXPGained", new EventParamProperties());
        }

        public void HandleLevelChanged(int newLevel) {
            systemEventManager.NotifyOnLevelChanged(unitController, newLevel);
            messageFeedManager.WriteMessage(string.Format("YOU HAVE REACHED LEVEL {0}!", newLevel.ToString()));
        }

        public void HandleReviveComplete(UnitController sourceUnitController) {
            SystemEventManager.TriggerEvent("OnReviveComplete", new EventParamProperties());
            if (activeUnitController != null) {
                activeUnitController.UnitAnimator.SetCorrectOverrideController();
            }
        }

        public void HandleReviveBegin() {
            playerController.HandleReviveBegin();
        }

        public void HandleBeforeDie(UnitController deadUnitController) {
            playerController.HandleDie();
            uIManager.PlayerDeathHandler(deadUnitController);
            SystemEventManager.TriggerEvent("OnPlayerDeath", new EventParamProperties());
        }

        public void HandleAfterDie(CharacterStats deadCharacterStats) {
        }

        public void HandleImmuneToEffect(AbilityEffectContext abilityEffectContext) {
            combatTextManager.SpawnCombatText(activeUnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
        }

    }

}
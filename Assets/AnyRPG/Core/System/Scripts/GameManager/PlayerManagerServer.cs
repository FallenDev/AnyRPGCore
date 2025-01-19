using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PlayerManagerServer : ConfiguredMonoBehaviour {

        // clientId, UnitController
        private Dictionary<int, UnitController> activePlayers = new Dictionary<int, UnitController>();
        private Dictionary<UnitController, int> activePlayerLookup = new Dictionary<UnitController, int>();

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemItemManager systemItemManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManager levelManager = null;
        protected InteractionManager interactionManager = null;

        public Dictionary<int, UnitController> ActivePlayers { get => activePlayers; }
        public Dictionary<UnitController, int> ActivePlayerLookup { get => activePlayerLookup; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
            systemItemManager = systemGameManager.SystemItemManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManager = systemGameManager.LevelManager;
            interactionManager = systemGameManager.InteractionManager;
        }


        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            /*
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
            systemEventManager.OnLevelChanged -= PlayLevelUpEffects;
            SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            */
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void AddActivePlayer(int clientId, UnitController unitController) {
            Debug.Log($"PlayerManagerServer.AddActivePlayer({clientId})");

            activePlayers.Add(clientId, unitController);
            activePlayerLookup.Add(unitController, clientId);
        }

        public void MonitorPlayer(UnitController unitController) {
            if (activePlayerLookup.ContainsKey(unitController) == false) {
                return;
            }
            SubscribeToPlayerEvents(unitController);
        }

        public void RemoveActivePlayer(int clientId) {
            if (ActivePlayers.ContainsKey(clientId) == false) {
                return;
            }
            UnsubscribeFromPlayerEvents(activePlayers[clientId]);
            activePlayerLookup.Remove(activePlayers[clientId]);
            activePlayers.Remove(clientId);
        }

        public void SubscribeToPlayerEvents(UnitController unitController) {
            Debug.Log($"PlayerManagerServer.SubscribeToPlayerEvents({unitController.gameObject.name})");

            unitController.UnitEventController.OnKillEvent += HandleKillEvent;
            unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTrigger;

        }

        public void UnsubscribeFromPlayerEvents(UnitController unitController) {
            Debug.Log($"PlayerManagerServer.UnsubscribeFromPlayerEvents({unitController.gameObject.name})");

            unitController.UnitEventController.OnKillEvent -= HandleKillEvent;
            unitController.UnitEventController.OnEnterInteractableTrigger -= HandleEnterInteractableTrigger;
        }

        private void HandleEnterInteractableTrigger(UnitController unitController, Interactable interactable) {
            Debug.Log($"PlayerManagerServer.HandleEnterInteractableTrigger({unitController.gameObject.name})");

            if (networkManagerServer.ServerModeActive || systemGameManager.GameMode == GameMode.Local) {
                interactionManager.InteractWithTrigger(unitController, interactable);
            }
        }

        public void HandleKillEvent(UnitController unitController, UnitController killedUnitController, float creditPercent) {
            if (creditPercent == 0) {
                return;
            }
            //Debug.Log($"{gameObject.name}: About to gain xp from kill with creditPercent: " + creditPercent);
            GainXP(unitController, (int)(LevelEquations.GetXPAmountForKill(unitController.CharacterStats.Level, killedUnitController, systemConfigurationManager) * creditPercent));
        }

        public void GainXP(int amount, int clientId) {
            if (activePlayers.ContainsKey(clientId) == true) {
                GainXP(activePlayers[clientId], amount);
            }
        }

        public void GainXP(UnitController unitController, int amount) {
            unitController.CharacterStats.GainXP(amount);
        }

        public void AddCurrency(Currency currency, int amount, int clientId) {
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }
            activePlayers[clientId].CharacterCurrencyManager.AddCurrency(currency, amount);

        }

        public void AddItem(string itemName, int clientId) {
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }

            Item tmpItem = systemItemManager.GetNewResource(itemName);
            if (tmpItem != null) {
                activePlayers[clientId].CharacterInventoryManager.AddItem(tmpItem, false);
            }
        }

        public void BeginAction(AnimatedAction animatedAction, int clientId) {
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }
            activePlayers[clientId].UnitActionManager.BeginAction(animatedAction);

        }

        public void LearnAbility(string abilityName, int clientId) {
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }
            Ability tmpAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (tmpAbility != null) {
                activePlayers[clientId].CharacterAbilityManager.LearnAbility(tmpAbility.AbilityProperties);
            }

        }

        public void SetLevel(int newLevel, int clientId) {
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }
            CharacterStats characterStats = activePlayers[clientId].CharacterStats;
            newLevel = Mathf.Clamp(newLevel, characterStats.Level, systemConfigurationManager.MaxLevel);
            if (newLevel > characterStats.Level) {
                while (characterStats.Level < newLevel) {
                    characterStats.GainLevel();
                }
            }
        }

        public void LoadScene(string sceneName, CharacterUnit characterUnit) {
            Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {characterUnit.UnitController.gameObject.name})");

            if (activePlayerLookup.ContainsKey(characterUnit.UnitController)) {
                LoadScene(sceneName, activePlayerLookup[characterUnit.UnitController]);
            }
        }

        public void LoadScene(string sceneName, int clientId) {
            Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {clientId})");
            
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }
            if (systemGameManager.GameMode == GameMode.Local) {
                levelManager.LoadLevel(sceneName);
            } else if (networkManagerServer.ServerModeActive) {
                networkManagerServer.AdvertiseLoadScene(sceneName, clientId);
            }
        }

        public void Teleport(UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            StartCoroutine(TeleportDelay(unitController, teleportEffectProperties));
        }


        // delay the teleport by one frame so abilities can finish up without the source character becoming null
        public IEnumerator TeleportDelay (UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            yield return null;
            if (unitController != null) {
                TeleportInternal(unitController, teleportEffectProperties);
            }
        }

        private void TeleportInternal(UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            if (networkManagerServer.ServerModeActive == true) {
                if (activePlayerLookup.ContainsKey(unitController) == false) {
                    return;
                }
                networkManagerServer.AdvertiseTeleport(activePlayerLookup[unitController], teleportEffectProperties);
                return;
            }

            // local mode active, continue with teleport
            if (teleportEffectProperties.levelName != null) {
                if (teleportEffectProperties.overrideSpawnDirection == true) {
                    levelManager.SetSpawnRotationOverride(teleportEffectProperties.spawnForwardDirection);
                }
                if (teleportEffectProperties.overrideSpawnLocation == true) {
                    levelManager.LoadLevel(teleportEffectProperties.levelName, teleportEffectProperties.spawnLocation);
                } else {
                    if (teleportEffectProperties.locationTag != null && teleportEffectProperties.locationTag != string.Empty) {
                        levelManager.OverrideSpawnLocationTag = teleportEffectProperties.locationTag;
                    }
                    levelManager.LoadLevel(teleportEffectProperties.levelName);
                }
            }
        }

        public void DespawnPlayerUnit(int clientId) {
            if (activePlayers.ContainsKey(clientId) == false) {
                return;
            }
            activePlayers[clientId].Despawn(0, false, true);
            RemoveActivePlayer(clientId);
        }
    }

}
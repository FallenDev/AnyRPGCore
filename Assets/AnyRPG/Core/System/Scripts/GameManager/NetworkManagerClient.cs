using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class NetworkManagerClient : ConfiguredMonoBehaviour {

        public event Action<string> OnClientVersionFailure = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };
        public event Action<string> OnSendLobbyChatMessage = delegate { };
        public event Action<string, int> OnSendLobbyGameChatMessage = delegate { };
        public event Action<string, int> OnSendSceneChatMessage = delegate { };
        public event Action<int, string> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };
        public event Action<List<LobbyGame>> OnSetLobbyGameList = delegate { };
        public event Action<Dictionary<int, string>> OnSetLobbyPlayerList = delegate { };
        public event Action<int, int, string> OnChooseLobbyGameCharacter = delegate { };
        public event Action<int, int, bool> OnSetLobbyGameReadyStatus = delegate { };


        private string username = string.Empty;
        private string password = string.Empty;
        
        private bool isLoggingInOrOut = false;

        private NetworkClientMode clientMode = NetworkClientMode.Lobby;
        private int clientId;
        private LobbyGame lobbyGame;

        [SerializeField]
        private NetworkController networkController = null;

        private Dictionary<int, LoggedInAccount> lobbyGamePlayerList = new Dictionary<int, LoggedInAccount>();
        
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();
        private Dictionary<int, string> lobbyPlayers = new Dictionary<int, string>();

        // game manager references
        private PlayerManager playerManager = null;
        private CharacterManager characterManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;
        private LogManager logManager = null;
        private InteractionManager interactionManager = null;
        private MessageFeedManager messageFeedManager = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;
        private CraftingManager craftingManager = null;

        public string Username { get => username; }
        public string Password { get => password; }
        public NetworkClientMode ClientMode { get => clientMode; set => clientMode = value; }
        public Dictionary<int, LoggedInAccount> LobbyGamePlayerList { get => lobbyGamePlayerList; }
        public LobbyGame LobbyGame { get => lobbyGame; }
        public int ClientId { get => clientId; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
            logManager = systemGameManager.LogManager;
            interactionManager = systemGameManager.InteractionManager;
            messageFeedManager = uIManager.MessageFeedManager;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
            craftingManager = systemGameManager.CraftingManager;
        }

        public bool Login(string username, string password, string server) {
            //Debug.Log($"NetworkManagerClient.Login({username}, {password})");
            
            isLoggingInOrOut = true;

            this.username = username;
            this.password = password;
            return networkController.Login(username, password, server);
        }

        public void Logout() {
            isLoggingInOrOut = true;
            networkController.Logout();
        }

        public void LoadScene(string sceneName) {
            //Debug.Log($"NetworkManagerClient.LoadScene({sceneName})");

            networkController.LoadScene(sceneName);
        }

        public void SpawnPlayer(int playerCharacterId, CharacterRequestData characterRequestData, Transform parentTransform) {
            //Debug.Log($"NetworkManagerClient.SpawnPlayer({playerCharacterId})");

            if (characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.NetworkUnitPrefab == null) {
                Debug.LogWarning($"NetworkManagerClient.SpawnPlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}) On UnitProfile Network Unit Prefab is null ");
            }
            networkController.SpawnPlayer(playerCharacterId, characterRequestData, parentTransform);
        }

        public void SpawnLobbyGamePlayer(int gameId, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerClient.SpawnPlayer({playerCharacterId})");

            if (characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.NetworkUnitPrefab == null) {
                Debug.LogWarning($"NetworkManagerClient.SpawnPlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}) On UnitProfile Network Unit Prefab is null ");
            }
            networkController.SpawnLobbyGamePlayer(gameId, characterRequestData, parentTransform, position, forward);
        }

        public GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            return networkController.SpawnModelPrefab(spawnRequestId, prefab, parentTransform, position, forward);
        }

        public void SendLobbyChatMessage(string messageText) {
            networkController.SendLobbyChatMessage(messageText);
        }

        public void SendLobbyGameChatMessage(string messageText, int gameId) {
            networkController.SendLobbyGameChatMessage(messageText, gameId);
        }

        public void SendSceneChatMessage(string chatMessage) {
            networkController.SendSceneChatMessage(chatMessage);
        }

        public void RequestLobbyPlayerList() {
            networkController.RequestLobbyPlayerList();
        }

        public void ToggleLobbyGameReadyStatus(int gameId) {
            networkController.ToggleLobbyGameReadyStatus(gameId);
        }

        public bool CanSpawnPlayerOverNetwork() {
            return networkController.CanSpawnCharacterOverNetwork();
        }

        public bool OwnPlayer(UnitController unitController) {
            return networkController.OwnPlayer(unitController);
        }

        public void ProcessStopNetworkUnitClient(UnitController unitController) {
            if (playerManager.UnitController == unitController) {
                playerManager.ProcessStopClient();
            } else {
                characterManager.ProcessStopNetworkUnit(unitController);
            }
        }

        public void ProcessStopConnection() {
            Debug.Log($"NetworkManagerClient.ProcessStopConnection()");
            systemGameManager.SetGameMode(GameMode.Local);
            if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                if (isLoggingInOrOut == false) {
                    uIManager.AddPopupWindowToQueue(uIManager.disconnectedWindow);
                }
                isLoggingInOrOut = false;
                levelManager.LoadMainMenu();
                return;
            }

            // don't open disconnected window if this was an expected logout;
            if (isLoggingInOrOut == true) {
                isLoggingInOrOut = false;
                return;
            }
            
            // main menu, close main menu windows and open the disconnected window
            uIManager.newGameWindow.CloseWindow();
            uIManager.loadGameWindow.CloseWindow();
            uIManager.clientLobbyWindow.CloseWindow();
            uIManager.clientLobbyGameWindow.CloseWindow();
            uIManager.createLobbyGameWindow.CloseWindow();
            uIManager.disconnectedWindow.OpenWindow();
        }

        public void ProcessClientVersionFailure(string requiredClientVersion) {
            Debug.Log($"NetworkManagerClient.ProcessClientVersionFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.wrongClientVersionWindow.OpenWindow();
            OnClientVersionFailure(requiredClientVersion);
        }

        public void ProcessAuthenticationFailure() {
            Debug.Log($"NetworkManagerClient.ProcessAuthenticationFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.loginFailedWindow.OpenWindow();
        }

        public void ProcessLoginSuccess() {
            //Debug.Log($"NetworkManagerClient.ProcessLoginSuccess()");

            // not doing this here because the connector has not spawned yet.
            //uIManager.ProcessLoginSuccess();

            isLoggingInOrOut = false;
        }

        public void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerClient.CreatePlayerCharacterClient(AnyRPGSaveData)");

            networkController.CreatePlayerCharacter(anyRPGSaveData);
        }

        public void RequestLobbyGameList() {
            networkController.RequestLobbyGameList();
        }

        public void LoadCharacterList() {
            //Debug.Log($"NetworkManagerClient.LoadCharacterList()");

            networkController.LoadCharacterList();
        }

        public void DeletePlayerCharacter(int playerCharacterId) {
            Debug.Log($"NetworkManagerClient.DeletePlayerCharacter({playerCharacterId})");

            networkController.DeletePlayerCharacter(playerCharacterId);
        }

        public void CreateLobbyGame(string sceneName) {
            networkController.CreateLobbyGame(sceneName);
        }

        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            //Debug.Log($"NetworkManagerClient.AdvertiseCreateLobbyGame({lobbyGame.leaderClientId}) clientid: {clientId}");

            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            if (lobbyGame.leaderClientId == clientId) {
                this.lobbyGame = lobbyGame;
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
            OnCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int gameId) {
            networkController.CancelLobbyGame(gameId);
        }

        public void AdvertiseCancelLobbyGame(int gameId) {
            OnCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId) {
            networkController.JoinLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId) {
            networkController.LeaveLobbyGame(gameId);
        }

        public void SetClientId(int clientId) {
            this.clientId = clientId;
        }



        /*
        public int GetClientId() {
            Debug.Log($"NetworkManagerClient.GetClientId()");

            return networkController.GetClientId();
        }
        */

        public void AdvertiseClientJoinLobbyGame(int gameId, int clientId, string userName) {
            OnJoinLobbyGame(gameId, clientId, userName);
            lobbyGames[gameId].AddPlayer(clientId, userName);
            if (clientId == this.clientId) {
                // this client just joined a game
                lobbyGame = lobbyGames[gameId];
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
        }

        public void AdvertiseClientLeaveLobbyGame(int gameId, int clientId) {
            OnLeaveLobbyGame(gameId, clientId);
        }

        public void AdvertiseSendLobbyChatMessage(string messageText) {
            OnSendLobbyChatMessage(messageText);
        }

        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            OnSendLobbyGameChatMessage(messageText, gameId);
        }

        public void AdvertiseSendSceneChatMessage(string messageText, int clientId) {
            OnSendSceneChatMessage(messageText, clientId);
            logManager.WriteChatMessageClient(messageText);
        }

        public void AdvertiseLobbyLogin(int clientId, string userName) {
            OnLobbyLogin(clientId, userName);
        }

        public void AdvertiseLobbyLogout(int clientId) {
            OnLobbyLogout(clientId);
        }

        public void SetLobbyGameList(List<LobbyGame> lobbyGames) {
            //Debug.Log($"NetworkManagerClient.SetLobbyGameList({lobbyGames.Count})");

            this.lobbyGames.Clear();
            foreach (LobbyGame lobbyGame in lobbyGames) {
                this.lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            }
            OnSetLobbyGameList(this.lobbyGames.Values.ToList<LobbyGame>());
        }

        public void SetLobbyPlayerList(Dictionary<int, string> lobbyPlayers) {
            this.lobbyPlayers.Clear();
            foreach (int loggedInClientId in lobbyPlayers.Keys) {
                this.lobbyPlayers.Add(loggedInClientId, lobbyPlayers[loggedInClientId]);
            }
            OnSetLobbyPlayerList(lobbyPlayers);
        }

        public void ChooseLobbyGameCharacter(string unitProfileName) {
            networkController.ChooseLobbyGameCharacter(unitProfileName, lobbyGame.gameId);
        }

        public void AdvertiseChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName) {
            //Debug.Log($"NetworkManagerClient.AdvertiseChooseLobbyGameCharacter({gameId}, {clientId}, {unitProfileName})");

            if (lobbyGames.ContainsKey(gameId) == false) {
                // game does not exist
                return;
            }
            lobbyGames[gameId].PlayerList[clientId].unitProfileName = unitProfileName;
            
            OnChooseLobbyGameCharacter(gameId, clientId, unitProfileName);

            if (gameId == lobbyGame.gameId && clientId == this.clientId) {
                // the character was chosen for this client so close the new game window
                uIManager.newGameWindow.CloseWindow();
            }
        }

        public void StartLobbyGame(int gameId) {
            networkController.StartLobbyGame(gameId);
        }

        public void AdvertiseStartLobbyGame(int gameId, string sceneName) {
            if (lobbyGames.ContainsKey(gameId) == false) {
                // lobby game does not exist
                return;
            }
            lobbyGames[gameId].inProgress = true;
            if (lobbyGame == null || lobbyGame.gameId != gameId) {
                // have not joined lobby game, or joined different lobby game
                return;
            }

            // this is our lobby game
            uIManager.clientLobbyGameWindow.CloseWindow();

            playerManager.SpawnPlayerConnection();
            levelManager.LoadLevel(sceneName);
        }

        public void AdvertiseSetLobbyGameReadyStatus(int gameId, int clientId, bool ready) {
            //Debug.Log($"NetworkManagerClient.AdvertiseSetLobbyGameReadyStatus({gameId}, {clientId}, {ready})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(clientId) == false) {
                // game does not exist or player is not in game
                return;
            }
            lobbyGames[gameId].PlayerList[clientId].ready = ready;
            OnSetLobbyGameReadyStatus(gameId, clientId, ready);
        }

        public void AdvertiseLoadSceneClient(string sceneName) {
            levelManager.LoadLevel(sceneName);
        }

        /*
        public void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithQuestGiverClient(interactable, optionIndex, playerManager.UnitController);
        }
        */

        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            Debug.Log($"NetworkManagerClient.InteractWithOption({targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");
            networkController.InteractWithOption(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
        }

        public void AdvertiseAddSpawnRequest(LoadSceneRequest loadSceneRequest) {
            levelManager.AddSpawnRequest(ClientId, loadSceneRequest);
        }

        public void HandleSceneLoadStart(string sceneName) {
            levelManager.NotifyOnBeginLoadingLevel(sceneName);
        }

        public void HandleSceneLoadPercentageChange(float percent) {
            levelManager.SetLoadingProgress(percent);
        }

        /*
        public void AdvertiseInteractWithClassChangeComponent(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithClassChangeComponentClient(interactable, optionIndex);
        }
        */

        public void SetPlayerCharacterClass(string className) {
            networkController.SetPlayerCharacterClass(className);
        }

        public void SetPlayerCharacterSpecialization(string specializationName) {
            networkController.SetPlayerCharacterSpecialization(specializationName);
        }

        public void SetPlayerFaction(string factionName) {
            networkController.SetPlayerFaction(factionName);
        }

        /*
        public void AdvertiseInteractWithSkillTrainerComponent(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithSkillTrainerComponentClient(interactable, optionIndex);
        }
        */

        public void LearnSkill(string skillName) {
            networkController.LearnSkill(skillName);
        }

        public void AcceptQuest(Quest quest) {
            networkController.AcceptQuest(quest.ResourceName);
        }

        public void CompleteQuest(Quest quest, QuestRewardChoices questRewardChoices) {
            networkController.CompleteQuest(quest.ResourceName, questRewardChoices);
        }

        public void AdvertiseMessageFeedMessage(string message) {
            messageFeedManager.WriteMessage(message);
        }

        public void AdvertiseSystemMessage(string message) {
            logManager.WriteSystemMessage(message);
        }

        public void SellItemToVendor(Interactable interactable, int componentIndex, int itemInstanceId) {
            networkController.SellVendorItem(interactable, componentIndex, itemInstanceId);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName) {
            Debug.Log($"NetworkManagerClient.RequestSpawnUnit({unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfileName}, {unitToughnessName})");

            networkController.RequestSpawnUnit(interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfileName, unitToughnessName);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, int instantiatedItemId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(instantiatedItemId) == false) {
                return;
            }
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                (currentInteractables[componentIndex] as VendorComponent).AddToBuyBackCollection(sourceUnitController, componentIndex, systemItemManager.InstantiatedItems[instantiatedItemId]);
            }

        }

        public void AdvertiseSellItemToPlayerClient(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                VendorComponent vendorComponent = (currentInteractables[componentIndex] as VendorComponent);
                List<VendorCollection> localVendorCollections = vendorComponent.GetLocalVendorCollections();
                if (localVendorCollections.Count > collectionIndex && localVendorCollections[collectionIndex].VendorItems.Count > itemIndex) {
                    VendorItem vendorItem = localVendorCollections[collectionIndex].VendorItems[itemIndex];
                    if (vendorItem.Item.ResourceName == resourceName) {
                        vendorComponent.ProcessQuantityNotification(vendorItem, remainingQuantity);
                    }
                }
            }
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName) {
            networkController.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName);
        }

        public void TakeAllLoot() {
            networkController.TakeAllLoot();
        }

        public void AddDroppedLoot(int lootDropId, int itemId) {
            //Debug.Log($"NetworkManagerClient.AddDroppedLoot({lootDropId}, {itemId})");

            lootManager.AddNetworkLootDrop(lootDropId, itemId);
        }

        public void AddAvailableDroppedLoot(List<int> lootDropIds) {
            //Debug.Log($"NetworkManagerClient.AddAvailableDroppedLoot(count: {lootDropIds.Count})");

            //lootManager.AddAvailableLoot(clientId, lootDropIds);
            // available loot is always clientId 0 on client
            lootManager.AddAvailableLoot(0, lootDropIds);
        }

        public void AdvertiseTakeLoot(int lootDropId) {
            lootManager.TakeLoot(clientId, lootDropId);
        }

        public void RequestTakeLoot(int lootDropId) {
            networkController.RequestTakeLoot(lootDropId);
        }

        /*
        public void SetCraftingManagerAbility(CraftAbility craftAbility) {
            Debug.Log($"NetworkManagerClient.SetCraftingManagerAbility({craftAbility.DisplayName})");

            craftingManager.SetAbility(playerManager.UnitController, craftAbility.CraftAbilityProperties);
        }
        */

        public void RequestBeginCrafting(Recipe recipe, int craftAmount) {
            Debug.Log($"NetworkManagerClient.RequestBeginCrafting({recipe.DisplayName}, {craftAmount})");

            networkController.RequestBeginCrafting(recipe, craftAmount);
        }

        public void RequestCancelCrafting() {
            networkController.RequestCancelCrafting();
        }



        /*
        public void AdvertiseInteractWithAnimatedObjectComponent(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithAnimatedObjectComponentClient(interactable, optionIndex);
        }
        */
    }

    public enum NetworkClientMode { Lobby, MMO }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {

    /// <summary>
    /// meant to be inherited from by actual network implementations like fish-net, etc.
    /// </summary>
    public abstract class NetworkController : ConfiguredMonoBehaviour {
        
        // client functions
        public virtual bool Login(string username, string password, string server) {
            return false;
        }
        public abstract void Logout();
        public abstract void SpawnPlayer(int playerCharacterId, CharacterRequestData characterRequestData, Transform parentTransform);
        public abstract GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);
        public abstract void LoadScene(string sceneName);
        public abstract bool CanSpawnCharacterOverNetwork();
        public abstract bool OwnPlayer(UnitController unitController);
        public abstract void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData);
        public abstract void DeletePlayerCharacter(int playerCharacterId);
        public abstract void LoadCharacterList();
        public abstract void CreateLobbyGame(string sceneName);
        public abstract void CancelLobbyGame(int gameId);
        public abstract void JoinLobbyGame(int gameId);
        public abstract void LeaveLobbyGame(int gameId);
        public abstract int GetClientId();
        public abstract void SendLobbyChatMessage(string messageText);
        public abstract void SendLobbyGameChatMessage(string messageText, int gameId);
        public abstract void RequestLobbyGameList();
        public abstract void RequestLobbyPlayerList();

        // server functions
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void KickPlayer(int clientId);
        public abstract string GetClientIPAddress(int clientId);
        public abstract void AdvertiseCreateLobbyGame(LobbyGame lobbyGame);
        public abstract void AdvertiseCancelLobbyGame(int gameId);
        public abstract void AdvertiseClientJoinLobbyGame(int gameId, int clientId, string userName);
        public abstract void AdvertiseClientLeaveLobbyGame(int gameId, int clientId);
        public abstract void AdvertiseSendLobbyChatMessage(string messageText);
        public abstract void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId);
        public abstract void AdvertiseLobbyLogin(int clientId, string userName);
        public abstract void AdvertiseLobbyLogout(int clientId);
        public abstract void SetLobbyGameList(int clientId, List<LobbyGame> lobbyGames);
        public abstract void SetLobbyPlayerList(int clientId, Dictionary<int, string> lobbyPlayers);
    }

}
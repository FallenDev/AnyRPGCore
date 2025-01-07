using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public class LobbyGame {
        public int leaderClientId;
        public string leaderUserName = string.Empty;
        public int gameId;
        public string gameName = string.Empty;
        public string sceneName = string.Empty;

        private Dictionary<int, LobbyGamePlayerInfo> playerList = new Dictionary<int, LobbyGamePlayerInfo>();

        public Dictionary<int, LobbyGamePlayerInfo> PlayerList { get => playerList; set => playerList = value; }

        public LobbyGame() {
            /*
            leaderUserName = string.Empty;
            gameName = string.Empty;
            sceneName = string.Empty;
            */
        }

        public LobbyGame(int clientId, int gameId, string sceneName, string userName) {
            this.leaderClientId = clientId;
            leaderUserName = userName;
            this.gameId = gameId;
            this.sceneName = sceneName;
            playerList.Add(clientId, new LobbyGamePlayerInfo(clientId, userName));
        }

        public void AddPlayer(int clientId, string userName) {
            playerList.Add(clientId, new LobbyGamePlayerInfo(clientId, userName));
        }

        public void RemovePlayer(int clientId) {
            playerList.Remove(clientId);
        }
    }

    public class LobbyGamePlayerInfo {
        public int clientId;
        public string userName = string.Empty;
        public string unitProfileName = string.Empty;

        public LobbyGamePlayerInfo() {
        }

        public LobbyGamePlayerInfo(int clientId, string userName) {
            this.clientId = clientId;
            this.userName = userName;
        }
    }

}

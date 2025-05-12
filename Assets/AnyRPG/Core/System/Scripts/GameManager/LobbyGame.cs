using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public class LobbyGame {
        public int leaderClientId;
        public string leaderUserName = string.Empty;
        public int gameId;
        public string gameName = string.Empty;
        public string sceneResourceName = string.Empty;
        public bool inProgress = false;
        public bool allowLateJoin = false;

        private Dictionary<int, LobbyGamePlayerInfo> playerList = new Dictionary<int, LobbyGamePlayerInfo>();

        public Dictionary<int, LobbyGamePlayerInfo> PlayerList { get => playerList; set => playerList = value; }

        public LobbyGame() {
            /*
            leaderUserName = string.Empty;
            gameName = string.Empty;
            sceneName = string.Empty;
            */
        }

        public LobbyGame(int clientId, int gameId, string sceneResourceName, string userName, bool allowLateJoin) {
            this.leaderClientId = clientId;
            leaderUserName = userName;
            this.gameId = gameId;
            this.sceneResourceName = sceneResourceName;
            playerList.Add(clientId, new LobbyGamePlayerInfo(clientId, userName));
            this.allowLateJoin = allowLateJoin;
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
        public bool ready = false;

        public LobbyGamePlayerInfo() {
        }

        public LobbyGamePlayerInfo(int clientId, string userName) {
            this.clientId = clientId;
            this.userName = userName;
        }
    }

}

using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public class LobbyGame {
        public int leaderClientId;
        public string leaderUserName;
        public int gameId;
        public string gameName;
        public string sceneName;

        private Dictionary<int, string> playerList = new Dictionary<int, string>();

        public int GameId { get => gameId; }
        public Dictionary<int, string> PlayerList { get => playerList; }

        public LobbyGame() {
            leaderUserName = string.Empty;
            gameName = string.Empty;
            sceneName = string.Empty;
        }

        public LobbyGame(int clientId, int gameId, string sceneName, string userName) {
            this.leaderClientId = clientId;
            leaderUserName = userName;
            this.gameId = gameId;
            this.sceneName = sceneName;
            playerList.Add(clientId, userName);
        }

        public void AddPlayer(int clientId, string userName) {
            playerList.Add(clientId, userName);
        }

        public void RemovePlayer(int clientId) {
            playerList.Remove(clientId);
        }
    }

}

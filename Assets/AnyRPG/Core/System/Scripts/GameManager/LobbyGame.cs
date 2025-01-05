using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public class LobbyGame {
        public int leaderClientId;
        public int gameId;
        public string gameName;

        private List<int> playerList = new List<int>();

        public int GameId { get => gameId; }
        public List<int> PlayerList { get => playerList; }

        public LobbyGame() {
        }

        public LobbyGame(int clientId, int gameId) {
            this.leaderClientId = clientId;
            this.gameId = gameId;
            playerList.Add(clientId);
        }

        public void AddPlayer(int clientId) {
            playerList.Add(clientId);
        }

        public void RemovePlayer(int clientId) {
            playerList.Remove(clientId);
        }
    }

}

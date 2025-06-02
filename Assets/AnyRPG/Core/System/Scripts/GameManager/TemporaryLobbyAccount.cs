using UnityEngine;

namespace AnyRPG {
    public class TemporaryLobbyAccount {
        public int accountId;
        public string username;
        public string password;

        public TemporaryLobbyAccount(int accountId, string username, string password) {
            this.accountId = accountId;
            this.username = username;
            this.password = password;
        }
    }

}

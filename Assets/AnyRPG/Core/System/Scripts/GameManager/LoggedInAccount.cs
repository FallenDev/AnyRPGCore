using UnityEngine;

namespace AnyRPG {
    public class LoggedInAccount {

        public int clientId;
        public string username;
        public string token;
        public string ipAddress;

        public LoggedInAccount(int clientId, string username, string token, string ipAddress) {
            this.clientId = clientId;
            this.username = username;
            this.token = token;
            this.ipAddress = ipAddress;
        }
    }
}

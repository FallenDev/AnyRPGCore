using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ClientPlayerLobbyConnectionButton : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        private int clientId;

        // game manager references
        NetworkManagerServer networkManagerServer = null;

        public TextMeshProUGUI PlayerNameText { get => playerNameText; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SetClientId(int clientId, string userName) {
            this.clientId = clientId;
            playerNameText.text = userName;
        }

    }

}

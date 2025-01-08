using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ClientPlayerLobbyGameConnectionButton : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        [SerializeField]
        private TextMeshProUGUI unitProfileNameText = null;

        private int clientId;

        // game manager references
        NetworkManagerServer networkManagerServer = null;

        public TextMeshProUGUI PlayerNameText { get => playerNameText; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SetClientId(int clientId, string userName, string unitProfileName) {
            this.clientId = clientId;
            playerNameText.text = userName;
            unitProfileNameText.text = unitProfileName;
        }

        public void SetUnitProfileName(string unitProfileName) {
            unitProfileNameText.text = unitProfileName;
        }

    }

}

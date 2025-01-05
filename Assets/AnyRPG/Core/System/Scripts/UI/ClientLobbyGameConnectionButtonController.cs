using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ClientLobbyGameConnectionButtonController : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI gameNameText = null;

        [SerializeField]
        private TextMeshProUGUI gameInfoText = null;

        [SerializeField]
        private HighlightButton joinButton = null;

        private int gameId;

        // game manager references
        NetworkManagerClient networkManagerClient = null;

        public TextMeshProUGUI PlayerNameText { get => gameNameText; }
        public TextMeshProUGUI PlayerInfoText { get => gameInfoText; }
        public HighlightButton JoinButton { get => joinButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerClient = systemGameManager.NetworkManagerClient;
            joinButton.Configure(systemGameManager);
        }

        public void SetGameId(int gameId, string gameName, string infoText) {
            this.gameId = gameId;
            gameNameText.text = gameName;
            gameInfoText.text = infoText;

        }

        public void JoinGame() {
            networkManagerClient.JoinLobbyGame(gameId);
        }


    }

}

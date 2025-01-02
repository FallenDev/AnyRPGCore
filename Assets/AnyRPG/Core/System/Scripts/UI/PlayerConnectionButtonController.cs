using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class PlayerConnectionButtonController : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        [SerializeField]
        private TextMeshProUGUI playerInfoText = null;

        [SerializeField]
        private HighlightButton nameHighlightButton = null;

        [SerializeField]
        private HighlightButton infoHighlightButton = null;

        public TextMeshProUGUI PlayerNameText { get => playerNameText; set => playerNameText = value; }
        public TextMeshProUGUI PlayerInfoText { get => playerInfoText; set => playerInfoText = value; }
        public HighlightButton NameHighlightButton { get => nameHighlightButton; }
        public HighlightButton AttributionHighlightButton { get => infoHighlightButton; }

        //public void OpenURL() {
        //    if (UserUrl != null && UserUrl != string.Empty) {
        //        Application.OpenURL(UserUrl);
        //    }
        //}


    }

}

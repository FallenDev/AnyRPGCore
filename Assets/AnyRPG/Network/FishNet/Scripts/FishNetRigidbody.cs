using UnityEngine;

namespace AnyRPG {
    public class FishNetRigidbody : ConfiguredNetworkBehaviour {

        [SerializeField]
        private Rigidbody rigidbodyComponent = null;

        // game manager references
        protected NetworkManagerServer networkManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            Debug.Log($"{gameObject.name}.FishNetRigidbody.Configure()");

            base.Configure(systemGameManager);
            /*
            if (rigidbodyComponent != null && systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                rigidbodyComponent.isKinematic = true;
            }
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        protected virtual void LocalConfigure() {
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            if (systemGameManager == null) {
                return;
            }
            Configure(systemGameManager);
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStartClient()");

            base.OnStartClient();

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            LocalConfigure();
        }
    }

}

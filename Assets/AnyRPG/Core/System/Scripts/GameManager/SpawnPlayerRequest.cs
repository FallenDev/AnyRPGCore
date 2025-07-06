using UnityEngine;

namespace AnyRPG {
    public class SpawnPlayerRequest {
        public string locationTag = string.Empty;
        public bool overrideSpawnLocation = false;
        public Vector3 spawnLocation = Vector3.zero;
        public bool overrideSpawnDirection = false;
        public Vector3 spawnForwardDirection = Vector3.forward;

        public SpawnPlayerRequest () {
        }

        public SpawnPlayerRequest (Vector3 position, Vector3 forwardDirection) {
            spawnLocation = position;
            spawnForwardDirection = forwardDirection;
        }

    }
}


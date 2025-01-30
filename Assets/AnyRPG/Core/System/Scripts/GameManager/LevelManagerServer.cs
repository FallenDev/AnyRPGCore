using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class LevelManagerServer : ConfiguredMonoBehaviour {

        private Dictionary<string, Scene> loadedScenes = new Dictionary<string, Scene>();

        // game manager references
        private LevelManager levelManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;
        private CameraManager cameraManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            cameraManager = systemGameManager.CameraManager;
        }

        public void AddLoadedScene(Scene scene) {
            Debug.Log($"LevelManagerServer.AddLoadedScene({scene.name})");

            loadedScenes.Add(scene.name, scene);
        }

        public void RemoveLoadedScene(string sceneName) {
            Debug.Log($"LevelManagerServer.RemoveLoadedScene({sceneName})");

            loadedScenes.Remove(sceneName);
        }

        public void ProcessLevelLoad(string sceneName)  {
            Debug.Log($"LevelManagerServer.ProcessLevelLoad({sceneName})");

            cameraManager.ActivateMainCamera();
            systemGameManager.AutoConfigureMonoBehaviours(sceneName);

        }

    }

}
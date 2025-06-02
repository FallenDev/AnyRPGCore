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

        // dictionary of loaded scenes, where the key is the scene name and the value is a list of scene handles
        private Dictionary<string, Dictionary<int, Scene>> loadedScenes = new Dictionary<string, Dictionary<int, Scene>>();

        // game manager references
        private LevelManager levelManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;
        private CameraManager cameraManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            cameraManager = systemGameManager.CameraManager;
        }

        public void HandleSceneUnloaded(Scene scene) {
            Debug.Log($"LevelManagerServer.HandleSceneUnloaded({scene.name})");

            // if the scene is not in the loaded scenes, then we don't need to do anything
            if (loadedScenes.ContainsKey(scene.name) == false) {
                //Debug.LogWarning($"LevelManagerServer.HandleSceneUnloaded() - scene {scene.name} not found in loaded scenes");
                return;
            }
            // remove the scene from the loaded scenes
            RemoveLoadedScene(scene.handle, scene.name);
            // if there are no more handles for this scene, then remove it from the dictionary
            if (loadedScenes[scene.name].Count == 0) {
                loadedScenes.Remove(scene.name);
            }

            networkManagerServer.HandleSceneUnloadEnd(scene.handle, scene.name);
        }

        public void AddLoadedScene(Scene scene) {
            Debug.Log($"LevelManagerServer.AddLoadedScene({scene.name})");
            if (loadedScenes.ContainsKey(scene.name) == false) {
                loadedScenes.Add(scene.name, new Dictionary<int, Scene>());
            }
            loadedScenes[scene.name].Add(scene.handle, scene);
        }

        public void RemoveLoadedScene(int sceneHandle, string sceneName) {
            Debug.Log($"LevelManagerServer.RemoveLoadedScene({sceneHandle}, {sceneName})");

            if (loadedScenes.ContainsKey(sceneName) == false) {
                //Debug.LogError($"LevelManagerServer.RemoveLoadedScene() - scene {sceneName} not found in loaded scenes");
                return;
            }

            loadedScenes[sceneName].Remove(sceneHandle);
        }

        public void ProcessLevelLoad(Scene loadedScene)  {
            Debug.Log($"LevelManagerServer.ProcessLevelLoad({loadedScene.name}({loadedScene.handle}))");

            cameraManager.ActivateMainCamera();
            systemGameManager.AutoConfigureMonoBehaviours(loadedScene);

        }

    }

}
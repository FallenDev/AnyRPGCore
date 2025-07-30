using FishNet.Managing.Scened;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class WeatherManagerServer : ConfiguredMonoBehaviour {


        private Dictionary<int, WeatherMonitor> weatherMonitors = new Dictionary<int, WeatherMonitor>();

        // game manager references
        protected SystemDataFactory systemDataFactory = null;
        protected LevelManager levelManager = null;
        protected PlayerManager playerManager = null;
        protected CameraManager cameraManager = null;
        protected TimeOfDayManagerServer timeOfDayManagerServer = null;
        protected TimeOfDayManagerClient timeOfDayManagerClient = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected SystemEventManager systemEventManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemEventManager.OnLevelUnload += HandleLevelUnload;
            systemEventManager.OnLevelLoad += HandleLevelLoad;
            systemEventManager.OnAddLoadedScene += HandleAddLoadedScene;
            systemEventManager.OnRemoveLoadedScene += HandleRemoveLoadedScene;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            levelManager = systemGameManager.LevelManager;
            playerManager = systemGameManager.PlayerManager;
            cameraManager = systemGameManager.CameraManager;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            timeOfDayManagerClient = systemGameManager.TimeOfDayManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandleRemoveLoadedScene(int sceneHandle, string sceneName) {
            Debug.Log($"WeatherManagerServer.HandleRemoveLoadedScene({sceneHandle}, {sceneName})");

            ProcessRemoveLoadedScene(sceneHandle);
        }

        private void HandleAddLoadedScene(int sceneHandle, string sceneName) {
            Debug.Log($"WeatherManagerServer.HandleAddLoadedScene({sceneHandle}, {sceneName})");

            if (levelManager.SceneDictionary.ContainsKey(sceneName) == true) {
                ProcessAddLoadedScene(sceneHandle, levelManager.SceneDictionary[sceneName]);
            }
        }

        public void HandleLevelUnload(int sceneHandle) {
            Debug.Log($"WeatherManagerServer.HandleLevelUnload({sceneHandle})");

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }

            ProcessRemoveLoadedScene(sceneHandle);
        }

        public void HandleLevelLoad() {
            Debug.Log("WeatherManagerServer.HandleLevelLoad()");

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }

            if (levelManager.IsMainMenu() == true) {
                return;
            }

            if (levelManager.GetActiveSceneNode() != null) {
                ProcessAddLoadedScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle, levelManager.GetActiveSceneNode());
            }
        }

        private void ProcessAddLoadedScene(int sceneHandle, SceneNode sceneNode) {
            Debug.Log($"WeatherManagerServer.ProcessAddLoadedScene({sceneHandle}, {sceneNode.ResourceName})");

            weatherMonitors.Add(sceneHandle, new WeatherMonitor(systemGameManager, sceneHandle, sceneNode));
        }

        private void ProcessRemoveLoadedScene(int sceneHandle) {
            Debug.Log($"WeatherManagerServer.ProcessRemoveLoadedScene({sceneHandle})");

            if (weatherMonitors.ContainsKey(sceneHandle) == true) {
                weatherMonitors[sceneHandle].EndWeather();
                weatherMonitors.Remove(sceneHandle);
            }
        }

        public void EndWeather(int sceneHandle, WeatherProfile previousWeather, bool immediate) {
            Debug.Log($"WeatherManagerServer.EndWeather({sceneHandle}, {previousWeather?.ResourceName}, {immediate})");

            systemEventManager.NotifyOnEndWeather(sceneHandle, previousWeather, immediate);
        }

        public void ChooseWeather(int sceneHandle, WeatherProfile currentWeather) {
            Debug.Log($"WeatherManagerServer.ChooseWeather({sceneHandle}, {currentWeather?.ResourceName})");

            systemEventManager.NotifyOnChooseWeather(sceneHandle, currentWeather);
        }

        public void StartWeather(int sceneHandle) {
            Debug.Log($"WeatherManagerServer.StartWeather({sceneHandle})");

            systemEventManager.NotifyOnStartWeather(sceneHandle);
        }

        public WeatherProfile GetSceneWeatherProfile(int handle) {
            if (weatherMonitors.ContainsKey(handle) == true) {
                return weatherMonitors[handle].CurrentWeather;
            } else {
                //Debug.LogWarning($"WeatherManagerServer.GetSceneWeatherProfile({handle}) - no weather monitor found for this scene");
                return null;
            }
        }
    }

}
using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace AnyRPG {
    public class LoadScreenManager : ConfiguredMonoBehaviour {

        [Header("Loading Screen")]

        public Slider loadBar;
        public TextMeshProUGUI finishedLoadingText;
        public Image backgroundImage = null;

        // game manager references
        private LevelManager levelManager = null;
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            levelManager.OnBeginLoadingLevel += HandleBeginLoadingLevel;
            levelManager.OnSetLoadingProgress += HandleSetLoadingProgress;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
        }

        public void HandleBeginLoadingLevel(string sceneName) {
            uIManager.ActivateLoadingUI();

            if (levelManager.LoadingSceneNode != null && levelManager.LoadingSceneNode.LoadingScreenImage != null) {
                backgroundImage.sprite = levelManager.LoadingSceneNode.LoadingScreenImage;
                backgroundImage.color = Color.white;
            } else {
                backgroundImage.sprite = null;
                backgroundImage.color = Color.black;
            }
        }

        private void HandleSetLoadingProgress(float newProgress) {
            if (loadBar != null) {
                loadBar.value = newProgress;
            }
        }

        public void HandleEndLoadingLevel() { 
        }

    }

}
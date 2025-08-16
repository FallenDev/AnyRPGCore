using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class FishNetNewSceneWizard : NewSceneWizardBase, ICreateSceneRequestor {

        private const string pathToPhysicsSceneSync = "/AnyRPG/Network/FishNet/GameManager/FishNetPhysicsSceneSync.prefab";

        [MenuItem("Tools/AnyRPG/Wizard/FishNet/New Scene Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<FishNetNewSceneWizard>("FishNet New Scene Wizard", "Create");
        }

        public override void ModifyScene() {
            base.ModifyScene();
            string sceneConfigPrefabAssetPath = "Assets" + pathToPhysicsSceneSync;
            GameObject sceneSyncGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(sceneConfigPrefabAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneSyncGameObject);
            instantiatedGO.transform.SetAsFirstSibling();
        }

    }


}

using UnityEditor;

namespace AnyRPG {
    public class NewSceneWizard : NewSceneWizardBase, ICreateSceneRequestor {

        [MenuItem("Tools/AnyRPG/Wizard/New Scene Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewSceneWizard>("New Scene Wizard", "Create");
        }
        
    }

}

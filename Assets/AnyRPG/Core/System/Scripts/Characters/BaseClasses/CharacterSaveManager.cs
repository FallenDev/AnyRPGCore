using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSaveManager : ConfiguredClass {

        private UnitController unitController;

        private Dictionary<string, BehaviorSaveData> behaviorSaveDataDictionary = new Dictionary<string, BehaviorSaveData>();
        private Dictionary<string, DialogSaveData> dialogSaveDataDictionary = new Dictionary<string, DialogSaveData>();

        public Dictionary<string, BehaviorSaveData> BehaviorSaveDataDictionary { get => behaviorSaveDataDictionary; set => behaviorSaveDataDictionary = value; }
        public Dictionary<string, DialogSaveData> DialogSaveDataDictionary { get => dialogSaveDataDictionary; set => dialogSaveDataDictionary = value; }

        public CharacterSaveManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public DialogSaveData GetDialogSaveData(Dialog dialog) {
            if (dialogSaveDataDictionary.ContainsKey(dialog.ResourceName)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): dialogSaveData size {dialogSaveDataDictionary[dialog.resourceName].dialogNodeShown.Count}");
                return dialogSaveDataDictionary[dialog.ResourceName];
            } else {
                DialogSaveData saveData = new DialogSaveData();
                saveData.dialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): initialized list size {dialog.DialogNodes.Count} : {saveData.dialogNodeShown.Count}");
                saveData.DialogName = dialog.ResourceName;
                dialogSaveDataDictionary.Add(dialog.ResourceName, saveData);
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): dialogSaveData size {dialogSaveDataDictionary[dialog.resourceName].dialogNodeShown.Count}");
                return saveData;
            }
            //return saveData;
        }

        public BehaviorSaveData GetBehaviorSaveData(BehaviorProfile behaviorProfile) {
            BehaviorSaveData saveData;
            if (behaviorSaveDataDictionary.ContainsKey(behaviorProfile.ResourceName)) {
                saveData = behaviorSaveDataDictionary[behaviorProfile.ResourceName];
            } else {
                saveData = new BehaviorSaveData();
                saveData.BehaviorName = behaviorProfile.ResourceName;
                behaviorSaveDataDictionary.Add(behaviorProfile.ResourceName, saveData);
            }
            return saveData;
        }

        public bool GetDialogNodeShown(Dialog dialog, int index) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogNodeShown({dialog.ResourceName}, {index})");
            DialogSaveData saveData = GetDialogSaveData(dialog);
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogNodeShown({dialog.ResourceName}, {index}) count: {saveData.dialogNodeShown.Count}");
            return saveData.dialogNodeShown[index];
        }

        public void SetDialogNodeShown(Dialog dialog, bool value, int index) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.dialogNodeShown[index] = value;
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
        }

        public void ResetDialogNodes(Dialog dialog) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.dialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
        }
    }

}
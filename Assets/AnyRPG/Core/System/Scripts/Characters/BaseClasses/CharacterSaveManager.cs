using AnyRPG;
using System.Collections;
using System.Collections.Generic;
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
            DialogSaveData saveData;
            if (dialogSaveDataDictionary.ContainsKey(dialog.ResourceName)) {
                saveData = dialogSaveDataDictionary[dialog.ResourceName];
            } else {
                saveData = new DialogSaveData();
                saveData.DialogName = dialog.ResourceName;
                dialogSaveDataDictionary.Add(dialog.ResourceName, saveData);
            }
            return saveData;
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



    }

}
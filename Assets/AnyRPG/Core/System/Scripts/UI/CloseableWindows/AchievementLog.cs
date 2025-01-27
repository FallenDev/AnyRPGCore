using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Maintains a list of all achievements
    /// </summary>
    public class AchievementLog : ConfiguredMonoBehaviour {

        private Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        // game manager references
        SystemDataFactory systemDataFactory = null;

        public Dictionary<string, Achievement> Achievements { get => achievements; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

       

    }

}
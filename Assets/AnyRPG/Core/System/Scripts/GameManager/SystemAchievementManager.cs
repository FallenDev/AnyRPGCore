using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemAchievementManager : ConfiguredMonoBehaviour {

        //private bool eventSubscriptionsInitialized = false;

        // game manager references
        //PlayerManager playerManager = null;
        SystemDataFactory systemDataFactory = null;
        //NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            //networkManagerClient = systemGameManager.NetworkManagerClient;

            //CreateEventSubscriptions();
        }

        /*
        public void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }
        */

        public void OnDisable() {
            //Debug.Log("SystemAchievementManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            //CleanupEventSubscriptions();
        }

        public void AcceptAchievements(UnitController sourceUnitController) {
            //Debug.Log($"SystemAchievementManager.AcceptAchievements({sourceUnitController.gameObject.name})");

            foreach (Achievement resource in systemDataFactory.GetResourceList<Achievement>()) {
                if (resource.TurnedIn(sourceUnitController) == false && resource.IsComplete(sourceUnitController) == false) {
                    sourceUnitController.CharacterQuestLog.AcceptAchievement(resource);
                }
            }
        }

       
    }

}
using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Action Command", menuName = "AnyRPG/Chat Commands/Action Command")]
    public class ActionCommand : ChatCommand {

        [Header("Action Command")]

        [Tooltip("The name of the action to perform")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimatedAction))]
        protected string actionName = string.Empty;

        protected AnimatedAction animatedAction = null;

        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("ActionCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");
            
            if (animatedAction == null) {
                return;
            }

            playerManager.UnitController.UnitActionManager.BeginAction(animatedAction);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (actionName != string.Empty) {
                animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
                if (animatedAction == null) {
                    Debug.LogError("ActionCommand.SetupScriptableObjects(): Could not find action : " + actionName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}
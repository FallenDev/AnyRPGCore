using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneComponent : PortalComponent {

        public LoadSceneProps LoadSceneProps { get => interactableOptionProps as LoadSceneProps; }

        public LoadSceneComponent(Interactable interactable, LoadSceneProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.LoadSceneComponent.Interact({sourceUnitController.gameObject.name}, {componentIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            //levelManager.LoadLevel(LoadSceneProps.SceneName);
            playerManagerServer.LoadScene(LoadSceneProps.SceneName, sourceUnitController);
            return true;
        }

    }
}
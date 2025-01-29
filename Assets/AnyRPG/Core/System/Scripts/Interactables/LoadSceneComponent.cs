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

        public override bool Interact(UnitController sourceUnitController, int optionIndex) {
            Debug.Log($"{interactable.gameObject.name}.LoadSceneComponent.Interact({sourceUnitController.gameObject.name}, {optionIndex})");

            base.Interact(sourceUnitController, optionIndex);

            //levelManager.LoadLevel(LoadSceneProps.SceneName);
            playerManagerServer.LoadScene(LoadSceneProps.SceneName, sourceUnitController);
            return true;
        }

    }
}
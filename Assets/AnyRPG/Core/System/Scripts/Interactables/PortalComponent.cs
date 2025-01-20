using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class PortalComponent : InteractableOptionComponent {

        // game manager references
        protected LevelManager levelManager = null;

        public PortalProps Props { get => interactableOptionProps as PortalProps; }

        public PortalComponent(Interactable interactable, PortalProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
        }

        public override bool Interact(CharacterUnit source, int optionIndex) {
            Debug.Log($"{interactable.gameObject.name}.PortalComponent.Interact({source.UnitController.gameObject.name})");

            base.Interact(source, optionIndex);
            //Debug.Log($"{gameObject.name}.PortalInteractable.Interact(): about to close interaction window");
            uIManager.interactionWindow.CloseWindow();
            //Debug.Log($"{gameObject.name}.PortalInteractable.Interact(): window should now be closed!!!!!!!!!!!!!!!!!");
            LoadSceneRequest loadSceneRequest = new LoadSceneRequest();
            if (Props.OverrideSpawnDirection == true) {
                loadSceneRequest.overrideSpawnDirection = true;
                loadSceneRequest.spawnForwardDirection = Props.SpawnForwardDirection;
            }
            if (Props.OverrideSpawnLocation == true) {
                loadSceneRequest.overrideSpawnLocation = true;
                loadSceneRequest.spawnLocation = Props.SpawnLocation;
            } else {
                if (Props.LocationTag != null && Props.LocationTag != string.Empty) {
                    loadSceneRequest.locationTag = Props.LocationTag;
                }
            }
            playerManagerServer.AddSpawnRequest(source.UnitController, loadSceneRequest);
            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log($"{gameObject.name}.PortalInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

    }

}
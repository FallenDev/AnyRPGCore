using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace AnyRPG {
    public class InteractionManager : ConfiguredMonoBehaviour {

        public event System.Action<Interactable> OnSetInteractable = delegate { };

        private Interactable currentInteractable = null;
        private InteractableOptionComponent currentInteractableOptionComponent = null;
        private InteractableOptionManager interactableOptionManager = null;

        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;
        private UIManager uiManager = null;
        private NetworkManagerServer networkManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private ClassChangeManager classChangeManager = null;
        private UIManager uIManager = null;

        /*
        public Interactable CurrentInteractable {
            get => currentInteractable;
            set {
                //Debug.Log("CurrentInteractable");
                currentInteractable = value;
                OnSetInteractable(currentInteractable);
            }
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            uiManager = systemGameManager.UIManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            classChangeManager = systemGameManager.ClassChangeManager;
        }

        public bool Interact(Interactable target) {
            // get reference to name now since interactable could change scene and then target reference is lost
            string targetDisplayName = target.DisplayName;

            //if (target.Interact(playerManager.ActiveUnitController.CharacterUnit, true)) {
            if (InteractWithInteractable(playerManager.ActiveUnitController, target)) {
                //Debug.Log($"{gameObject.name}.PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                systemEventManager.NotifyOnInteractionStarted(targetDisplayName);
                return true;
            }
            return false;
        }

        public bool InteractWithInteractable(UnitController sourceUnitController, Interactable targetInteractable) {
            Debug.Log("InteractionManager.InteractWithInteractable");

            // perform range check
            bool passedRangeCheck = false;

            Collider[] colliders = new Collider[0];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int interactableMask = 1 << LayerMask.NameToLayer("Interactable");
            int triggerMask = 1 << LayerMask.NameToLayer("Triggers");
            int validMask = (playerMask | characterMask | interactableMask | triggerMask);
            Vector3 bottomPoint = new Vector3(sourceUnitController.Collider.bounds.center.x,
            sourceUnitController.Collider.bounds.center.y - sourceUnitController.Collider.bounds.extents.y,
                sourceUnitController.Collider.bounds.center.z);
            Vector3 topPoint = new Vector3(sourceUnitController.Collider.bounds.center.x,
            sourceUnitController.Collider.bounds.center.y + sourceUnitController.Collider.bounds.extents.y,
                sourceUnitController.Collider.bounds.center.z);
            colliders = Physics.OverlapCapsule(bottomPoint, topPoint, targetInteractable.InteractionMaxRange, validMask);
            foreach (Collider collider in colliders) {
                if (collider.gameObject == targetInteractable.gameObject) {
                    passedRangeCheck = true;
                    break;
                }
            }

            float factionValue = targetInteractable.PerformFactionCheck(sourceUnitController);

            // get a list of valid interactables to determine if there is an action we can treat as default
            Dictionary<int, InteractableOptionComponent> validInteractables = targetInteractable.GetCurrentInteractables(factionValue);
            Dictionary<int, InteractableOptionComponent> inRangeInteractables = new Dictionary<int, InteractableOptionComponent>();
            foreach (KeyValuePair<int, InteractableOptionComponent> validInteractable in validInteractables) {
                //Debug.Log($"{gameObject.name}.Interactable.Interact(" + source.name + "): valid interactable name: " + validInteractable);
                if (validInteractable.Value.CanInteract(true, passedRangeCheck, factionValue)) {
                    inRangeInteractables.Add(validInteractable.Key, validInteractable.Value);
                }
            }

            if (inRangeInteractables.Count > 0) {
                targetInteractable.InteractWithPlayer(sourceUnitController, factionValue);
                if (targetInteractable.SuppressInteractionWindow == true || inRangeInteractables.Count == 1) {
                    int firstInteractable = inRangeInteractables.Take(1).Select(d => d.Key).First();
                    if (inRangeInteractables[firstInteractable].GetCurrentOptionCount() > 1) {
                        OpenInteractionWindow(targetInteractable);
                    } else {
                        InteractWithOptionClient(sourceUnitController, targetInteractable, inRangeInteractables[firstInteractable], firstInteractable);
                        
                    }
                } else {
                    OpenInteractionWindow(targetInteractable);
                }
                return true;
            }

            if (validInteractables.Count > 0 && inRangeInteractables.Count == 0) {
                if (passedRangeCheck == false) {
                    sourceUnitController.UnitEventController.NotifyOnMessageFeed($"{targetInteractable.DisplayName} is out of range");
                }
            }
            return false;
        }

        public void InteractWithTrigger(UnitController unitController, Interactable triggerInteractable) {
            Debug.Log($"InteractionManager.InteractionWithTrigger({unitController.gameObject.name}, {triggerInteractable.gameObject.name})");

            // no range check for triggers since the unit walked into it so we know its in range
            Dictionary<int, InteractableOptionComponent> validInteractables = triggerInteractable.GetCurrentInteractables(0f);
            if (validInteractables.Count == 1) {
                int firstInteractable = validInteractables.Take(1).Select(d => d.Key).First();
                InteractWithOptionInternal(unitController, triggerInteractable, validInteractables[firstInteractable], firstInteractable);
            }
        }

        public void InteractWithOptionClient(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                InteractWithOptionInternal(sourceUnitController, targetInteractable, interactableOptionComponent, componentIndex);
            } else {
                networkManagerClient.InteractWithOption(sourceUnitController, targetInteractable, componentIndex);
            }
        }

        public void InteractWithOptionServer(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex) {
            Dictionary<int, InteractableOptionComponent> interactionOptions = targetInteractable.GetCurrentInteractables();
            if (interactionOptions.ContainsKey(componentIndex)) {
                InteractWithOptionInternal(sourceUnitController, targetInteractable, interactionOptions[componentIndex], componentIndex);
            }
        }

        public void InteractWithOptionInternal(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex) {
            Debug.Log($"InteractionManager.InteractWithOptionInternal({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name})");

            interactableOptionComponent.Interact(sourceUnitController.CharacterUnit, componentIndex);
        }

        public void OpenInteractionWindow(Interactable targetInteractable) {
            //Debug.Log($"{gameObject.name}.Interactable.OpenInteractionWindow");
            BeginInteraction(targetInteractable);
            uiManager.craftingWindow.CloseWindow();
            uiManager.interactionWindow.OpenWindow();
        }


        public void BeginInteraction(Interactable interactable) {
            SetInteractable(interactable);
            interactable.ProcessStartInteract();
        }

        public void EndInteraction() {
            currentInteractable.ProcessStopInteract();
            SetInteractable(null);
        }

        public void SetInteractable(Interactable interactable) {
            currentInteractable = interactable;
            OnSetInteractable(currentInteractable);
        }

        public void BeginInteractionWithOption(InteractableOptionComponent interactableOptionComponent, InteractableOptionManager interactableOptionManager) {
            this.interactableOptionManager = interactableOptionManager;
            currentInteractableOptionComponent = interactableOptionComponent;
            SetInteractable(interactableOptionComponent.Interactable);
        }

        public void InteractWithClassChangeComponent(Interactable interactable, int optionIndex) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables();
            if (currentInteractables.ContainsKey(optionIndex)) {
                if (currentInteractables[optionIndex] is ClassChangeComponent) {
                    InteractWithClassChangeComponent(currentInteractables[optionIndex] as ClassChangeComponent);
                }
            }
        }

        public void InteractWithClassChangeComponent(ClassChangeComponent classChangeComponent) {
            classChangeManager.SetDisplayClass(classChangeComponent.Props.CharacterClass, classChangeComponent);
            uIManager.classChangeWindow.OpenWindow();
        }


        //public void SetInteractableOptionManager(InteractableOptionManager interactableOptionManager) {
        //}



    }

}
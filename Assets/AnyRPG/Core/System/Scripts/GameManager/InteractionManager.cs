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
        private PlayerManagerServer playerManagerServer = null;
        private SystemEventManager systemEventManager = null;
        private NetworkManagerServer networkManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private ClassChangeManager classChangeManager = null;
        private UIManager uIManager = null;
        private DialogManager dialogManager = null;
        private SkillTrainerManager skillTrainerManager = null;

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
            //Debug.Log($"InteractionManager.SetGameManagerReferences()");

            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            uIManager = systemGameManager.UIManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            classChangeManager = systemGameManager.ClassChangeManager;
            dialogManager = systemGameManager.DialogManager;
            skillTrainerManager = systemGameManager.SkillTrainerManager;
        }

        public bool Interact(UnitController sourceUnitController, Interactable target) {
            Debug.Log($"InteractionManager.Interact({sourceUnitController.gameObject.name}, {target.gameObject.name})");

            // get reference to name now since interactable could change scene and then target reference is lost
            string targetDisplayName = target.DisplayName;

            //if (target.Interact(playerManager.ActiveUnitController.CharacterUnit, true)) {
            if (InteractWithInteractable(playerManager.ActiveUnitController, target)) {
                //Debug.Log($"{gameObject.name}.PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                systemEventManager.NotifyOnInteractionStarted(sourceUnitController, targetDisplayName);
                return true;
            }
            return false;
        }

        public bool InteractWithInteractable(UnitController sourceUnitController, Interactable targetInteractable) {
            Debug.Log($"InteractionManager.InteractWithInteractable({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name})");

            // perform range check
            bool passedRangeCheck = false;

            Collider[] colliders = new Collider[100];
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
            sourceUnitController.PhysicsScene.OverlapCapsule(bottomPoint, topPoint, targetInteractable.InteractionMaxRange, colliders, validMask);
            foreach (Collider collider in colliders) {
                if (collider == null) {
                    continue;
                }
                if (collider.gameObject == targetInteractable.gameObject) {
                    passedRangeCheck = true;
                    break;
                }
            }

            //float factionValue = targetInteractable.PerformFactionCheck(sourceUnitController);

            // get a list of valid interactables to determine if there is an action we can treat as default
            Dictionary<int, InteractableOptionComponent> validInteractables = targetInteractable.GetCurrentInteractables(sourceUnitController);
            Dictionary<int, InteractableOptionComponent> inRangeInteractables = new Dictionary<int, InteractableOptionComponent>();
            foreach (KeyValuePair<int, InteractableOptionComponent> validInteractable in validInteractables) {
                //Debug.Log($"{gameObject.name}.Interactable.Interact(" + source.name + "): valid interactable name: " + validInteractable);
                if (validInteractable.Value.CanInteract(sourceUnitController, true, passedRangeCheck)) {
                    inRangeInteractables.Add(validInteractable.Key, validInteractable.Value);
                }
            }

            if (inRangeInteractables.Count > 0) {
                targetInteractable.InteractWithPlayer(sourceUnitController);
                if (targetInteractable.SuppressInteractionWindow == true || inRangeInteractables.Count == 1) {
                    int firstInteractable = inRangeInteractables.Take(1).Select(d => d.Key).First();
                    if (inRangeInteractables[firstInteractable].GetCurrentOptionCount(sourceUnitController) > 1) {
                        OpenInteractionWindow(targetInteractable);
                    } else {
                        InteractWithOptionClient(sourceUnitController, targetInteractable, inRangeInteractables[firstInteractable], firstInteractable, 0);
                        
                    }
                } else {
                    OpenInteractionWindow(targetInteractable);
                }
                return true;
            }

            if (validInteractables.Count > 0 && inRangeInteractables.Count == 0) {
                if (passedRangeCheck == false) {
                    sourceUnitController.UnitEventController.NotifyOnMessageFeedMessage($"{targetInteractable.DisplayName} is out of range");
                }
            }
            return false;
        }

        public void InteractWithTrigger(UnitController unitController, Interactable triggerInteractable) {
            Debug.Log($"InteractionManager.InteractionWithTrigger({unitController.gameObject.name}, {triggerInteractable.gameObject.name})");

            // no range check for triggers since the unit walked into it so we know its in range
            Dictionary<int, InteractableOptionComponent> validInteractables = triggerInteractable.GetCurrentInteractables(unitController);
            if (validInteractables.Count == 1) {
                int firstInteractable = validInteractables.Take(1).Select(d => d.Key).First();
                InteractWithOptionInternal(unitController, triggerInteractable, validInteractables[firstInteractable], firstInteractable, 0);
            }
        }

        public void InteractWithOptionClient(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                InteractWithOptionInternal(sourceUnitController, targetInteractable, interactableOptionComponent, componentIndex, choiceIndex);
            } else {
                networkManagerClient.InteractWithOption(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
            }
        }

        public void InteractWithOptionServer(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            Debug.Log($"InteractionManager.InteractWithOptionServer({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            Dictionary<int, InteractableOptionComponent> interactionOptions = targetInteractable.GetCurrentInteractables(sourceUnitController);
            if (interactionOptions.ContainsKey(componentIndex)) {
                InteractWithOptionInternal(sourceUnitController, targetInteractable, interactionOptions[componentIndex], componentIndex, choiceIndex);
            }
        }

        public void InteractWithOptionInternal(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            Debug.Log($"InteractionManager.InteractWithOptionInternal({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            interactableOptionComponent.Interact(sourceUnitController, componentIndex, choiceIndex);
        }

        public void OpenInteractionWindow(Interactable targetInteractable) {
            //Debug.Log($"{gameObject.name}.Interactable.OpenInteractionWindow");
            BeginInteraction(targetInteractable);
            uIManager.craftingWindow.CloseWindow();
            uIManager.interactionWindow.OpenWindow();
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

        /*
        // since interactions are server authoritative this always happens on the server
        public void InteractWithClassChangeComponent(UnitController sourceUnitController, ClassChangeComponent classChangeComponent, int optionIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                InteractWithClassChangeComponentInternal(classChangeComponent);
            } else if (networkManagerServer.ServerModeActive) {
                if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                    networkManagerServer.AdvertiseInteractWithClassChangeComponent(playerManagerServer.ActivePlayerLookup[sourceUnitController], classChangeComponent.Interactable, optionIndex);
                }
            }
        }

        public void InteractWithClassChangeComponentClient(Interactable interactable, int optionIndex) {
            Debug.Log($"InteractionManager.InteractWithClassChangeComponentClient({interactable.gameObject.name}, {optionIndex})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(playerManager.UnitController);
            if (currentInteractables.ContainsKey(optionIndex)) {
                if (currentInteractables[optionIndex] is ClassChangeComponent) {
                    InteractWithClassChangeComponentInternal(currentInteractables[optionIndex] as ClassChangeComponent);
                }
            }
        }

        public void InteractWithClassChangeComponentInternal(ClassChangeComponent classChangeComponent) {
            Debug.Log($"InteractionManager.InteractWithClassChangeComponentInternal()");
            
            classChangeManager.SetDisplayClass(classChangeComponent.Props.CharacterClass, classChangeComponent);

            uIManager.classChangeWindow.OpenWindow();
        }
        */

        /*
        public void InteractWithQuestGiver(QuestGiverComponent questGiverComponent, int optionIndex, UnitController sourceUnitController) {
            if (systemGameManager.GameMode == GameMode.Local) {
                InteractWithQuestGiverInternal(questGiverComponent, optionIndex, sourceUnitController);
            } else if (networkManagerServer.ServerModeActive) {
                networkManagerServer.AdvertiseInteractWithQuestGiver(questGiverComponent.Interactable, optionIndex, sourceUnitController);
                return;
            }
        }

        public void InteractWithQuestGiverClient(Interactable interactable, int optionIndex, UnitController sourceUnitController) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if ((currentInteractables[optionIndex] as QuestGiverComponent) is QuestGiverComponent) {
                InteractWithQuestGiverInternal(currentInteractables[optionIndex] as QuestGiverComponent, optionIndex, sourceUnitController);
            }
        }

        public void InteractWithQuestGiverInternal(QuestGiverComponent questGiverComponent, int optionIndex, UnitController sourceUnitController) {
            // this is running locally
            if (sourceUnitController.CharacterQuestLog.GetCompleteQuests(questGiverComponent.Props.Quests, true).Count + sourceUnitController.CharacterQuestLog.GetAvailableQuests(questGiverComponent.Props.Quests).Count > 1) {
                OpenInteractionWindow(questGiverComponent.Interactable);
                return;
            } else if (sourceUnitController.CharacterQuestLog.GetAvailableQuests(questGiverComponent.Props.Quests).Count == 1 && sourceUnitController.CharacterQuestLog.GetCompleteQuests(questGiverComponent.Props.Quests).Count == 0) {
                if (sourceUnitController.CharacterQuestLog.GetAvailableQuests(questGiverComponent.Props.Quests)[0].HasOpeningDialog == true && sourceUnitController.CharacterQuestLog.GetAvailableQuests(questGiverComponent.Props.Quests)[0].OpeningDialog.TurnedIn(sourceUnitController) == false) {
                    dialogManager.SetQuestDialog(sourceUnitController.CharacterQuestLog.GetAvailableQuests(questGiverComponent.Props.Quests)[0], questGiverComponent.Interactable, questGiverComponent);
                    uIManager.dialogWindow.OpenWindow();
                    return;
                } else {
                    // do nothing will skip to below and open questlog to the available quest
                }
            }
            // we got here: we only have a single complete quest, or a single available quest with the opening dialog competed already
            if (!uIManager.questGiverWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                sourceUnitController.CharacterQuestLog.ShowQuestGiverDescription(sourceUnitController.CharacterQuestLog.GetAvailableQuests(questGiverComponent.Props.Quests).Union(sourceUnitController.CharacterQuestLog.GetCompleteQuests(questGiverComponent.Props.Quests)).ToList()[0], questGiverComponent);
                return;
            }
        }
        */

        /*
        // since interactions are server authoritative this always happens on the server
        internal void InteractWithSkillTrainerComponent(UnitController sourceUnitController, SkillTrainerComponent skillTrainerComponent, int optionIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                InteractWithSkillTrainerComponentInternal(skillTrainerComponent);
            } else if (networkManagerServer.ServerModeActive) {
                if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                    networkManagerServer.AdvertiseInteractWithSkillTrainerComponent(playerManagerServer.ActivePlayerLookup[sourceUnitController], skillTrainerComponent.Interactable, optionIndex);
                }
            }
        }

        public void InteractWithSkillTrainerComponentClient(Interactable interactable, int optionIndex) {
            Debug.Log($"InteractionManager.InteractWithSkillTrainerComponentClient({interactable.gameObject.name}, {optionIndex})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(playerManager.UnitController);
            if (currentInteractables.ContainsKey(optionIndex)) {
                if (currentInteractables[optionIndex] is SkillTrainerComponent) {
                    InteractWithSkillTrainerComponentInternal(currentInteractables[optionIndex] as SkillTrainerComponent);
                }
            }
        }
        */

    }
}
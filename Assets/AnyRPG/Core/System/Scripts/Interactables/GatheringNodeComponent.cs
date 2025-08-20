using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GatheringNodeComponent : LootableNodeComponent {

        //private bool available = true;

        public GatheringNodeProps GatheringNodeProps { get => interactableOptionProps as GatheringNodeProps; }

        public GatheringNodeComponent(Interactable interactable, GatheringNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                return base.PrerequisitesMet(sourceUnitController);
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnAbilityListChanged += HandleAbilityListChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            if (systemEventManager != null) {
                systemEventManager.OnAbilityListChanged -= HandleAbilityListChange;
            }
        }

        public void HandleAbilityListChange(UnitController sourceUnitController, AbilityProperties baseAbility) {
            //Debug.Log($"{gameObject.name}.GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public static GatheringNodeComponent GetGatheringNodeComponent(Interactable searchInteractable) {
            if (searchInteractable == null) {
                return null;
            }
            return searchInteractable.GetFirstInteractableOption(typeof(GatheringNodeComponent)) as GatheringNodeComponent;
        }

        public override string GetInteractionButtonText(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            return (GatheringNodeProps.BaseAbility != null ? GatheringNodeProps.BaseAbility.DisplayName : base.GetInteractionButtonText(sourceUnitController, componentIndex, choiceIndex));
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.GatheringNode.Interact(" + source.name + ")");
            if (Props.LootTables == null) {
                //Debug.Log($"{gameObject.name}.GatheringNode.Interact(" + source.name + "): lootTable was null!");
                return true;
            }
            // base.Interact() will drop loot automatically so we will intentionally not call it because the loot drop in this class is activated by the gatherability
            if (lootDropped == true) {
                // this call is safe, it will internally check if loot is already dropped and just pickup instead
                Gather(sourceUnitController, componentIndex);
            } else {
                sourceUnitController.CharacterAbilityManager.BeginAbility(GatheringNodeProps.BaseAbility.AbilityProperties, interactable);
            }
            return true;
            //return PickUp();
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.interactionWindow.CloseWindow();

        }

        public void Gather(UnitController sourceUnitController, int componentIndex = 0, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.GatheringNode.DropLoot()");
            base.Interact(sourceUnitController, componentIndex, choiceIndex);
        }

        /*
        public override void DropLoot() {
            Debug.Log(gameObject.name + ".GatheringNode.DropLoot()");
            base.Interact(playerManager.UnitController.CharacterUnit);
            //base.DropLoot();
            //PickUp();
        }
        */

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.GatheringNode.GetCurrentOptionCount()");
            return ((sourceUnitController.CharacterAbilityManager.HasAbility(GatheringNodeProps.BaseAbility.AbilityProperties) == true
                && currentTimer <= 0f) ? 1 : 0);
        }

        /*

        public override bool CanInteract(CharacterUnit source) {
            bool returnValue = base.CanInteract(source);
            if (returnValue == false) {
                return false;
            }
            return (GetCurrentOptionCount() == 0 ? false : true);
        }
        */

    }

}
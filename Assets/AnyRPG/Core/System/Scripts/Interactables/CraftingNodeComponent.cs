using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNodeComponent : InteractableOptionComponent {

        // game manager references
        private CraftingManager craftingManager = null;

        public CraftingNodeProps Props { get => interactableOptionProps as CraftingNodeProps; }

        public CraftingNodeComponent(Interactable interactable, CraftingNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
        }

        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.CharacterAbilityManager.HasAbility(Props.Ability) == false) {
                    return false;
                }
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

            systemEventManager.OnAbilityListChanged -= HandleAbilityListChange;
        }

        public static List<CraftingNodeComponent> GetCraftingNodeComponents(Interactable searchInteractable) {
            if (searchInteractable == null) {
                return new List<CraftingNodeComponent>();
            }
            return searchInteractable.GetInteractableOptionList(typeof(CraftingNodeComponent)).Cast<CraftingNodeComponent>().ToList();
        }

        public void HandleAbilityListChange(UnitController sourceUnitController, AbilityProperties baseAbility) {
            //Debug.Log($"{gameObject.name}.GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.GatheringNode.GetCurrentOptionCount()");
            return ((sourceUnitController.CharacterAbilityManager.HasAbility(Props.Ability) == true) ? 1 : 0);
        }

        public override bool Interact(UnitController sourceUnitController, int optionIndex) {
            base.Interact(sourceUnitController, optionIndex);

            if (Props == null || Props.Ability == null) {
                Debug.Log("Props is null");
            }
            craftingManager.SetAbility(Props.Ability);
            //source.MyCharacter.MyCharacterAbilityManager.BeginAbility(ability);
            return true;
            //return PickUp();
        }

        public override void StopInteract() {
            base.StopInteract();

            uIManager.craftingWindow.CloseWindow();
        }

    }

}
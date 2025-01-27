using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainerComponent : InteractableOptionComponent {

        // game manager references
        private SkillTrainerManager skillTrainerManager = null;

        public SkillTrainerProps Props { get => interactableOptionProps as SkillTrainerProps; }

        public SkillTrainerComponent(Interactable interactable, SkillTrainerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Train Me";
            }
            systemEventManager.OnSkillListChanged += HandleSkillListChanged;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            skillTrainerManager = systemGameManager.SkillTrainerManager;
        }

        public override bool Interact(UnitController source, int optionIndex) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.Interact(" + source + ")");
            base.Interact(source, optionIndex);
            if (!uIManager.skillTrainerWindow.IsOpen) {
                skillTrainerManager.SetSkillTrainer(this);
                uIManager.skillTrainerWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.SkillTrainer.StopInteract()");
            base.StopInteract();
            uIManager.skillTrainerWindow.CloseWindow();
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.SkillTrainer.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            if (systemEventManager != null) {
                systemEventManager.OnSkillListChanged -= HandleSkillListChanged;
            }
        }

        public void HandleSkillListChanged(UnitController sourceUnitController, Skill skill) {
            // this is a special case.  since skill is not a prerequisites, we need to subscribe directly to the event to get notified things have changed
            if (Props.Skills.Contains(skill)) {
                HandlePrerequisiteUpdates(sourceUnitController);
            }
        }

        public override int GetValidOptionCount(UnitController sourceUnitController) {
            if (base.GetValidOptionCount(sourceUnitController) == 0) {
                return 0;
            }
            return GetCurrentOptionCount(sourceUnitController);
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.SkillTrainerInteractable.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            int optionCount = 0;
            foreach (Skill skill in Props.Skills) {
                if (!sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                    optionCount++;
                }
            }
            //Debug.Log($"{gameObject.name}.SkillTrainerInteractable.GetCurrentOptionCount(); return: " + optionCount);
            // testing - having the actual skill count causes multiple interaction window items
            // return 1 for anything other than no skills
            return (optionCount == 0 ? 0 : 1);
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck = false, bool passedRangeCheck = false, bool processNonCombatCheck = true) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.CanInteract()");
            bool returnValue = ((GetCurrentOptionCount(sourceUnitController) > 0 && base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck)) ? true : false);
            //Debug.Log($"{gameObject.name}.SkillTrainer.CanInteract(): return: " + returnValue);
            return returnValue;
        }

        public override bool CanShowMiniMapIcon(UnitController sourceUnitController) {
            float relationValue = interactable.PerformFactionCheck(sourceUnitController);
            return CanInteract(sourceUnitController, false, false);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}
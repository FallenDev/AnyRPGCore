using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeComponent : InteractableOptionComponent {

        // game manager references
        private SpecializationChangeManager specializationChangeManager = null;

        public SpecializationChangeProps Props { get => interactableOptionProps as SpecializationChangeProps; }

        public SpecializationChangeComponent(Interactable interactable, SpecializationChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.ClassSpecialization.DisplayName + " Specialization";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            specializationChangeManager = systemGameManager.SpecializationChangeManager;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            // because the class is a special type of prerequisite, we need to be notified when it changes
            systemEventManager.OnSpecializationChange += HandleSpecializationChange;
            systemEventManager.OnClassChange += HandleClassChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            if (systemEventManager != null) {
                systemEventManager.OnSpecializationChange -= HandleSpecializationChange;
                systemEventManager.OnClassChange -= HandleClassChange;
            }
        }

        public override bool Interact(UnitController source, int optionIndex) {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.Interact()");
            base.Interact(source, optionIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int optionIndex) {
            base.ClientInteraction(sourceUnitController, optionIndex);

            specializationChangeManager.SetDisplaySpecialization(Props.ClassSpecialization, this, optionIndex);
            uIManager.specializationChangeWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.specializationChangeWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        public void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        // specialization is a special type of prerequisite
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.BaseCharacter.ClassSpecialization == Props.ClassSpecialization) {
                    return false;
                }
                if (Props.ClassSpecialization.CharacterClasses.Contains(sourceUnitController.BaseCharacter.CharacterClass) == false) {
                    return false;
                }
                return base.PrerequisitesMet(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}



    }

}
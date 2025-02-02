using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeComponent : InteractableOptionComponent {

        // game manager references
        private ClassChangeManager classChangeManager = null;

        public ClassChangeProps Props { get => interactableOptionProps as ClassChangeProps; }

        public ClassChangeComponent(Interactable interactable, ClassChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = Props.CharacterClass.DisplayName + " Class";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            classChangeManager = systemGameManager.ClassChangeManager;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnClassChange += HandleClassChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            systemEventManager.OnClassChange -= HandleClassChange;
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public override bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.Interact()");
            base.Interact(sourceUnitController, componentIndex, choiceIndex);

            //interactionManager.InteractWithClassChangeComponent(sourceUnitController, this, optionIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            classChangeManager.SetDisplayClass(Props.CharacterClass, this, componentIndex, choiceIndex);

            uIManager.classChangeWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.classChangeWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        // character class is a special type of prerequisite
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.BaseCharacter.CharacterClass == Props.CharacterClass) {
                    return false;
                }
                return base.PrerequisitesMet(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}
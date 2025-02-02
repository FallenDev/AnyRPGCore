using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeManager : InteractableOptionManager {

        private CharacterClass characterClass = null;

        // game manager references
        private PlayerManager playerManager = null;

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void SetDisplayClass(CharacterClass characterClass, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            Debug.Log("ClassChangeManager.SetDisplayClass(" + characterClass + ")");

            this.characterClass = characterClass;

            BeginInteraction(interactableOptionComponent, componentIndex, choiceIndex);
        }

        public void ChangeCharacterClass() {
            
            playerManager.SetPlayerCharacterClass(characterClass);
            
            ConfirmAction(playerManager.UnitController);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            characterClass = null;
        }

    }

}
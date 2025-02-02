using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillTrainerManager : InteractableOptionManager {

        private SkillTrainerComponent skillTrainer = null;

        // game manager references
        private PlayerManager playerManager = null;

        public SkillTrainerComponent SkillTrainer { get => skillTrainer; set => skillTrainer = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public List<Skill> GetAvailableSkillList(UnitController sourceUnitController) {
            List<Skill> returnList = new List<Skill>();

            foreach (Skill skill in skillTrainer.Props.Skills) {
                if (!sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                    returnList.Add(skill);
                }
            }

            return returnList;
        }

        public void LearnSkill(UnitController sourceUnitController, Skill skill) {

            playerManager.LearnSkill(skill);

            ConfirmAction(sourceUnitController);
        }

        public void UnlearnSkill(UnitController sourceUnitController, Skill skill) {
            sourceUnitController.CharacterSkillManager.UnLearnSkill(skill);
        }

        public bool SkillIsKnown(UnitController sourceUnitController, Skill skill) {
            return sourceUnitController.CharacterSkillManager.HasSkill(skill);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            skillTrainer = null;
        }

        public void SetSkillTrainer(SkillTrainerComponent skillTrainerComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("ClassChangeManager.SetDisplayClass(" + characterClass + ")");
            this.skillTrainer = skillTrainerComponent;
            
            BeginInteraction(skillTrainerComponent, componentIndex, choiceIndex);
        }


    }

}
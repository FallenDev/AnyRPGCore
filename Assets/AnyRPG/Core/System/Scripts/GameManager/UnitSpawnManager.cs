using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnManager : InteractableOptionManager {

        private UnitSpawnControllerProps unitSpawnControllerProps = null;

        public UnitSpawnControllerProps UnitSpawnControllerProps { get => unitSpawnControllerProps; set => unitSpawnControllerProps = value; }

        // game manager references
        PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void SetProps(UnitSpawnControllerProps unitSpawnControllerProps, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("UnitSpawnManager.SetProps()");

            this.unitSpawnControllerProps = unitSpawnControllerProps;
            BeginInteraction(interactableOptionComponent, componentIndex, choiceIndex);
        }

        public void SpawnUnit(int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness) {
            foreach (UnitSpawnNode unitSpawnNode in unitSpawnControllerProps.UnitSpawnNodeList) {
                if (unitSpawnNode != null) {
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
                }
            }
            ConfirmAction(playerManager.UnitController);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            unitSpawnControllerProps = null;
        }


    }

}
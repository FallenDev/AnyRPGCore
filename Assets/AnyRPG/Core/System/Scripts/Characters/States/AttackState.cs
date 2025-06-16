using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AttackState : IState {
        private UnitController baseUnitController;

        public void Enter(UnitController baseController) {
            //Debug.Log(baseController.gameObject.name + ".AttackState.Enter()");
            this.baseUnitController = baseController;
            this.baseUnitController.UnitMotor.StopFollowingTarget();
        }

        public void Exit() {
            //Debug.Log(baseController.gameObject.name + ".AttackState.Exit()");
        }

        public void Update() {
            //Debug.Log($"{baseUnitController.gameObject.name}.AttackState.Update()");


            baseUnitController.UpdateTarget();

            if (baseUnitController.Target == null) {
                //Debug.Log(aiController.gameObject.name + ": about to change to returnstate");
                baseUnitController.ChangeState(new ReturnState());
                return;
            }

            if (baseUnitController.CharacterAbilityManager.PerformingAnyAbility() == true) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update() WaitingForAnimatedAbility is true");
                // nothing to do, other attack or ability in progress
                return;
            }

            // face target before attack to ensure they are in the hitbox
            baseUnitController.UnitMotor.FaceTarget(baseUnitController.Target);

            if (baseUnitController.CanGetValidAttack(true)) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update(): got valid ability");
                return;
            }

            // no valid ability found, try auto attack range check
            if (!baseUnitController.IsTargetInHitBox(baseUnitController.Target)) {
                // target is out of range, follow it
                baseUnitController.ChangeState(new FollowState());
                return;
            }

            // target is in range, attack it
            //aiController.AttackCombatTarget();

        }

    }

}
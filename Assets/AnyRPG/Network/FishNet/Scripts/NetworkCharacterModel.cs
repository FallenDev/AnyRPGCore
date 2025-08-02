using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class NetworkCharacterModel : SpawnedNetworkObject {

        private SystemGameManager systemGameManager = null;
        private NetworkAnimator networkAnimator = null;
        private UnitController unitController = null;
        private Animator animator = null;

        private void FindGameManager() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.FindGameManager()");

            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            networkAnimator = GetComponent<NetworkAnimator>();
            unitController = GetComponentInParent<UnitController>();
            animator = GetComponent<Animator>();
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.FindGameManager(): animator: {animator.GetInstanceID()}");
            unitController.UnitEventController.OnInitializeAnimator += HandleInitializeAnimator;
            //unitController.UnitModelController.OnModelCreated += HandleModelCreated;

            // clients are authoritative for their own animators, and server is authoritative for all others
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.FindGameManager(): IsOwner: {base.IsOwner}, OwnerId: {base.OwnerId}, ServerModeActive: {systemGameManager.NetworkManagerServer.ServerModeActive}");
            if (base.IsOwner || (systemGameManager.NetworkManagerServer.ServerModeActive == true && base.OwnerId == -1)) {
                unitController.UnitEventController.OnAnimatorSetTrigger += HandleSetTrigger;
                unitController.UnitEventController.OnAnimatorResetTrigger += HandleResetTrigger;
            }
        }

        /*
        private void HandleModelCreated() {
            animator = GetComponent<Animator>();
            Debug.Log($"{gameObject.name}.NetworkCharacterModel.HandleModelCreated(): animator: {animator.GetInstanceID()}");

            networkAnimator.SetAnimator(animator);
        }
        */

        private void HandleSetTrigger(string triggerName) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.HandleSetTrigger({triggerName})");

            networkAnimator.SetTrigger(triggerName);
        }

        private void HandleResetTrigger(string triggerName) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.HandleResetTrigger({triggerName})");

            networkAnimator.ResetTrigger(triggerName);
        }

        private void HandleInitializeAnimator() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.HandleInitializeAnimator()");

            networkAnimator.SetAnimator(animator);
        }

        private void CompleteModelRequest(bool isOwner) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.CompleteModelRequest() isOwner: {isOwner}");

            systemGameManager.CharacterManager.CompleteNetworkModelRequest(unitController, gameObject, base.OwnerId == -1);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.OnStartClient()");

            FindGameManager();
            if (systemGameManager == null) {
                return;
            }
            if (unitController.CharacterConfigured == true) {
                CompleteModelRequest(base.IsOwner);
            } else {
                SubscribeToUnitConfigured();
            }

        }


        public override void OnStartServer() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.OnStartServer()");

            FindGameManager();
            if (systemGameManager == null) {
                return;
            }

            if (unitController.CharacterConfigured == true) {
                CompleteModelRequest(base.OwnerId == -1);
            } else {
                SubscribeToUnitConfigured();
            }
        }

        private void SubscribeToUnitConfigured() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.SubscribeToUnitConfigured()");

            unitController.UnitEventController.OnCharacterConfigured += HandleCharacterConfigured;
        }

        private void HandleCharacterConfigured() {
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.HandleCharacterConfigured()");

            unitController.UnitEventController.OnCharacterConfigured -= HandleCharacterConfigured;
            if (systemGameManager.NetworkManagerServer.ServerModeActive == false) {
                CompleteModelRequest(base.IsOwner);
            } else {
                CompleteModelRequest(base.OwnerId == -1);
            }
        }

        /*
        [ServerRpc(RequireOwnership = false)]
        public void GetClientSaveData(NetworkConnection networkConnection = null) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.GetClientSaveData()");

            PutClientSaveData(networkConnection, unitController.CharacterSaveManager.SaveData);
        }

        [TargetRpc]
        public void PutClientSaveData(NetworkConnection networkConnection, AnyRPGSaveData saveData) {
            CompleteModelRequest(base.IsOwner, saveData);
        }
        */


    }
}


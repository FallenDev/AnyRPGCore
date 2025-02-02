using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace AnyRPG {
    public class NetworkInteractable : SpawnedNetworkObject {

        private Interactable interactable = null;

        // game manager references
        protected SystemGameManager systemGameManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;

        public Interactable Interactable { get => interactable; }

        protected virtual void Awake() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.Awake() position: { gameObject.transform.position}");
        }

        protected virtual void Configure() {
            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            
            interactable = GetComponent<Interactable>();
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStartClient()");

            base.OnStartClient();

            Configure();
            if (systemGameManager == null) {
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            interactable.AutoConfigure(systemGameManager);

            SubscribeToClientInteractableEvents();
        }

        public override void OnStopClient() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStopClient()");

            base.OnStopClient();
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientInteractableEvents();
            //systemGameManager.NetworkManagerClient.ProcessStopClient(unitController);
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStartServer()");

            base.OnStartServer();

            Configure();
            if (systemGameManager == null) {
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            //interactable.AutoConfigure(systemGameManager);

            SubscribeToServerInteractableEvents();
        }

        public override void OnStopServer() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStopServer()");

            base.OnStopServer();

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            UnsubscribeFromServerInteractableEvents();
            //systemGameManager.NetworkManagerServer.ProcessStopServer(unitController);
        }

        public void SubscribeToServerInteractableEvents() {
            if (interactable == null) {
                // something went wrong
                return;
            }

            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement += HandleAnimatedObjectChooseMovementServer;
            interactable.OnInteractionWithOptionStarted += HandleInteractionWithOptionStarted;
            interactable.InteractableEventController.OnPlayDialogNode += HandlePlayDialogNode;
        }

        public void UnsubscribeFromServerInteractableEvents() {
            if (interactable == null) {
                return;
            }
            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement -= HandleAnimatedObjectChooseMovementServer;
            interactable.OnInteractionWithOptionStarted -= HandleInteractionWithOptionStarted;
            interactable.InteractableEventController.OnPlayDialogNode += HandlePlayDialogNode;
        }

        public void SubscribeToClientInteractableEvents() {
            if (interactable == null) {
                // something went wrong
                return;
            }

            //unitController.UnitEventController.OnBeginChatMessage += HandleBeginChatMessageServer;
        }

        public void UnsubscribeFromClientInteractableEvents() {
            if (interactable == null) {
                return;
            }
            //unitController.UnitEventController.OnBeginChatMessage -= HandleBeginChatMessageServer;
        }

        [ObserversRpc]
        public void HandlePlayDialogNode(string dialogName, int dialogIndex) {
            
            interactable.DialogController.PlayDialogNode(dialogName, dialogIndex);
        }


        /*
        public void HandleAnimatedObjectChooseMovementServer(UnitController sourceUnitController, int optionIndex) {
            
            NetworkCharacterUnit targetNetworkCharacterUnit = null;
            if (sourceUnitController != null) {
                targetNetworkCharacterUnit = sourceUnitController.GetComponent<NetworkCharacterUnit>();
            }
            HandleAnimatedObjectChooseMovementClient(targetNetworkCharacterUnit, optionIndex);
        }

        [ObserversRpc]
        public void HandleAnimatedObjectChooseMovementClient(NetworkCharacterUnit sourceNetworkCharacterUnit, int optionIndex) {
            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit != null) {
                sourceUnitController = sourceNetworkCharacterUnit.UnitController;
            }

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables.ContainsKey(optionIndex)) {
                if (currentInteractables[optionIndex] is AnimatedObjectComponent) {
                    (currentInteractables[optionIndex] as AnimatedObjectComponent).ChooseMovement(sourceUnitController, optionIndex);
                }
            }
        }
        */

        public void HandleInteractionWithOptionStarted(UnitController sourceUnitController, int componentIndex, int choiceIndex) {

            NetworkCharacterUnit targetNetworkCharacterUnit = null;
            if (sourceUnitController != null) {
                targetNetworkCharacterUnit = sourceUnitController.GetComponent<NetworkCharacterUnit>();
            }
            HandleInteractionWithOptionStartedClient(targetNetworkCharacterUnit, componentIndex, choiceIndex);
        }

        [ObserversRpc]
        public void HandleInteractionWithOptionStartedClient(NetworkCharacterUnit sourceNetworkCharacterUnit, int componentIndex, int choiceIndex) {
            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit != null) {
                sourceUnitController = sourceNetworkCharacterUnit.UnitController;
            }

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables.ContainsKey(componentIndex)) {
                currentInteractables[componentIndex].Interact(sourceUnitController, componentIndex, choiceIndex);
            }
        }



    }
}


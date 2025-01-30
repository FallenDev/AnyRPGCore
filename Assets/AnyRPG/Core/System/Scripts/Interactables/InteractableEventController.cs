using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AnyRPG {

    public class InteractableEventController : ConfiguredClass {

        //public event System.Action<UnitController, int> OnAnimatedObjectChooseMovement = delegate { };
        //public event System.Action<UnitController, int> OnInteractionWithOptionStarted = delegate { };

        // interactable this controller is attached to
        private Interactable Interactable;

        public InteractableEventController(Interactable interactable, SystemGameManager systemGameManager) {
            this.Interactable = interactable;
            Configure(systemGameManager);
        }

        #region EventNotifications

        // temporarily disabled because this object is not created early enough in the process when its a unitcontroller

        //public void NotifyOnAnimatedObjectChooseMovement(UnitController sourceUnitController, int optionIndex) {
        //    OnAnimatedObjectChooseMovement(sourceUnitController, optionIndex);
        //}

        //public void NotifyOnInteractionWithOptionStarted(UnitController sourceUnitController, int optionIndex) {
        //    OnInteractionWithOptionStarted(sourceUnitController, optionIndex);
        //}

        #endregion


    }

}
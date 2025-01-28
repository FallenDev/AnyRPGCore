using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOptionComponent : ConfiguredClass, IPrerequisiteOwner {

        protected Interactable interactable = null;
        protected InteractableOptionProps interactableOptionProps = null;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected SystemEventManager systemEventManager = null;
        protected PlayerManager playerManager = null;
        protected PlayerManagerServer playerManagerServer = null;
        protected UIManager uIManager = null;
        protected InteractionManager interactionManager = null;

        public Interactable Interactable { get => interactable; set => interactable = value; }
        public virtual InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
        public virtual int PriorityValue { get => 0; }
        public virtual string DisplayName {
            get {
                if (interactableOptionProps.GetInteractionPanelTitle() != null && interactableOptionProps.GetInteractionPanelTitle() != string.Empty) {
                    return interactableOptionProps.GetInteractionPanelTitle();
                }
                if (interactable != null) {
                    return interactable.DisplayName;
                }
                return "interactable is null!";
            }
        }

        public virtual bool PrerequisitesMet(UnitController sourceUnitController) {
                //Debug.Log($"{gameObject.name}.InteractableOption.MyPrerequisitesMet");
                foreach (PrerequisiteConditions prerequisiteCondition in interactableOptionProps.PrerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
        }

        public InteractableOptionComponent(Interactable interactable, InteractableOptionProps interactableOptionProps, SystemGameManager systemGameManager) {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent(" + interactable.gameObject.name + ", " + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");
            this.interactable = interactable;
            this.interactableOptionProps = interactableOptionProps;
            Configure(systemGameManager);
            SetupScriptableObjects();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.SetGameManagerReferences");
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            interactionManager = systemGameManager.InteractionManager;
        }

        /*
        public virtual void Init() {
            //AddUnitProfileSettings();
            CreateEventSubscriptions();
        }
        */

        public virtual void Cleanup() {
            CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        /*
        protected virtual void AddUnitProfileSettings() {
            // do nothing here
        }
        */

        public virtual void ProcessStatusIndicatorSourceInit() {
        }

        public void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public virtual void ProcessCreateEventSubscriptions() {
        }

        public void CleanupEventSubscriptions() {
            /*
            if (!eventSubscriptionsInitialized) {
                return;
            }
            */
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        public virtual void ProcessCleanupEventSubscriptions() {
        }

        public virtual void NotifyOnConfirmAction(UnitController sourceUnitController) {
            systemEventManager.NotifyOnInteractionWithOptionCompleted(sourceUnitController, this);
        }

        public virtual bool ProcessFactionValue(float factionValue) {
            return (factionValue >= 0f ? true : false);
        }

        public virtual bool ProcessCombatOnly() {
            if (interactable.CombatOnly == true) {
                return false;
            }
            return true;
        }

        public virtual bool NonCombatOptionsAvailable() {
            if (interactable.NonCombatOptionsAvailable == false) {
                return false;
            }
            return true;
        }

        public virtual bool CanInteract(UnitController sourceUnitController, bool processRangeCheck = false, bool passedRangeCheck = false, bool processNonCombatCheck = true) {
            //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOptionComponent.CanInteract(" + processRangeCheck + ", " + passedRangeCheck + ", " + factionValue + ")");
            if (processRangeCheck == true && passedRangeCheck == false) {
                //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact(): range check failed");
                return false;
            }
            if (ProcessCombatOnly() == false) {
                //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact(): combatOnly check failed");
                return false;
            }
            if (processNonCombatCheck == true && NonCombatOptionsAvailable() == false) {
                return false;
            }

            bool returnValue = PrerequisitesMet(sourceUnitController);
            if (returnValue == false) {
                //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOptionComponent.Interact(): prerequisites not met");
            }
            return returnValue;
        }

        public virtual bool Interact(UnitController sourceUnitController, int optionIndex) {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact()");
            //source.CancelMountEffects();
            systemEventManager.NotifyOnInteractionWithOptionStarted(sourceUnitController, this);
            return true;
        }

        public virtual void StopInteract() {
            //Debug.Log($"{gameObject.name}.InanimateUnit.StopInteract()");
            playerManager.PlayerController.StopInteract();
        }

        public virtual void ProcessStartInteract() {
            interactable.ProcessStartInteractWithOption(this);
        }

        public virtual void ProcessStopInteract() {
            interactable.ProcessStopInteractWithOption(this);
        }

        public virtual bool PlayInteractionSound() {
            return false;
        }

        public virtual AudioClip GetInteractionSound(VoiceProps voiceProps) {
            return voiceProps.RandomStartInteract;
        }


        public virtual bool HasMiniMapText() {
            return false;
        }

        public virtual bool HasMainMapText() {
            return false;
        }

        public virtual bool HasMiniMapIcon() {
            return (interactableOptionProps.NamePlateImage != null);
        }

        public virtual bool HasMainMapIcon() {
            return false;
        }

        public virtual bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.SetMiniMapText()");

            return (GetCurrentOptionCount(playerManager.UnitController) > 0);
        }

        public virtual void SetMiniMapIcon(Image icon) {
            //Debug.Log($"{gameObject.name}.InteractableOption.SetMiniMapIcon()");
            if (CanShowMiniMapIcon(playerManager.UnitController)) {
                icon.sprite = GetMiniMapIcon();
                icon.color = GetMiniMapIconColor();
            } else {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
            }
            return;
        }

        public virtual Sprite GetMiniMapIcon() {
            return interactableOptionProps.NamePlateImage;
        }

        public virtual Color GetMiniMapIconColor() {
            return Color.white;
        }

        public virtual bool CanShowMiniMapIcon(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.InteractableOption.CanShowMiniMapIcon()");
            return (GetCurrentOptionCount(sourceUnitController) > 0);
        }

        public virtual string GetDescription() {
            return string.Format("<color=#ffff00ff>{0}</color>", GetSummary());
        }

        public virtual string GetSummary() {
            return string.Format("{0}", interactableOptionProps.GetInteractionPanelTitle());
        }
        

        public virtual void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".InteractableOption.HandlePlayerUnitSpawn()");

            if (interactableOptionProps.PrerequisiteConditions != null && interactableOptionProps.PrerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(sourceUnitController, false);
                    }
                }
                /*
                if (MyPrerequisitesMet) {
                    HandlePrerequisiteUpdates();
                }
                */
            } else {
                //HandlePrerequisiteUpdates();
            }
            //HandlePrerequisiteUpdates();

            CallMiniMapStatusUpdateHandler();
        }


        public virtual int GetValidOptionCount(UnitController sourceUnitController) {
            // overwrite me if this type of interactable option has a list of options instead of just one
            /*
            if (processRangeCheck == true && passedRangeCheck == false) {
                return 0;
            }
            */
            if (interactable.CombatOnly) {
                return 0;
            }
            return (PrerequisitesMet(sourceUnitController) == true ? 1 : 0);
        }

        public virtual int GetCurrentOptionCount(UnitController sourceUnitController) {
            // overwrite me or everything is valid as long as prerequisites are met, which isn't the case for things like dialog, which have multiple options
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            return GetValidOptionCount(sourceUnitController);
        }

        public virtual void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOption.HandlePrerequisiteUpdates()");
            HandleOptionStateChange();
        }

        /// <summary>
        /// trigger to update minimap
        /// </summary>
        public void HandleOptionStateChange() {
            if (interactable != null) {
                interactable.HandlePrerequisiteUpdates();
            }
            CallMiniMapStatusUpdateHandler();
        }

        public void CallMiniMapStatusUpdateHandler() {
            interactable?.HandleMiniMapStatusUpdate(this);
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.SetupScriptableObjects()");
            if (interactableOptionProps.PrerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);
                    }
                }
            }

            //interactableOptionProps.SetupScriptableObjects();
        }

        public virtual void CleanupScriptableObjects() {
            if (interactableOptionProps.PrerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
                    }
                }
            }

        }


    }

}
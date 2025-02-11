using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterEquipmentManager : EquipmentManager {


        // component references
        protected UnitController unitController = null;

        //protected EquipmentManager equipmentManager = null;

        // keep track of holdable objects to be used during weapon attack animations such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponAbilityAnimationObjects = new List<AbilityAttachmentNode>();

        // keep track of holdable objects to be used during weapon attacks such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponAbilityObjects = new List<AbilityAttachmentNode>();

        // game manager references
        SystemItemManager systemItemManager = null;

        //public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => equipmentManager.CurrentEquipment; set => equipmentManager.CurrentEquipment = value; }
        public List<AbilityAttachmentNode> WeaponAbilityAnimationObjects { get => weaponAbilityAnimationObjects; }
        public List<AbilityAttachmentNode> WeaponAbilityObjects { get => weaponAbilityObjects; }

        public CharacterEquipmentManager(UnitController unitController, SystemGameManager systemGameManager) : base(systemGameManager) {
            this.unitController = unitController;
            //Configure(systemGameManager);

            //equipmentManager = new EquipmentManager(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemItemManager = systemGameManager.SystemItemManager;
        }

        public void HandleCapabilityConsumerChange() {
            List<InstantiatedEquipment> equipmentToRemove = new List<InstantiatedEquipment>();
            foreach (InstantiatedEquipment instantiatedEquipment in CurrentEquipment.Values) {
                if (instantiatedEquipment != null && instantiatedEquipment.Equipment.CanEquip(instantiatedEquipment.GetItemLevel(unitController.CharacterStats.Level), unitController) == false) {
                    equipmentToRemove.Add(instantiatedEquipment);
                }
            }
            if (equipmentToRemove.Count > 0) {
                foreach (InstantiatedEquipment equipment in equipmentToRemove) {
                    Unequip(equipment);
                }
                unitController.UnitModelController.RebuildModelAppearance();
            }

            // since all status effects were cancelled on the change, it is necessary to re-apply set bonuses
            foreach (InstantiatedEquipment instantiatedEquipment in CurrentEquipment.Values) {
                if (instantiatedEquipment != null) {
                    unitController.CharacterAbilityManager.UpdateEquipmentTraits(instantiatedEquipment);
                }
            }
        }

        public float GetWeaponDamage() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in CurrentEquipment.Keys) {
                if (CurrentEquipment[equipmentSlotProfile] != null && CurrentEquipment[equipmentSlotProfile].Equipment is Weapon) {
                    returnValue += (CurrentEquipment[equipmentSlotProfile].Equipment as Weapon).GetDamagePerSecond(unitController.CharacterStats.Level);
                }
            }
            return returnValue;
        }

        /// <summary>
        /// meant to be called by SetUnitProfile since it relies on that for the equipment list
        /// </summary>
        public void LoadDefaultEquipment(bool loadProviderEquipment) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment(" + loadProviderEquipment + ")");

            if (unitController?.UnitProfile?.EquipmentList == null) {
                return;
            }

            // load the unit profile equipment
            foreach (Equipment equipment in unitController.UnitProfile.EquipmentList) {
                if (equipment != null) {
                    Equip(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
            }

            if (loadProviderEquipment == false || unitController.UnitProfile.UseProviderEquipment == false) {
                return;
            }

            if (unitController.BaseCharacter.Faction != null) {
                foreach (Equipment equipment in unitController.BaseCharacter.Faction.EquipmentList) {
                    Equip(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
            }

            if (unitController.BaseCharacter.CharacterRace != null) {
                foreach (Equipment equipment in unitController.BaseCharacter.CharacterRace.EquipmentList) {
                    Equip(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
            }

            if (unitController.BaseCharacter.CharacterClass != null) {
                foreach (Equipment equipment in unitController.BaseCharacter.CharacterClass.EquipmentList) {
                    Equip(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
                if (unitController.BaseCharacter.ClassSpecialization != null) {
                    foreach (Equipment equipment in unitController.BaseCharacter.ClassSpecialization.EquipmentList) {
                        Equip(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                    }
                }
            }

        }

        public bool Equip(InstantiatedEquipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName) + ")");

            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return false;
            }

            if (newItem.Equipment.EquipmentSlotType == null) {
                Debug.LogError(unitController.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.Equipment.ResourceName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
                return false;
            }

            if (newItem.Equipment.CanEquip(newItem.GetItemLevel(unitController.CharacterStats.Level), unitController) == false) {
                //Debug.Log(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; could not equip");
                return false;
            }


            equipmentSlotProfile = base.EquipEquipment(newItem, equipmentSlotProfile);

            if (equipmentSlotProfile == null) {
                Debug.LogError(unitController.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.Equipment.ResourceName + " equipmentSlotProfile is null.  CHECK INSPECTOR.");
                return false;
            }

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            NotifyEquipmentChanged(newItem, null, -1, equipmentSlotProfile);

            //Debug.Log("CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; successfully equipped");

            return true;
        }

        public override void UnequipEquipment(EquipmentSlotProfile equipmentSlotProfile) {
            // intentionally not calling base
            // this override exists to intercept a list-only update and perform more character level log
            //base.UnequipFromList(equipmentSlotProfile);
            Unequip(equipmentSlotProfile);
        }

        public void RemoveWeaponAbilityAnimationObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // animation phase objects
            if (weaponAbilityAnimationObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityAnimationObjects.Remove(abilityAttachmentNode);
            }
        }

        public void RemoveWeaponAbilityObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // attack phase objects
            if (weaponAbilityObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityObjects.Remove(abilityAttachmentNode);
            }
        }

        public void AddWeaponAbilityAnimationObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // animation phase objects
            if (!weaponAbilityAnimationObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityAnimationObjects.Add(abilityAttachmentNode);
            }
        }

        public void AddWeaponAbilityObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // attack phase objects
            if (!weaponAbilityObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityObjects.Add(abilityAttachmentNode);
            }
        }

        public void HandleWeaponHoldableObjects(InstantiatedEquipment newItem, InstantiatedEquipment oldItem) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");

            oldItem?.Equipment.HandleUnequip(this);

            newItem?.Equipment.HandleEquip(this);
        }

        public void NotifyEquipmentChanged(InstantiatedEquipment newItem, InstantiatedEquipment oldItem, int slotIndex, EquipmentSlotProfile equipmentSlotProfile) {
            HandleWeaponHoldableObjects(newItem, oldItem);
            //OnEquipmentChanged(newItem, oldItem, slotIndex);
            unitController.CharacterStats.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            unitController.CharacterCombat.HandleEquipmentChanged(newItem, oldItem, slotIndex, equipmentSlotProfile);
            unitController.CharacterAbilityManager.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            unitController.UnitAnimator.HandleEquipmentChanged(newItem, oldItem, slotIndex);

            // now that all stats have been recalculated, it's safe to fire this event, so things that listen will show the correct values
            unitController.UnitEventController.NotifyOnEquipmentChanged(newItem, oldItem, slotIndex);
        }

        /*
        public int GetEquipmentSetCount(EquipmentSet equipmentSet) {
            return equipmentManager.GetEquipmentSetCount(equipmentSet);
        }

        /// <summary>
        /// return the equipment slot that a piece of equipment is currently equipped in, or null if not equipped
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        public EquipmentSlotProfile FindEquipmentSlotForEquipment(Equipment equipment) {
            return equipmentManager.FindEquipmentSlotForEquipment(equipment);
        }
        */

        public InstantiatedEquipment Unequip(InstantiatedEquipment instantiatedEquipment) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Unequip(" + (equipment == null ? "null" : equipment.DisplayName) + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");

            EquipmentSlotProfile equipmentSlotProfile = FindEquipmentSlotForEquipment(instantiatedEquipment);
            if (equipmentSlotProfile != null) {
                return Unequip(equipmentSlotProfile, -1);
            }
            return null;
        }

        public InstantiatedEquipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlot.ToString() + ", " + slotIndex + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");

            if (CurrentEquipment.ContainsKey(equipmentSlot) && CurrentEquipment[equipmentSlot] != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");

                InstantiatedEquipment oldItem = base.UnequipFromList(equipmentSlot);

                NotifyEquipmentChanged(null, oldItem, slotIndex, equipmentSlot);
                return oldItem;
            }
            return null;
        }

        public bool HasAffinity(WeaponSkill weaponAffinity) {
            //Debug.Log("EquipmentManager.HasAffinity(" + weaponAffinity.ToString() + ")");
            int weaponCount = 0;
            foreach (InstantiatedEquipment instantiatedEquipment in CurrentEquipment.Values) {
                if (instantiatedEquipment.Equipment is Weapon) {
                    weaponCount++;
                    if (weaponAffinity == (instantiatedEquipment.Equipment as Weapon).WeaponSkill) {
                        return true;
                    }
                }
            }
            if (weaponCount == 0) {
                // there are no weapons equipped
                // check if the character class is set and contains a weapon skill that is considered to be active when no weapon is equipped
                if (weaponAffinity.WeaponSkillProps.DefaultWeaponSkill && unitController.BaseCharacter.CapabilityConsumerProcessor.IsWeaponSkillSupported(weaponAffinity)) {
                    return true;
                }
            }
            return false;
        }

        /*
        void SetEquipmentBlendShapes(Equipment item, int weight) {
            foreach (EquipmentMeshRegion blendShape in item.coveredMeshRegions) {
                targetMesh.SetBlendShapeWeight((int)blendShape, weight);
            }
        }
        */

        /*
        public bool HasEquipment(string equipmentName, bool partialMatch = false) {
            return equipmentManager.HasEquipment(equipmentName, partialMatch);
        }

        public int GetEquipmentCount(string equipmentName, bool partialMatch = false) {
            return GetEquipmentCount(equipmentName, partialMatch);
        }
        */

    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Services
{
    /// <summary>
    ///     Service that manages weapon switching logic according to Adventure Island 3 mechanics
    ///     All weapons are always available - power-ups just switch between them
    /// </summary>
    public class WeaponManagerService : MonoBehaviour
    {

        [Header("Attack Settings")] [SerializeField]
        public bool canAttack;

        private Dictionary<WeaponType, IWeapon> _weaponMap;

        public WeaponType CurrentPrimaryWeapon { get; private set; }

        public WeaponType ActiveWeapon { get; private set; }

        public bool IsUsingTemporaryWeapon { get; private set; }

        private void Start()
        {
            // Discover all weapon components and map by their WeaponType
            var weapons = GetComponentsInChildren<MonoBehaviour>().OfType<IWeapon>();
            _weaponMap = weapons.ToDictionary(w => w.WeaponType, w => w);
            // Start with no weapons equipped
            UnequipAllWeapons();
        }


        public event Action<WeaponType> OnWeaponChanged;
        public event Action<WeaponType> OnPrimaryWeaponChanged;


        /// <summary>
        ///     Switch to a new primary weapon (Axe or Boomerang)
        /// </summary>
        public void SwitchToPrimaryWeapon(WeaponType weaponType)
        {
            if (weaponType != WeaponType.Axe && weaponType != WeaponType.Boomerang)
            {
                Debug.LogWarning(
                    $"WeaponManagerService: Cannot set {weaponType} as primary weapon. Only Axe and Boomerang are allowed.");

                return;
            }

            // Unequip current primary weapon if not using temporary weapon
            if (!IsUsingTemporaryWeapon && CurrentPrimaryWeapon != WeaponType.None)
            {
                UnequipWeapon(CurrentPrimaryWeapon);
            }

            // Set new primary weapon
            CurrentPrimaryWeapon = weaponType;
            OnPrimaryWeaponChanged?.Invoke(CurrentPrimaryWeapon);

            // If not using temporary weapon, equip the new primary weapon
            if (!IsUsingTemporaryWeapon)
            {
                EquipWeapon(CurrentPrimaryWeapon);
                ActiveWeapon = CurrentPrimaryWeapon;
                OnWeaponChanged?.Invoke(ActiveWeapon);
            }
        }

        /// <summary>
        ///     Temporarily switch to a specified weapon (overrides current weapon)
        /// </summary>
        /// <param name="weaponType">The weapon type to switch to (defaults to Fireball if not specified)</param>
        public void SwitchToTemporaryWeapon(WeaponType weaponType = WeaponType.Fireball)
        {
            // Unequip current active weapon
            if (ActiveWeapon != WeaponType.None)
            {
                UnequipWeapon(ActiveWeapon);
            }

            // Equip the specified weapon
            EquipWeapon(weaponType);
            ActiveWeapon = weaponType;
            IsUsingTemporaryWeapon = true;

            OnWeaponChanged?.Invoke(ActiveWeapon);
        }

        /// <summary>
        ///     Stop using temporary weapon and revert to primary weapon
        /// </summary>
        public void RevertFromTemporaryWeapon()
        {
            if (!IsUsingTemporaryWeapon)
            {
                Debug.LogWarning("WeaponManagerService: Not currently using temporary weapon");
                return;
            }

            // Unequip fireball weapon
            UnequipWeapon(ActiveWeapon);

            IsUsingTemporaryWeapon = false;

            // Equip primary weapon if we have one
            if (CurrentPrimaryWeapon != WeaponType.None)
            {
                EquipWeapon(CurrentPrimaryWeapon);
                ActiveWeapon = CurrentPrimaryWeapon;
            }
            else
            {
                ActiveWeapon = WeaponType.None;
            }

            OnWeaponChanged?.Invoke(ActiveWeapon);
        }

        private void EquipWeapon(WeaponType weaponType)
        {
            if (_weaponMap.TryGetValue(weaponType, out IWeapon weapon))
            {
                if (weapon is IUseableWeapon useable)
                    useable.Equip();
                else if (weapon is IAmmoWeapon ammo)
                    ammo.Reload();
            }
        }

        private void UnequipWeapon(WeaponType weaponType)
        {
            if (_weaponMap.TryGetValue(weaponType, out IWeapon weapon) && weapon is IUseableWeapon useable)
                useable.UnEquip();
        }

        private void UnequipAllWeapons()
        {
            foreach (IWeapon weapon in _weaponMap.Values)
                if (weapon is IUseableWeapon useable)
                    useable.UnEquip();
        }

        /// <summary>
        ///     For debugging - get current weapon status
        /// </summary>
        public string GetWeaponStatus() =>
            $"Primary: {CurrentPrimaryWeapon}, Active: {ActiveWeapon}, Temporary: {IsUsingTemporaryWeapon}";
    }
}

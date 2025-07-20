using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Weapons.Interfaces;
using Weapons;

namespace Weapons.Services
{
    /// <summary>
    /// Service that manages weapon switching logic according to Adventure Island 3 mechanics
    /// All weapons are always available - power-ups just switch between them
    /// </summary>
    public class WeaponManagerService : MonoBehaviour
    {
        private Dictionary<WeaponType, IWeapon> _weaponMap;

        private WeaponType _currentPrimaryWeapon;
        private WeaponType _activeWeapon;
        private bool _isUsingTemporaryWeapon;

        [Header("Attack Settings")] [SerializeField]
        public bool canAttack;


        public event Action<WeaponType> OnWeaponChanged;
        public event Action<WeaponType> OnPrimaryWeaponChanged;

        private void Start()
        {
            // Discover all weapon components and map by their WeaponType
            var weapons = GetComponentsInChildren<MonoBehaviour>().OfType<IWeapon>();
            _weaponMap = weapons.ToDictionary(w => w.WeaponType, w => w);
            // Start with no weapons equipped
            UnequipAllWeapons();
        }

        public WeaponType CurrentPrimaryWeapon => _currentPrimaryWeapon;
        public WeaponType ActiveWeapon => _activeWeapon;
        public bool IsUsingTemporaryWeapon => _isUsingTemporaryWeapon;


        /// <summary>
        /// Switch to a new primary weapon (Axe or Boomerang)
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
            if (!_isUsingTemporaryWeapon && _currentPrimaryWeapon != WeaponType.None)
            {
                UnequipWeapon(_currentPrimaryWeapon);
            }

            // Set new primary weapon
            _currentPrimaryWeapon = weaponType;
            OnPrimaryWeaponChanged?.Invoke(_currentPrimaryWeapon);

            // If not using temporary weapon, equip the new primary weapon
            if (!_isUsingTemporaryWeapon)
            {
                EquipWeapon(_currentPrimaryWeapon);
                _activeWeapon = _currentPrimaryWeapon;
                OnWeaponChanged?.Invoke(_activeWeapon);
            }
        }

        /// <summary>
        /// Temporarily switch to a specified weapon (overrides current weapon)
        /// </summary>
        /// <param name="weaponType">The weapon type to switch to (defaults to Fireball if not specified)</param>
        public void SwitchToTemporaryWeapon(WeaponType weaponType = WeaponType.Fireball)
        {
            // Unequip current active weapon
            if (_activeWeapon != WeaponType.None)
            {
                UnequipWeapon(_activeWeapon);
            }

            // Equip the specified weapon
            EquipWeapon(weaponType);
            _activeWeapon = weaponType;
            _isUsingTemporaryWeapon = true;

            OnWeaponChanged?.Invoke(_activeWeapon);
        }

        /// <summary>
        /// Stop using temporary weapon and revert to primary weapon
        /// </summary>
        public void RevertFromTemporaryWeapon()
        {
            if (!_isUsingTemporaryWeapon)
            {
                Debug.LogWarning("WeaponManagerService: Not currently using temporary weapon");
                return;
            }

            // Unequip fireball weapon
            UnequipWeapon(_activeWeapon);

            _isUsingTemporaryWeapon = false;

            // Equip primary weapon if we have one
            if (_currentPrimaryWeapon != WeaponType.None)
            {
                EquipWeapon(_currentPrimaryWeapon);
                _activeWeapon = _currentPrimaryWeapon;
            }
            else
            {
                _activeWeapon = WeaponType.None;
            }

            OnWeaponChanged?.Invoke(_activeWeapon);
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
        /// For debugging - get current weapon status
        /// </summary>
        public string GetWeaponStatus()
        {
            return $"Primary: {_currentPrimaryWeapon}, Active: {_activeWeapon}, Temporary: {_isUsingTemporaryWeapon}";
        }
    }
}

using System;
using UnityEngine;
using Weapons.Models;

namespace Weapons.Services
{
    public enum WeaponType
    {
        None,
        Axe,
        Boomerang,
        Fireball
    }

    /// <summary>
    /// Service that manages weapon switching logic according to Adventure Island 3 mechanics
    /// All weapons are always available - power-ups just switch between them
    /// </summary>
    public class WeaponManagerService : MonoBehaviour
    {
        [Header("Weapon References")]
        [SerializeField] private AxeWeapon axeWeapon;
        [SerializeField] private BoomerangWeapon boomerangWeapon;
        [SerializeField] private FireballWeapon fireballWeapon;

        private WeaponType _currentPrimaryWeapon = WeaponType.None;
        private WeaponType _activeWeapon = WeaponType.None;
        private bool _isUsingTemporaryWeapon = false;

        public event Action<WeaponType> OnWeaponChanged;
        public event Action<WeaponType> OnPrimaryWeaponChanged;

        private void Start()
        {
            // Initialize weapon references if not set
            if (!axeWeapon) axeWeapon = GetComponentInChildren<AxeWeapon>();
            if (!boomerangWeapon) boomerangWeapon = GetComponentInChildren<BoomerangWeapon>();
            if (!fireballWeapon) fireballWeapon = GetComponentInChildren<FireballWeapon>();

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
                Debug.LogWarning($"WeaponManagerService: Cannot set {weaponType} as primary weapon. Only Axe and Boomerang are allowed.");
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
        /// Temporarily switch to Fireball weapon (overrides current weapon)
        /// </summary>
        public void SwitchToTemporaryWeapon()
        {
            // Unequip current active weapon
            if (_activeWeapon != WeaponType.None)
            {
                UnequipWeapon(_activeWeapon);
            }

            // Equip fireball weapon
            EquipWeapon(WeaponType.Fireball);
            _activeWeapon = WeaponType.Fireball;
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
            UnequipWeapon(WeaponType.Fireball);

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
            switch (weaponType)
            {
                case WeaponType.Axe:
                    if (axeWeapon)
                    {
                        axeWeapon.Equip();
                    }
                    break;

                case WeaponType.Boomerang:
                    // Boomerang doesn't have equip/unequip - it's active when selected
                    break;

                case WeaponType.Fireball:
                    if (fireballWeapon)
                    {
                        fireballWeapon.Equip();
                    }
                    break;
            }
        }

        private void UnequipWeapon(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Axe:
                    if (axeWeapon)
                    {
                        axeWeapon.UnEquip();
                    }
                    break;

                case WeaponType.Boomerang:
                    // Boomerang doesn't have equip/unequip, just not active
                    break;

                case WeaponType.Fireball:
                    if (fireballWeapon)
                    {
                        fireballWeapon.UnEquip();
                    }
                    break;
            }
        }

        private void UnequipAllWeapons()
        {
            axeWeapon?.UnEquip();
            fireballWeapon?.UnEquip();
            // Boomerang doesn't need unequipping
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

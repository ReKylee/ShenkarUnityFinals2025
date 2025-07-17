using System;
using UnityEngine;
using Weapons.Models;
using VContainer;
using Core.Data;

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

        private IGameDataService _gameDataService;

        public event Action<WeaponType> OnWeaponChanged;
        public event Action<WeaponType> OnPrimaryWeaponChanged;

        #region VContainer Injection
        [Inject]
        public void Construct(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }
        #endregion

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

            Debug.Log($"WeaponManagerService: Switching primary weapon from {_currentPrimaryWeapon} to {weaponType}");

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
            Debug.Log("WeaponManagerService: Switching to temporary Fireball weapon");

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

            Debug.Log($"WeaponManagerService: Reverting from temporary weapon to primary weapon {_currentPrimaryWeapon}");

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
                    if (axeWeapon != null)
                    {
                        axeWeapon.Equip();
                        Debug.Log("WeaponManagerService: Equipped Axe weapon");
                    }
                    break;

                case WeaponType.Boomerang:
                    // Boomerang uses IAmmoWeapon, so we don't equip/unequip it directly
                    // It's always "available" when unlocked
                    Debug.Log("WeaponManagerService: Switched to Boomerang weapon");
                    break;

                case WeaponType.Fireball:
                    if (fireballWeapon != null)
                    {
                        fireballWeapon.Equip();
                        Debug.Log("WeaponManagerService: Equipped Fireball weapon");
                    }
                    break;
            }
        }

        private void UnequipWeapon(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Axe:
                    if (axeWeapon != null)
                    {
                        axeWeapon.UnEquip();
                        Debug.Log("WeaponManagerService: Unequipped Axe weapon");
                    }
                    break;

                case WeaponType.Boomerang:
                    // Boomerang doesn't have equip/unequip, just not active
                    Debug.Log("WeaponManagerService: Switched away from Boomerang weapon");
                    break;

                case WeaponType.Fireball:
                    if (fireballWeapon != null)
                    {
                        fireballWeapon.UnEquip();
                        Debug.Log("WeaponManagerService: Unequipped Fireball weapon");
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
        /// Check if a weapon type is currently unlocked
        /// </summary>
        public bool IsWeaponUnlocked(WeaponType weaponType)
        {
            if (_gameDataService == null) return false;

            return weaponType switch
            {
                WeaponType.Axe => _gameDataService.HasPowerUp("axe"),
                WeaponType.Boomerang => _gameDataService.HasPowerUp("boomerang"),
                WeaponType.Fireball => _gameDataService.HasPowerUp("fireball"),
                _ => false
            };
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

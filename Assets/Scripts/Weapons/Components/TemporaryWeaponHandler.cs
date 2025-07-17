using System.Collections;
using UnityEngine;
using Weapons.Services;

namespace Weapons.Components
{
    /// <summary>
    /// Component that handles temporary weapon duration and reversion
    /// Follows Single Responsibility Principle - only manages weapon duration
    /// </summary>
    public class TemporaryWeaponHandler : MonoBehaviour
    {
        [Header("Temporary Weapon Settings")]
        [SerializeField] private float fireballDuration = 10f; // Duration in seconds
        
        private WeaponManagerService _weaponManager;
        private Coroutine _temporaryWeaponCoroutine;

        private void Awake()
        {
            _weaponManager = GetComponent<WeaponManagerService>();
            
            if (_weaponManager == null)
            {
                Debug.LogError("TemporaryWeaponHandler: WeaponManagerService not found!");
                return;
            }

            // Subscribe to weapon change events
            _weaponManager.OnWeaponChanged += OnWeaponChanged;
        }

        private void OnDestroy()
        {
            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponChanged -= OnWeaponChanged;
            }
        }

        private void OnWeaponChanged(WeaponType newWeapon)
        {
            if (newWeapon == WeaponType.Fireball && _weaponManager.IsUsingTemporaryWeapon)
            {
                // Start countdown for temporary weapon
                StartTemporaryWeaponCountdown();
            }
            else if (_temporaryWeaponCoroutine != null)
            {
                // Stop countdown if we're no longer using temporary weapon
                StopCoroutine(_temporaryWeaponCoroutine);
                _temporaryWeaponCoroutine = null;
            }
        }

        private void StartTemporaryWeaponCountdown()
        {
            if (_temporaryWeaponCoroutine != null)
            {
                StopCoroutine(_temporaryWeaponCoroutine);
            }

            _temporaryWeaponCoroutine = StartCoroutine(TemporaryWeaponCountdown());
        }

        private IEnumerator TemporaryWeaponCountdown()
        {
            Debug.Log($"TemporaryWeaponHandler: Starting {fireballDuration}s countdown for Fireball weapon");
            
            yield return new WaitForSeconds(fireballDuration);
            
            Debug.Log("TemporaryWeaponHandler: Fireball duration expired, reverting to primary weapon");
            _weaponManager.RevertFromTemporaryWeapon();
            
            _temporaryWeaponCoroutine = null;
        }

        /// <summary>
        /// Manually end temporary weapon (can be called by other systems)
        /// </summary>
        public void EndTemporaryWeapon()
        {
            if (_weaponManager.IsUsingTemporaryWeapon)
            {
                if (_temporaryWeaponCoroutine != null)
                {
                    StopCoroutine(_temporaryWeaponCoroutine);
                    _temporaryWeaponCoroutine = null;
                }
                
                _weaponManager.RevertFromTemporaryWeapon();
            }
        }

        /// <summary>
        /// Get remaining time for temporary weapon
        /// </summary>
        public float GetRemainingTime()
        {
            // This could be enhanced to track actual remaining time
            return _temporaryWeaponCoroutine != null ? fireballDuration : 0f;
        }
    }
}

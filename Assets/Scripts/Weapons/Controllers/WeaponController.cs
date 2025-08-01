using System;
using System.Collections.Generic;
using InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons.Interfaces;
using Weapons.Services;

namespace Weapons.Controllers
{
    /// <summary>
    ///     Controller for managing weapon input following the MVC pattern
    ///     Works with WeaponManagerService to handle weapon switching logic
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private List<WeaponMapping> weaponMappings = new();

        // Input system reference
        private InputSystem_Actions _inputActions;
        private WeaponManagerService _weaponManager;


        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
            _weaponManager = GetComponent<WeaponManagerService>();

            foreach (WeaponMapping mapping in weaponMappings)
            {
                mapping.Initialize(_inputActions, _weaponManager);
            }
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            // Subscribe all weapon mappings to their actions
            foreach (WeaponMapping mapping in weaponMappings)
            {
                mapping.Subscribe();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe all weapon mappings from their actions
            foreach (WeaponMapping mapping in weaponMappings)
            {
                mapping.Unsubscribe();
            }

            _inputActions.Disable();
        }


        [Serializable]
        public class WeaponMapping
        {
            [Header("Weapon Info")] public string weaponName;
            public MonoBehaviour weaponComponent;
            public WeaponType weaponType;

            [Header("Input Configuration")]
            [Tooltip("The full action path from the Input Actions asset (e.g., 'Player/Fire')")]
            [SerializeField]
            private string actionName;

            private InputAction _action;
            private WeaponManagerService _weaponManager;

            // Property for easy access to the weapon component as IWeapon
            public IWeapon WeaponComponent => weaponComponent as IWeapon;

            // Initialize with the input actions instance and weapon manager
            public void Initialize(InputSystem_Actions inputActions, WeaponManagerService weaponManager)
            {
                _weaponManager = weaponManager;

                if (!weaponComponent)
                {
                    Debug.LogError($"Weapon component is null for weapon '{weaponName}'.");
                    return;
                }

                if (string.IsNullOrEmpty(actionName))
                {
                    Debug.LogError($"Action name is not set for weapon '{weaponName}'.");
                    return;
                }

                // Find the action using the full path from the asset.
                _action = inputActions.asset.FindAction(actionName);

                if (_action == null)
                {
                    Debug.LogError($"Action '{actionName}' not found for weapon '{weaponName}'. " +
                                   "Ensure the action path (e.g., 'Player/Fire') is correct and exists in the Input Actions asset.");
                }
            }

            // Subscribe to the action events
            public void Subscribe()
            {
                if (_action != null)
                {
                    _action.performed += OnActionPerformed;
                }
            }

            // Unsubscribe from the action events
            public void Unsubscribe()
            {
                if (_action != null)
                {
                    _action.performed -= OnActionPerformed;
                }
            }

            // Handle input action performed
            private void OnActionPerformed(InputAction.CallbackContext context)
            {
                // Only shoot if this weapon is currently active according to the weapon manager
                if (_weaponManager is { CanAttack: true } && _weaponManager.ActiveWeapon == weaponType)
                {
                    WeaponComponent?.Shoot();
                }
            }
        }
    }


}

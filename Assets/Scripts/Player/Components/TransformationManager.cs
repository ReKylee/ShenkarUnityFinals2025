using System;
using UnityEngine;
using Weapons.Services;
using GabrielBigardi.SpriteAnimator;
using Health.Components;
using Player.Interfaces;
using Weapons;

namespace Player.Components
{
    /// <summary>
    /// Orchestrates transformations 
    /// </summary>
    public class TransformationManager : MonoBehaviour, ITransformationCoordinator
    {
        private TransformationVisualEffects _visualEffects;
        private WeaponManagerService _weaponManager;
        private PlayerHealthController _healthController;
        private DamageShield _damageShield;

        /// <summary>
        /// Event raised when transformation process completes
        /// </summary>
        public event Action OnTransformationComplete;

        /// <summary>
        /// Event raised when detransformation process completes
        /// </summary>
        public event Action OnDetransformationComplete;

        /// <summary>
        /// Whether player is currently transformed
        /// </summary>
        public bool IsTransformed { get; private set; } = false;

        private void Awake()
        {
            _visualEffects = GetComponent<TransformationVisualEffects>();
            _weaponManager = GetComponentInChildren<WeaponManagerService>();
            _healthController = GetComponent<PlayerHealthController>();
            _damageShield = GetComponent<DamageShield>();
        }

        private void Start()
        {
            if (_damageShield)
                _damageShield.OnDamageAbsorbed += OnShieldDamageAbsorbed;

            if (_visualEffects)
                _visualEffects.OnEffectComplete += OnVisualEffectComplete;
        }

        private void OnDestroy()
        {
            if (_damageShield)
                _damageShield.OnDamageAbsorbed -= OnShieldDamageAbsorbed;

            if (_visualEffects)
                _visualEffects.OnEffectComplete -= OnVisualEffectComplete;
        }

        /// <summary>
        /// Apply transformation by coordinating all necessary components
        /// </summary>
        public void ApplyTransformation(SpriteAnimationObject animationObject, Sprite transitionTexture,
            WeaponType weapon)
        {
            if (!ValidateDependencies()) return;


            // Set transformation state
            IsTransformed = true;

            // Activate shield protection
            _healthController.ActivateShield();

            // Disable attacking during visual effect
            _weaponManager.canAttack = false;

            // Cache weapon for after effect completion
            _pendingWeapon = weapon;

            // Start visual transformation effect
            _visualEffects.PlayTransformationEffect(transitionTexture, animationObject);
        }

        /// <summary>
        /// Revert transformation by coordinating all necessary components
        /// </summary>
        public void RevertTransformation()
        {
            if (!ValidateDependencies()) return;

            if (!IsTransformed) return;

            // Clear transformation state
            IsTransformed = false;

            // Revert animation
            _visualEffects.RevertToOriginalState();

            // Revert weapon
            _weaponManager.RevertFromTemporaryWeapon();

            // Deactivate shield
            _healthController.DeactivateShield();

            // Ensure attacking is enabled
            _weaponManager.canAttack = true;

            OnDetransformationComplete?.Invoke();

            Debug.Log("[TransformationManager] Transformation reverted");
        }

        private WeaponType _pendingWeapon = WeaponType.None;

        private void OnVisualEffectComplete()
        {
            // Visual effect completed, now apply gameplay changes
            _weaponManager.canAttack = true;
            _weaponManager.SwitchToTemporaryWeapon(_pendingWeapon);

            OnTransformationComplete?.Invoke();

            Debug.Log("[TransformationManager] Transformation complete");
        }

        private void OnShieldDamageAbsorbed(int damageAmount)
        {
            // Automatic reversion when shield is hit
            RevertTransformation();

            Debug.Log($"[TransformationManager] Shield absorbed {damageAmount} damage - reverting transformation");
        }

        private bool ValidateDependencies()
        {
            if (!_visualEffects || !_weaponManager || !_healthController)
            {
                Debug.LogError("[TransformationManager] Missing required dependencies");
                return false;
            }

            return true;
        }
    }
}

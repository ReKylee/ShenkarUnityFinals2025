using System;
using GabrielBigardi.SpriteAnimator;
using Health.Interfaces;
using Player.Interfaces;
using UnityEngine;
using Weapons;
using Weapons.Services;

namespace Player.Components
{
    public class TransformationManager : MonoBehaviour, ITransformationCoordinator
    {
        private TransformationVisualEffects _visualEffects;
        private WeaponManagerService _weaponManager;
        private IShield _shield;

        private WeaponType _pendingWeapon = WeaponType.None;
        private bool _isTransformed;

        private void Awake()
        {
            _visualEffects = GetComponent<TransformationVisualEffects>();
            _weaponManager = GetComponentInChildren<WeaponManagerService>();
            _shield = GetComponent<IShield>();
        }

        private void OnEnable()
        {
            if (_shield != null)
                _shield.OnShieldBroken += OnShieldDamageAbsorbed;

            if (_visualEffects)
                _visualEffects.OnEffectComplete += OnVisualEffectComplete;
        }

        private void OnDisable()
        {
            if (_shield != null)
                _shield.OnShieldBroken -= OnShieldDamageAbsorbed;

            if (_visualEffects)
                _visualEffects.OnEffectComplete -= OnVisualEffectComplete;
        }


        public void ApplyTransformation(SpriteAnimationObject animationObject, Sprite transitionTexture,
            WeaponType weapon)
        {
            if (!ValidateDependencies()) return;
            _isTransformed = true;
            _shield?.ActivateShield();
            _weaponManager.DisableAttacking();
            _pendingWeapon = weapon;
            _visualEffects.PlayTransformationEffect(transitionTexture, animationObject);
        }


        public void RevertTransformation()
        {
            if (!ValidateDependencies() || !_isTransformed) return;
            _isTransformed = false;
            _visualEffects.RevertToOriginalState();
            _weaponManager.RevertFromTemporaryWeapon();
            Debug.Log("[TransformationManager] Transformation reverted");
        }


        private void OnVisualEffectComplete()
        {
            _weaponManager.EnableAttacking();
            _weaponManager.SwitchToTemporaryWeapon(_pendingWeapon);

            Debug.Log("[TransformationManager] Transformation complete");
        }

        private void OnShieldDamageAbsorbed(int damageAmount)
        {
            RevertTransformation();
        }

        private bool ValidateDependencies() => _visualEffects && _weaponManager && _shield is not null;
    }
}

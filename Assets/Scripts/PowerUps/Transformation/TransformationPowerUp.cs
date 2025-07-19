using System.Collections;
using GabrielBigardi.SpriteAnimator;
using PowerUps._Base;
using UnityEngine;
using Weapons.Services;
using Player;

namespace PowerUps.Transformation
{
    public class TransformationPowerUp : IPowerUp
    {
        private readonly SpriteAnimationObject _animationObject;
        private readonly Sprite _transitionTexture;
        private readonly WeaponType _transformationWeapon;

        public TransformationPowerUp(SpriteAnimationObject animationObject, Sprite transitionTexture,
            WeaponType transformationWeapon = WeaponType.Fireball)
        {
            _animationObject = animationObject;
            _transitionTexture = transitionTexture;
            _transformationWeapon = transformationWeapon;
        }
        public void ApplyPowerUp(GameObject player)
        {
            if (!TryGetRequiredComponents(player, out PlayerAnimationController playerAnimationController,
                    out WeaponManagerService weaponManager, out PlayerHealthController playerHealthController))
                return;

            // Activate one hit shield
            playerHealthController.ActivateShield();

            // Stop attacking during transformation
            weaponManager.canAttack = false;
            
            // Play Transformation Effect
            void OnTransComplete()
            {
                weaponManager.canAttack = true;
                weaponManager.SwitchToTemporaryWeapon(_transformationWeapon);
            }

            playerAnimationController.PlayTransformationEffect(
                _transitionTexture,
                _animationObject,
                OnTransComplete);
        }

        private bool TryGetRequiredComponents(GameObject player, out PlayerAnimationController animationController,
            out WeaponManagerService weaponManager, out PlayerHealthController healthController)
        {
            animationController = player.GetComponent<PlayerAnimationController>();
            weaponManager = player.GetComponentInChildren<WeaponManagerService>();
            healthController = player.GetComponent<PlayerHealthController>();

            return animationController && weaponManager && healthController;
        }

    }
}

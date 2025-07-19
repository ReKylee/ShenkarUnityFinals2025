using System.Collections;
using GabrielBigardi.SpriteAnimator;
using ModularCharacterController.Core;
using PowerUps._Base;
using UnityEngine;
using Weapons.Controllers;
using Weapons.Services;

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
            MonoBehaviour playerBehaviour = player.GetComponent<MonoBehaviour>();
            SpriteAnimator spriteAnimator = player.GetComponent<SpriteAnimator>();
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            WeaponManagerService weaponManager = player.GetComponentInChildren<WeaponManagerService>();
            if (playerBehaviour)
            {
                playerBehaviour.StartCoroutine(ApplyPowerUpCoroutine(weaponManager, spriteAnimator, spriteRenderer));

            }

        }
        private IEnumerator ApplyPowerUpCoroutine(WeaponManagerService weaponManager, SpriteAnimator spriteAnimator,
            SpriteRenderer spriteRenderer)
        {
            if (!spriteAnimator || !spriteRenderer || !weaponManager) yield break;

            weaponManager.canAttack = false;

            spriteAnimator.Pause();
            Sprite originalSprite = spriteRenderer.sprite;
            WaitForSeconds shortWait = new(0.1f);

            // Flash animation sequence
            const int flashCount = 6;
            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.sprite = _transitionTexture;
                yield return shortWait;
                spriteRenderer.sprite = originalSprite;
                yield return shortWait;
            }

            // Complete transformation
            spriteRenderer.sprite = _transitionTexture;
            spriteAnimator.ChangeAnimationObject(_animationObject);
            spriteAnimator.Play("Walk");

            weaponManager.SwitchToTemporaryWeapon(_transformationWeapon);

            weaponManager.canAttack = true;

        }
    }
}

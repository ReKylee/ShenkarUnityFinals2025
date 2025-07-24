using GabrielBigardi.SpriteAnimator;
using Player.Interfaces;
using PowerUps._Base;
using UnityEngine;
using Weapons;

namespace PowerUps.Transformation
{
    public class TransformationPowerUp : IPowerUp
    {
        private readonly SpriteAnimationObject _animationObject;
        private readonly WeaponType _transformationWeapon;
        private readonly Sprite _transitionTexture;

        public TransformationPowerUp(SpriteAnimationObject animationObject, Sprite transitionTexture,
            WeaponType transformationWeapon = WeaponType.Fireball)
        {
            _animationObject = animationObject;
            _transitionTexture = transitionTexture;
            _transformationWeapon = transformationWeapon;
        }

        public void ApplyPowerUp(GameObject player)
        {
            ITransformationCoordinator transformationManager = player.GetComponent<ITransformationCoordinator>();

            transformationManager?.ApplyTransformation(_animationObject, _transitionTexture, _transformationWeapon);
        }
    }
}

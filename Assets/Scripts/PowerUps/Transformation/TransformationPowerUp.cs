using GabrielBigardi.SpriteAnimator;
using PowerUps._Base;
using UnityEngine;
using Weapons.Services;
using Player.Components;

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
            TransformationManager transformationManager = player.GetComponent<TransformationManager>();
            
            // Delegate all transformation logic to the manager
            transformationManager?.ApplyTransformation(_animationObject, _transitionTexture, _transformationWeapon);
        }
    }
}

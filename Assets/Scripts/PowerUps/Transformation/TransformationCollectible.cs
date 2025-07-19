using GabrielBigardi.SpriteAnimator;
using PowerUps._Base;
using UnityEngine;
using Weapons.Services;

namespace PowerUps.Transformation
{
    public class TransformationCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private SpriteAnimationObject animationObject;
        [SerializeField] private Sprite transitionTexture;
        [SerializeField] private WeaponType transformationWeapon = WeaponType.Fireball;

        protected override IPowerUp CreatePowerUp() => new TransformationPowerUp(animationObject, transitionTexture, transformationWeapon);
    }
}

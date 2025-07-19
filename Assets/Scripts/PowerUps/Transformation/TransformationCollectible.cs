using GabrielBigardi.SpriteAnimator;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Transformation
{
    public class TransformationCollectible : PowerUpCollectibleBase
    {
        [SerializeField] private SpriteAnimationObject animationObject;
        [SerializeField] private Sprite transitionTexture;
        protected override IPowerUp CreatePowerUp() => new TransformationPowerUp(animationObject, transitionTexture);
    }
}

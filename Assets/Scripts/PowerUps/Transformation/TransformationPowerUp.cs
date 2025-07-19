using System.Collections;
using GabrielBigardi.SpriteAnimator;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps.Transformation
{
    public class TransformationPowerUp : IPowerUp
    {
        private readonly SpriteAnimationObject _animationObject;
        private readonly Sprite _transitionTexture;
        public TransformationPowerUp(SpriteAnimationObject animationObject, Sprite transitionTexture)
        {
            _animationObject = animationObject;
            _transitionTexture = transitionTexture;
        }
        public void ApplyPowerUp(GameObject player)
        {
            MonoBehaviour playerBehaviour = player.GetComponent<MonoBehaviour>();
            SpriteAnimator spriteAnimator = player.GetComponent<SpriteAnimator>();
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            if (playerBehaviour)
            {
                playerBehaviour.StartCoroutine(ApplyPowerUpCoroutine(player, spriteAnimator, spriteRenderer));
            }

        }
        private IEnumerator ApplyPowerUpCoroutine(GameObject player, SpriteAnimator spriteAnimator, SpriteRenderer spriteRenderer)
        {
            if (!_animationObject || !_transitionTexture)
            {
                Debug.LogError("Animation object is null. Cannot apply transformation power-up.");
                yield break;
            }

            // Cache component references
            
            if (!spriteAnimator || !spriteRenderer) yield break;

            // Pause current animation
            spriteAnimator.Pause();

            // Cache original sprite and wait times
            Sprite originalSprite = spriteRenderer.sprite;
            WaitForSeconds shortWait = new WaitForSeconds(0.1f);

            const int flashCount = 6;
            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.sprite = _transitionTexture;
                yield return shortWait;
                spriteRenderer.sprite = originalSprite;
                yield return shortWait;
            }

            // Final transformation
            spriteRenderer.sprite = _transitionTexture;
            spriteAnimator.ChangeAnimationObject(_animationObject);
        }
    }
}

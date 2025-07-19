using System.Collections;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Centralized controller for player animations, including transformation effects.
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator spriteAnimator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private SpriteAnimationObject _originalAnimationObject;
        private void Awake()
        {
            if (!spriteAnimator) spriteAnimator = GetComponent<SpriteAnimator>();
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
            _originalAnimationObject = spriteAnimator?.SpriteAnimationObject;
        }
        public void Detransform()
        {
            spriteAnimator.ChangeAnimationObject(_originalAnimationObject);
        }

        public void PlayTransformationEffect(Sprite transitionTexture, SpriteAnimationObject newAnimationObject,
            System.Action onComplete, int flashCount = 6, float flashInterval = 0.1f)
        {
            StartCoroutine(TransformationEffect(transitionTexture, newAnimationObject, onComplete, flashCount,
                flashInterval));
        }
 
        private IEnumerator TransformationEffect(Sprite transitionTexture, SpriteAnimationObject newAnimationObject,
            System.Action onComplete, int flashCount = 6, float flashInterval = 0.1f)
        {
            if (!spriteAnimator || !spriteRenderer) yield break;

            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0;

            spriteAnimator.Pause();
            Sprite originalSprite = spriteRenderer.sprite;
            WaitForSecondsRealtime shortWait = new(flashInterval);

            // Flash animation sequence
            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.sprite = transitionTexture;
                yield return shortWait;
                spriteRenderer.sprite = originalSprite;
                yield return shortWait;
            }

            Time.timeScale = originalTimeScale;

            // Apply new animation
            spriteRenderer.sprite = transitionTexture;
            spriteAnimator.ChangeAnimationObject(newAnimationObject);
            spriteAnimator.Play("Walk");

            // Execute callback
            onComplete?.Invoke();
        }
    }
}

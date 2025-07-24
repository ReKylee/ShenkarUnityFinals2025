using System;
using System.Collections;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Player.Components
{
    /// <summary>
    ///     Handles visual transformation effects
    /// </summary>
    public class TransformationVisualEffects : MonoBehaviour
    {
        [SerializeField] private int flashCount = 6;
        [SerializeField] private float flashInterval = 0.1f;

        private PlayerAnimationController _animationController;

        private void Awake()
        {
            _animationController = GetComponent<PlayerAnimationController>();
        }

        /// <summary>
        ///     Event raised when visual effect completes
        /// </summary>
        public event Action OnEffectComplete;

        public void RevertToOriginalState()
        {
            if (!_animationController)
            {
                Debug.LogError("[TransformationVisualEffects] No PlayerAnimationController found");
                return;
            }

            // Restore original sprite and animation object
            _animationController.ChangeAnimationObject(_animationController.OriginalAnimationObject);
            _animationController.PlayAnimation("Idle");

            // Notify completion
            OnEffectComplete?.Invoke();
        }

        /// <summary>
        ///     Play transformation visual effect
        /// </summary>
        public void PlayTransformationEffect(Sprite transitionTexture, SpriteAnimationObject newAnimationObject)
        {
            if (!_animationController)
            {
                Debug.LogError("[TransformationVisualEffects] No PlayerAnimationController found");
                return;
            }

            StartCoroutine(ExecuteTransformationEffect(transitionTexture, newAnimationObject));
        }

        private IEnumerator ExecuteTransformationEffect(Sprite transitionTexture,
            SpriteAnimationObject newAnimationObject)
        {

            // Store original state
            float originalTimeScale = Time.timeScale;
            Sprite originalSprite = _animationController.CurrentSprite;

            // Pause time and animation
            Time.timeScale = 0;
            _animationController.PauseAnimation();

            WaitForSecondsRealtime shortWait = new(flashInterval);

            // Flash animation sequence
            for (int i = 0; i < flashCount; i++)
            {
                _animationController.CurrentSprite = transitionTexture;
                yield return shortWait;
                _animationController.CurrentSprite = originalSprite;
                yield return shortWait;
            }

            // Restore time scale
            Time.timeScale = originalTimeScale;

            // Apply transformation
            _animationController.CurrentSprite = transitionTexture;
            _animationController.ChangeAnimationObject(newAnimationObject);
            _animationController.PlayAnimation("Idle");

            // Notify completion
            OnEffectComplete?.Invoke();
        }
    }
}

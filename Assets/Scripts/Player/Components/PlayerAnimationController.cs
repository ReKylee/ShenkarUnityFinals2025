using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Handles basic player animation operations
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        private SpriteAnimator _spriteAnimator;
        private SpriteRenderer _spriteRenderer;
        public SpriteAnimationObject OriginalAnimationObject { get; private set; }
        public Sprite CurrentSprite {
            get => _spriteRenderer.sprite;
            set => _spriteRenderer.sprite = value;
        }
        
        
        private void Awake()
        {
           _spriteAnimator = GetComponent<SpriteAnimator>();
           _spriteRenderer = GetComponent<SpriteRenderer>();
            OriginalAnimationObject = _spriteAnimator?.SpriteAnimationObject;
        }
        
        /// <summary>
        /// Change to a new animation object
        /// </summary>
        public void ChangeAnimationObject(SpriteAnimationObject animationObject)
        {
            _spriteAnimator?.ChangeAnimationObject(animationObject);
        }
        
        /// <summary>
        /// Play a specific animation
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            _spriteAnimator?.Play(animationName);
        }
        
        
        
        /// <summary>
        /// Pause animation playback
        /// </summary>
        public void PauseAnimation()
        {
            _spriteAnimator?.Pause();
        }
        
        /// <summary>
        /// Resume animation playback
        /// </summary>
        public void ResumeAnimation()
        {
            _spriteAnimator?.Resume();
        }
    }
}

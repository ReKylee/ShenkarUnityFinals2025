using GabrielBigardi.SpriteAnimator;
using ModularCharacterController.Core;
using ModularCharacterController.Core.Components;
using UnityEngine;

namespace Player
{
    /// <summary>
    ///     Handles basic player animation operations
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        private MccGroundCheck _groundCheck;
        private InputHandler _inputHandler;
        private SpriteAnimator _spriteAnimator;
        private SpriteRenderer _spriteRenderer;
        public SpriteAnimationObject OriginalAnimationObject { get; private set; }

        public Sprite CurrentSprite
        {
            get => _spriteRenderer.sprite;
            set => _spriteRenderer.sprite = value;
        }

        private void Awake()
        {
            _inputHandler = GetComponent<InputHandler>();
            _spriteAnimator = GetComponent<SpriteAnimator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _groundCheck = GetComponent<MccGroundCheck>();
            OriginalAnimationObject = _spriteAnimator?.SpriteAnimationObject;
        }

        private void Update()
        {
            if (!_inputHandler || !_groundCheck || Mathf.Approximately(Time.timeScale, 0)) return;

            InputContext input = _inputHandler.CurrentInput;
            // NOTE: I ADDED THIS
            if (input.WalkInput != 0)
            {
                transform.localScale = new Vector3(input.WalkInput, 1, 1);
            }

            // Don't interrupt the Attacking animation if it's still playing
            if (_spriteAnimator.CurrentAnimation.Name == "Attacking" && !_spriteAnimator.AnimationCompleted)
            {
                return;
            }

            // Handle attack
            if (input.AttackPressed)
            {
                _spriteAnimator.Play("Attacking");

                _spriteAnimator.SetOnComplete(() =>
                {
                    if (_groundCheck.IsGrounded)
                    {
                        _spriteAnimator.Play("Idle");
                    }
                });

                return;
            }

            // Jump animation
            if (!_groundCheck.IsGrounded)
            {
                _spriteAnimator.PlayIfNotPlaying("Jump");

                _spriteAnimator.SetOnComplete(() =>
                {
                    if (_groundCheck.IsGrounded)
                    {
                        _spriteAnimator.Play("Idle");
                    }
                });

                return;
            }

            // Walk animation
            if (input.WalkInput != 0)
            {
                _spriteAnimator.PlayIfNotPlaying("Walk");
                return;
            }

            // Default to idle
            _spriteAnimator.PlayIfNotPlaying("Idle");
        }


        /// <summary>
        ///     Change to a new animation object
        /// </summary>
        public void ChangeAnimationObject(SpriteAnimationObject animationObject)
        {
            _spriteAnimator?.ChangeAnimationObject(animationObject);
        }

        /// <summary>
        ///     Play a specific animation
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            _spriteAnimator?.Play(animationName);
        }


        /// <summary>
        ///     Pause animation playback
        /// </summary>
        public void PauseAnimation()
        {
            _spriteAnimator?.Pause();
        }

        /// <summary>
        ///     Resume animation playback
        /// </summary>
        public void ResumeAnimation()
        {
            _spriteAnimator?.Resume();
        }
    }
}

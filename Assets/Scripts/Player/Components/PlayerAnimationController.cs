using System.Collections;
using System.Collections.Generic;
using Core.Events;
using GabrielBigardi.SpriteAnimator;
using ModularCharacterController.Core;
using ModularCharacterController.Core.Components;
using UnityEngine;
using VContainer;

namespace Player.Components
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

        private IEventBus _eventBus;

        private bool _isDead = false;

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
            if (_isDead) return;
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


        
        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<PlayerDeathEvent>(OnPlayerDeath);
        }

        private IEnumerator PlayDeathSequence()
        {
            _spriteAnimator.Play("Death");

            const float moveUpUnits = 42f / 16f;
            Vector3 startPos = transform.position;
            Vector3 upPos = startPos + Vector3.up * moveUpUnits;
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, upPos, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.position = upPos;
            yield return new WaitForSecondsRealtime(0.6f);
            float moveDownUnits = 10f;
            Vector3 downPos = upPos + Vector3.down * moveDownUnits;
            duration = 0.6f;
            elapsed = 0f;
            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(upPos, downPos, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.position = downPos;
        }

        private void OnPlayerDeath(PlayerDeathEvent evt)
        {
            _isDead = true;
            StartCoroutine(PlayDeathSequence());
        }
    }
}

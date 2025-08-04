using Audio.Data;
using Audio.Interfaces;
using Audio.Services;
using Collectables.Score;
using Health.Interfaces;
using ModularCharacterController.Core;
using ModularCharacterController.Core.Components;
using UnityEngine;
using VContainer;

namespace Player.Components
{
    /// <summary>
    ///     Unified player audio controller that handles both sound events and audio playback
    ///     Combines event subscription logic with SOLID audio system integration
    /// </summary>
    public class PlayerSoundController : MonoBehaviour
    {
        [Header("Player Sound Events")] [SerializeField]
        private SoundData jumpSound;

        [SerializeField] private SoundData collectSound;
        [SerializeField] private SoundData deathSound;
        [SerializeField] private SoundData attackSound;
        private MccGroundCheck _groundCheck;
        private IHealthEvents _health;
        private InputHandler _inputHandler;

        private void Awake()
        {
            _health = GetComponent<IHealthEvents>();
            _inputHandler = GetComponent<InputHandler>();
            _groundCheck = GetComponent<MccGroundCheck>();
        }

        private void Update()
        {
            InputContext input = _inputHandler.CurrentInput;
            if (input.AttackPressed)
            {
                PlayAttackSound();
                return;
            }
            
            if (input.JumpPressed && _groundCheck.IsGrounded)
            {
                PlayJumpSound();
            }

        }
        private void PlayAttackSound()
        {
            AudioService.Instance?.PlaySound(attackSound);
        }

        private void OnEnable()
        {
            ScoreCollectable.OnScoreCollected += OnScoreCollected;

            if (_health != null)
            {
                _health.OnDeath += OnDeath;
            }
        }

        private void OnDisable()
        {
            ScoreCollectable.OnScoreCollected -= OnScoreCollected;

            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
            }
        }


        #region Event Handlers

        private void OnScoreCollected(int score, Vector3 position)
        {
            PlayCollectSoundAtPosition(position);
        }

        private void OnDeath()
        {
            PlayDeathSound();
        }

        #endregion

        #region Public Audio Methods

        private void PlayJumpSound()
        {
            AudioService.Instance?.PlaySound(jumpSound);
        }


        private void PlayDeathSound()
        {
            AudioService.Instance?.PlaySound(deathSound);
        }

        private void PlayCollectSoundAtPosition(Vector3 position)
        {
            AudioService.Instance?.PlaySoundAtPosition(collectSound, position);
        }

        #endregion


    }
}

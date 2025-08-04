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
        [SerializeField] private SoundData transformSound;
        private MccGroundCheck _groundCheck;
        private IHealthEvents _health;
        private InputHandler _inputHandler;
        private bool _jumpSoundPlayed;
        private TransformationVisualEffects _transformationVisualEffects;

        private void Awake()
        {
            _health = GetComponent<IHealthEvents>();
            _inputHandler = GetComponent<InputHandler>();
            _groundCheck = GetComponent<MccGroundCheck>();
            _transformationVisualEffects = GetComponent<TransformationVisualEffects>();
            _transformationVisualEffects.OnEffectStarted += () => AudioService.Instance?.PlaySound(transformSound);
        }

        private void Update()
        {
            InputContext input = _inputHandler.CurrentInput;

            if (input.JumpPressed && _groundCheck.IsGrounded && !_jumpSoundPlayed)
            {
                AudioService.Instance?.PlaySound(jumpSound);
                _jumpSoundPlayed = true;
            }

            if (!input.JumpPressed)
            {
                _jumpSoundPlayed = false;
            }

            if (input.AttackPressed)
            {
                AudioService.Instance?.PlaySound(attackSound);
            }
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
            AudioService.Instance?.PlaySound(collectSound);
        }

        private void OnDeath()
        {
            AudioService.Instance?.PlaySound(deathSound);
        }

        #endregion


    }
}

using Audio.Data;
using Audio.Interfaces;
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
        private IAudioService _audioService;
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
            if (input.JumpPressed && _groundCheck.IsGrounded)
            {
                PlayJumpSound();
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

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
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

        public void PlayJumpSound()
        {
            PlaySoundData(jumpSound);
        }


        private void PlayDeathSound()
        {
            PlaySoundData(deathSound);
        }

        public void PlayCollectSoundAtPosition(Vector3 position)
        {
            PlaySoundDataAtPosition(collectSound, position);
        }

        #endregion

        #region Private Audio Helpers

        private void PlaySoundData(SoundData soundData)
        {
            if (soundData?.clip && _audioService != null)
            {
                _audioService.PlaySound(
                    soundData.clip,
                    soundData.GetRandomizedVolume(),
                    soundData.GetRandomizedPitch()
                );
            }
        }

        private void PlaySoundDataAtPosition(SoundData soundData, Vector3 position)
        {
            if (soundData?.clip && _audioService != null)
            {
                _audioService.PlaySoundAtPosition(
                    soundData.clip,
                    position,
                    soundData.GetRandomizedVolume(),
                    soundData.GetRandomizedPitch()
                );
            }
        }

        #endregion

    }
}

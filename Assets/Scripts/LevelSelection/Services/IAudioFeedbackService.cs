using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for audio feedback during level selection
    /// </summary>
    public interface IAudioFeedbackService
    {
        void Initialize(AudioSource audioSource, LevelSelectionConfig config);
        void PlayNavigationSound();
        void PlaySelectionSound();
        void PlayLockedSound();
    }

    /// <summary>
    ///     Handles audio feedback for level selection events (Single Responsibility)
    /// </summary>
    public class AudioFeedbackService : IAudioFeedbackService
    {
        private AudioSource _audioSource;
        private LevelSelectionConfig _config;

        public void Initialize(AudioSource audioSource, LevelSelectionConfig config)
        {
            _audioSource = audioSource;
            _config = config;
        }

        public void PlayNavigationSound()
        {
            if (_config?.navigationSound && _audioSource)
            {
                _audioSource.PlayOneShot(_config.navigationSound);
            }
        }

        public void PlaySelectionSound()
        {
            if (_config?.selectionSound && _audioSource)
            {
                _audioSource.PlayOneShot(_config.selectionSound);
            }
        }

        public void PlayLockedSound()
        {
            if (_config?.lockedSound && _audioSource)
            {
                _audioSource.PlayOneShot(_config.lockedSound);
            }
        }
    }
}

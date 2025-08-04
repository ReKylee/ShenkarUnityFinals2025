using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Handles audio feedback for level selection events (Single Responsibility)
    /// </summary>
    public class AudioFeedbackService : IAudioFeedbackService
    {
        private AudioSource _audioSource;

        public void Initialize(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        public void PlayNavigationSound()
        {
            if (_audioSource)
            {
                _audioSource.PlayOneShot(_audioSource.clip);
            }
        }

        public void PlaySelectionSound()
        {
            if (_audioSource)
            {
                _audioSource.PlayOneShot(_audioSource.clip);
            }
        }

        public void PlayLockedSound()
        {
            if (_audioSource)
            {
                _audioSource.PlayOneShot(_audioSource.clip);
            }
        }
    }
}

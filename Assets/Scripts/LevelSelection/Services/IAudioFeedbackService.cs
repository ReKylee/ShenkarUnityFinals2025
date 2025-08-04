using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for audio feedback during level selection
    /// </summary>
    public interface IAudioFeedbackService
    {
        void Initialize(AudioSource audioSource);
        void PlaySelectionSound();
        void PlayLockedSound();
    }

}

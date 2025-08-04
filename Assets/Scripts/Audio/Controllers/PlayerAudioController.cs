using Audio.Data;
using Audio.Interfaces;
using UnityEngine;
using VContainer;

namespace Audio.Controllers
{
    /// <summary>
    ///     Player-specific audio controller following Single Responsibility Principle
    ///     Only handles player audio events, delegates actual audio playing to AudioService
    /// </summary>
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Player Sound Events")] [SerializeField]
        private SoundData jumpSound;

        [SerializeField] private SoundData collectSound;
        [SerializeField] private SoundData deathSound;
        [SerializeField] private SoundData damageSound;

        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void PlayJumpSound()
        {
            PlaySoundData(jumpSound);
        }

        public void PlayCollectSound()
        {
            PlaySoundData(collectSound);
        }

        public void PlayDeathSound()
        {
            PlaySoundData(deathSound);
        }

        public void PlayDamageSound()
        {
            PlaySoundData(damageSound);
        }

        public void PlayCollectSoundAtPosition(Vector3 position)
        {
            PlaySoundDataAtPosition(collectSound, position);
        }

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
    }
}

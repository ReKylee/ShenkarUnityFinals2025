using Audio.Data;
using UnityEngine;

namespace Audio.Interfaces
{
    /// <summary>
    ///     Main audio service interface for playing sounds and music with mixer support
    /// </summary>
    public interface IAudioService
    {
        void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f);
        void PlaySound(SoundData soundData);
        void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f);
        void PlaySoundAtPosition(SoundData soundData, Vector3 position);
        void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true);
        void StopMusic();
        void SetMasterVolume(float volume);
        void SetSFXVolume(float volume);
        void SetMusicVolume(float volume);
    }
}

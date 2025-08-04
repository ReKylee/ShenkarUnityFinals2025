using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Audio.Data
{
    /// <summary>
    ///     Simple sound data container with mixer channel support
    /// </summary>
    [Serializable]
    public class SoundData
    {
        [Header("Audio Clip")] public AudioClip clip;

        [Header("Mixer Settings")] public AudioMixerGroup mixerGroup;

        [Header("Settings")] [Range(0f, 1f)] public float volume = 1f;

        [Range(0.1f, 3f)] public float pitch = 1f;
        [Range(0f, 0.5f)] public float randomVolumeVariance = 0f;
        [Range(0f, 0.5f)] public float randomPitchVariance = 0f;

        [Header("3D Audio")] public bool use3D = false;

        /// <summary>
        ///     Get randomized volume within variance range
        /// </summary>
        public float GetRandomizedVolume() =>
            volume + Random.Range(-randomVolumeVariance, randomVolumeVariance);

        /// <summary>
        ///     Get randomized pitch within variance range
        /// </summary>
        public float GetRandomizedPitch() =>
            pitch + Random.Range(-randomPitchVariance, randomPitchVariance);
    }
}

using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio.Data
{
    /// <summary>
    ///     Simple sound data container without ScriptableObjects
    /// </summary>
    [Serializable]
    public class SoundData
    {
        [Header("Audio Clip")] public AudioClip clip;

        [Header("Settings")] [Range(0f, 1f)] public float volume = 1f;

        [Range(0.1f, 3f)] public float pitch = 1f;
        [Range(0f, 0.5f)] public float randomVolumeVariance;
        [Range(0f, 0.5f)] public float randomPitchVariance;

        [Header("3D Audio")] public bool use3D;

        /// <summary>
        ///     Get randomized volume within variance range
        /// </summary>
        public float GetRandomizedVolume() => volume + Random.Range(-randomVolumeVariance, randomVolumeVariance);

        /// <summary>
        ///     Get randomized pitch within variance range
        /// </summary>
        public float GetRandomizedPitch() => pitch + Random.Range(-randomPitchVariance, randomPitchVariance);
    }
}

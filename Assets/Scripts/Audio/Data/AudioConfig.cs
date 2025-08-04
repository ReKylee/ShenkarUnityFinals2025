using System;
using UnityEngine;

namespace Audio.Data
{
    /// <summary>
    ///     Configuration data for audio settings (Single Responsibility)
    /// </summary>
    [Serializable]
    public class AudioConfig
    {
        [Header("Volume Settings")] [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;

        [Header("Audio Source Settings")] public int maxConcurrentSounds = 10;

        public float defaultPitch = 1f;

        [Header("3D Audio Settings")] public float spatialBlend; // 0 = 2D, 1 = 3D

        public float minDistance = 1f;
        public float maxDistance = 500f;
    }
}

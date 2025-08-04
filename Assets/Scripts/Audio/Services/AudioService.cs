﻿using System.Collections.Generic;
using Audio.Data;
using Audio.Interfaces;
using UnityEngine;

namespace Audio.Services
{
    /// <summary>
    ///     Main audio service implementation following SOLID principles
    ///     Single Responsibility: Manages all audio playback
    ///     Open/Closed: Extensible through interfaces
    ///     Dependency Inversion: Depends on abstractions
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioConfig audioConfig = new();
        private int _currentSfxIndex;

        private float _masterVolume = 1f;

        private AudioSource _musicSource;
        private float _musicVolume = 1f;
        private List<AudioSource> _sfxSources;
        private float _sfxVolume = 1f;

        private void Awake()
        {
            // Don't destroy on load for persistent audio
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            LoadVolumeSettings();
        }

        public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (!clip) return;

            AudioSource availableSource = GetAvailableSfxSource();
            if (!availableSource) return;

            ConfigureSfxSource(availableSource, clip, volume, pitch);
            availableSource.Play();
        }

        public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (!clip) return;

            // Create temporary audio source at position for 3D audio
            GameObject tempAudioObject = new($"TempAudio_{clip.name}");
            tempAudioObject.transform.position = position;

            AudioSource tempSource = tempAudioObject.AddComponent<AudioSource>();
            ConfigureSfxSource(tempSource, clip, volume, pitch, true);

            tempSource.Play();

            // Destroy after clip finishes
            Destroy(tempAudioObject, clip.length);
        }

        public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (!clip || !_musicSource) return;

            _musicSource.clip = clip;
            _musicSource.volume = volume * _musicVolume * _masterVolume;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource)
            {
                _musicSource.Stop();
            }
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource && _musicSource.isPlaying)
            {
                _musicSource.volume = _musicSource.volume * _musicVolume * _masterVolume;
            }
        }

        private void InitializeAudioSources()
        {
            // Create dedicated music source
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            // Create pool of SFX sources for concurrent sounds
            _sfxSources = new List<AudioSource>();
            int maxSources = audioConfig.maxConcurrentSounds;

            for (int i = 0; i < maxSources; i++)
            {
                AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                _sfxSources.Add(sfxSource);
            }
        }

        private void LoadVolumeSettings()
        {
            _masterVolume = audioConfig.masterVolume;
            _sfxVolume = audioConfig.sfxVolume;
            _musicVolume = audioConfig.musicVolume;
        }

        private AudioSource GetAvailableSfxSource()
        {
            // Find an available (not playing) source
            foreach (AudioSource source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // If all sources are busy, use round-robin
            AudioSource roundRobinSource = _sfxSources[_currentSfxIndex];
            _currentSfxIndex = (_currentSfxIndex + 1) % _sfxSources.Count;
            return roundRobinSource;
        }

        private void ConfigureSfxSource(AudioSource source, AudioClip clip, float volume, float pitch,
            bool is3D = false)
        {
            source.clip = clip;
            source.volume = volume * _sfxVolume * _masterVolume;
            source.pitch = pitch;

            source.spatialBlend = is3D ? 1f : audioConfig.spatialBlend;
            source.minDistance = audioConfig.minDistance;
            source.maxDistance = audioConfig.maxDistance;
        }

        private void UpdateAllVolumes()
        {
            // Update music volume
            if (_musicSource && _musicSource.isPlaying)
            {
                float currentMusicVolume = _musicSource.volume / (_musicVolume * _masterVolume);
                _musicSource.volume = currentMusicVolume * _musicVolume * _masterVolume;
            }
        }
    }
}

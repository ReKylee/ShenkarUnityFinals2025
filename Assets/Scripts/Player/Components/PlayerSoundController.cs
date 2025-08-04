using System;
using Core.Events;
using Collectables.Score;
using Health.Interfaces;
using ModularCharacterController.Core;
using ModularCharacterController.Core.Components;
using UnityEngine;
using VContainer;

namespace Player.Components
{
    /// <summary>
    /// Handles all player sound effects and audio feedback
    /// </summary>
    public class PlayerSoundController : MonoBehaviour
    {
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip deathSound;
        
        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float jumpVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float collectVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float damageVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;
        
        private IHealthEvents _health;
        private AudioSource _audioSource;
        private InputHandler _inputHandler;
        private MccGroundCheck _groundCheck;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _health = GetComponent<IHealthEvents>();
            _inputHandler = gameObject.GetComponent<InputHandler>();
            _groundCheck = gameObject.GetComponent<MccGroundCheck>();
        }
        
        private void OnEnable()
        {
            ScoreCollectable.OnScoreCollected += OnScoreCollected;
            
            if (_health != null)
            {
                _health.OnHealthChanged += OnHealthChanged;
                _health.OnDeath += OnDeath;
            }
        }
        private void OnScoreCollected(int arg1, Vector3 arg2)
        {
            PlaySound(collectSound, collectVolume);
            
        }

        private void OnDisable()
        {
            ScoreCollectable.OnScoreCollected -= OnScoreCollected;
            
            if (_health != null)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnDeath -= OnDeath;
            }
        }
        private void Update()
        {
            InputContext input = _inputHandler.CurrentInput;
            if (input.JumpPressed && _groundCheck.IsGrounded)
            {
                PlayJumpSound();
            }
        }
        private void PlayJumpSound()
        {
            PlaySound(jumpSound, jumpVolume);
        }


        private void OnHealthChanged(int currentHp, int maxHp)
        {
            PlaySound(damageSound, damageVolume);
        }

        private void OnDeath()
        {
            PlaySound(deathSound, deathVolume);
        }
        

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (_audioSource && clip)
            {
                _audioSource.PlayOneShot(clip, volume);
            }
        }

    }
}

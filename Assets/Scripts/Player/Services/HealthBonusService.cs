using System;
using System.Collections;
using Collectables.Score;
using Health.Interfaces;
using Player.Components;
using UnityEngine;
using VContainer;

namespace Player.Services
{
    /// <summary>
    /// Service responsible for calculating and awarding bonus points based on remaining health
    /// </summary>
    public class HealthBonusService : MonoBehaviour
    {
        [Header("Bonus Settings")]
        [SerializeField] private int pointsPerHp = 100;
        [SerializeField] private float drainDelay = 0.2f;
        [SerializeField] private float drainInterval = 0.1f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip bonusDrainSfx;
        
        private IScoreService _scoreService;
        private PlayerHealthController _healthController;
        private IHealthView _healthView;
        private AudioSource _audioSource;
        
        [Inject]
        public void Construct(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            _healthController = GetComponent<PlayerHealthController>();
        }
        private void Start()
        {
            _healthView = _healthController?.HealthView;
            
        }

        /// <summary>
        /// Calculate and award bonus points for remaining health
        /// </summary>
        /// <param name="onComplete">Callback when bonus calculation is complete</param>
        public void CalculateHealthBonus(System.Action onComplete = null)
        {
            if (_healthController == null)
            {
                Debug.LogWarning("[HealthBonusService] No PlayerHealthController found");
                onComplete?.Invoke();
                return;
            }
            
            int remainingHp = _healthController.CurrentHp;
            
            if (remainingHp <= 0)
            {
                Debug.Log("[HealthBonusService] No remaining HP for bonus");
                onComplete?.Invoke();
                return;
            }
            
            Debug.Log($"[HealthBonusService] Starting health bonus calculation: {remainingHp} HP remaining");
            StartCoroutine(DrainHealthForBonus(remainingHp, onComplete));
        }
        
        private IEnumerator DrainHealthForBonus(int remainingHp, System.Action onComplete)
        {
            // Wait before starting the drain
            yield return new WaitForSeconds(drainDelay);
            
            // Drain each HP point individually
            for (int i = 0; i < remainingHp; i++)
            {
                // Award points for this HP
                _scoreService?.AddScore(pointsPerHp);
                
                // Calculate remaining health after draining this HP point
                int healthAfterDrain = remainingHp - (i + 1);
                
                // Update health display (visual only)
                _healthView?.UpdateDisplay(healthAfterDrain, _healthController.MaxHp);

                // Play drain sound effect
                if (bonusDrainSfx && _audioSource)
                {
                    _audioSource.PlayOneShot(bonusDrainSfx);
                }
                
                
                // Wait before next drain
                yield return new WaitForSeconds(drainInterval);
            }

            onComplete?.Invoke();
        }
    }
}

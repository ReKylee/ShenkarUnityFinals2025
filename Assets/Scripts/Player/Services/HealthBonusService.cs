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
        [SerializeField] private float drainDelay = 0.5f;
        [SerializeField] private float drainInterval = 0.3f;
        
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
            
            int totalBonus = remainingHp * pointsPerHp;
            Debug.Log($"[HealthBonusService] Total bonus to award: {totalBonus} points");
            
            // Drain each HP point individually
            for (int i = 0; i <= remainingHp; i++)
            {
                // Award points for this HP
                _scoreService?.AddScore(pointsPerHp);
                
                // Reduce health by 1 (visual only)
                _healthView?.UpdateDisplay(
                    Mathf.Max(_healthController.CurrentHp - i, 0), 
                    _healthController.MaxHp);

                Debug.Log("HealthView: " + _healthView);
                // Play drain sound effect
                if (bonusDrainSfx && _audioSource)
                {
                    _audioSource.PlayOneShot(bonusDrainSfx);
                }
                
                Debug.Log($"[HealthBonusService] HP consumed for bonus: {pointsPerHp} points awarded");
                
                // Wait before next drain
                yield return new WaitForSeconds(drainInterval);
            }

            Debug.Log("[HealthBonusService] Health bonus calculation complete");
            
            // Wait a moment for final sound to play
            yield return new WaitForSeconds(0.5f);
            
            onComplete?.Invoke();
        }
    }
}

using System;
using System.Collections;
using Audio.Data;
using Audio.Interfaces;
using Collectables.Score;
using Health.Interfaces;
using Player.Components;
using UnityEngine;
using VContainer;

namespace Player.Services
{
    /// <summary>
    ///     Service responsible for calculating and awarding bonus points based on remaining health
    ///     Now uses the SOLID audio system
    /// </summary>
    public class HealthBonusService : MonoBehaviour
    {
        [Header("Bonus Settings")] [SerializeField]
        private int pointsPerHp = 100;

        [SerializeField] private float drainDelay = 0.2f;
        [SerializeField] private float drainInterval = 0.1f;

        [Header("Audio")] [SerializeField] private SoundData bonusDrainSound;

        private IAudioService _audioService;
        private PlayerHealthController _healthController;
        private IHealthView _healthView;

        private IScoreService _scoreService;

        private void Awake()
        {
            _healthController = GetComponent<PlayerHealthController>();
        }

        private void Start()
        {
            _healthView = _healthController?.HealthView;
        }

        [Inject]
        public void Construct(IScoreService scoreService, IAudioService audioService)
        {
            _scoreService = scoreService;
            _audioService = audioService;
        }

        /// <summary>
        ///     Calculate and award bonus points for remaining health
        /// </summary>
        /// <param name="onComplete">Callback when bonus calculation is complete</param>
        public void CalculateHealthBonus(Action onComplete = null)
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

        private IEnumerator DrainHealthForBonus(int remainingHp, Action onComplete)
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

                // Play drain sound effect using new audio system
                if (bonusDrainSound?.clip && _audioService != null)
                {
                    _audioService.PlaySound(bonusDrainSound);
                }

                // Wait before next drain
                yield return new WaitForSeconds(drainInterval);
            }

            Debug.Log(
                $"[HealthBonusService] Health bonus calculation complete. Awarded {remainingHp * pointsPerHp} bonus points");

            onComplete?.Invoke();
        }
    }
}

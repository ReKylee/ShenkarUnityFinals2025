﻿using System.Collections;
using Audio.Data;
using Audio.Interfaces;
using Core;
using ModularCharacterController.Core;
using Player.Services;
using UnityEngine;
using VContainer;

namespace LevelSelection
{
    /// <summary>
    ///     Trigger zone that detects when the player completes a level.
    ///     Its only responsibility is to notify the GameFlowManager.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EndLevelZone : MonoBehaviour
    {
        [Header("Level Completion Settings")] [SerializeField]
        private string currentLevelName;

        [SerializeField] private string nextLevelName;
        [SerializeField] private float completionDelay = 2f;

        [Header("Audio")] [SerializeField] private SoundData completionSound;

        private GameFlowManager _gameFlowManager;
        private IAudioService _audioService;
        private bool _hasTriggered;
        private HealthBonusService _healthBonusService;


        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if player entered
            if (other.CompareTag("Player") && !_hasTriggered)
            {
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                InputHandler input = other.GetComponent<InputHandler>();
                StartCoroutine(CompleteLevel(rb, input));
            }
        }

        [Inject]
        public void Construct(GameFlowManager gameFlowManager, HealthBonusService healthBonusService,
            IAudioService audioService)
        {
            _gameFlowManager = gameFlowManager;
            _healthBonusService = healthBonusService;
            _audioService = audioService;
        }

        private IEnumerator CompleteLevel(Rigidbody2D rb, InputHandler input)
        {
            _hasTriggered = true;

            // Stop all music immediately when level is completed
            _audioService?.StopMusic();

            // Take control from the player
            if (input)
            {
                input.enabled = false;
            }

            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Make the player walk right
            const float walkDuration = 1.3f;
            float timer = 0f;
            while (timer < walkDuration)
            {
                if (rb)
                {
                    rb.linearVelocityX = 2f;
                }

                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            Debug.Log($"[EndLevelZone] Player completed level: {currentLevelName}");

            // Play completion sound using the new audio system
            if (completionSound?.clip && _audioService != null)
            {
                _audioService.PlaySound(completionSound);
            }

            bool bonusComplete = false;
            if (_healthBonusService)
            {
                Debug.Log("[EndLevelZone] Starting health bonus calculation...");
                _healthBonusService.CalculateHealthBonus(() => bonusComplete = true);

                // Wait for bonus calculation to complete
                yield return new WaitUntil(() => bonusComplete);
                Debug.Log("[EndLevelZone] Health bonus calculation finished");
            }

            yield return new WaitForSeconds(completionDelay);

            _gameFlowManager?.CompleteLevel(currentLevelName);
        }
    }
}

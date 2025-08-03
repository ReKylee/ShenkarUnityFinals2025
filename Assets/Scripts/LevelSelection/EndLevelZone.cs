using System.Collections;
using System.Collections.Generic;
using Core.Data;
using Core.Events;
using UnityEngine;
using VContainer;

namespace LevelSelection
{
    /// <summary>
    ///     Trigger zone that detects when the player completes a level
    ///     and unlocks the next level in sequence
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EndLevelZone : MonoBehaviour
    {
        [Header("Level Completion Settings")] [SerializeField]
        private string currentLevelName;

        [SerializeField] private string nextLevelName;
        [SerializeField] private bool autoReturnToLevelSelect = true;
        [SerializeField] private float completionDelay = 2f;

        [Header("Audio Feedback")] [SerializeField]
        private AudioClip completionSound;

        [Header("UI Feedback")] [SerializeField]
        private GameObject completionUI;

        [SerializeField] private float uiDisplayDuration = 3f;
        private AudioSource _audioSource;

        private IEventBus _eventBus;
        private IGameDataService _gameDataService;
        private bool _hasTriggered;

        private void Awake()
        {
            // Setup audio component
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Ensure trigger is set up correctly
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;

            // Hide completion UI initially
            if (completionUI != null)
            {
                completionUI.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if player entered
            if (other.CompareTag("Player") && !_hasTriggered)
            {
                StartCoroutine(CompleteLevel());
            }
        }

        [Inject]
        public void Construct(IEventBus eventBus, IGameDataService gameDataService)
        {
            _eventBus = eventBus;
            _gameDataService = gameDataService;
        }

        private IEnumerator CompleteLevel()
        {
            _hasTriggered = true;

            Debug.Log($"[EndLevelZone] Player completed level: {currentLevelName}");

            // Play completion sound
            if (completionSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(completionSound);
            }

            // Show completion UI
            if (completionUI != null)
            {
                completionUI.SetActive(true);
                yield return new WaitForSeconds(uiDisplayDuration);
                completionUI.SetActive(false);
            }

            // Wait for completion delay
            yield return new WaitForSeconds(completionDelay);

            // Calculate completion time (should be tracked by GameFlowManager, but we'll use a simple calculation here)
            float completionTime = Time.time; // This should be the actual level completion time

            // Update game data with completion stats
            UpdateCompletionStats(completionTime);

            // Unlock next level and save progress
            UnlockNextLevel();

            // Publish level completion event (for GameFlowManager)
            _eventBus?.Publish(new Core.Events.LevelCompletedEvent
            {
                Timestamp = Time.time,
                LevelName = currentLevelName,
                CompletionTime = completionTime
            });

            // Publish level unlocked event (for level selection system)
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                _eventBus?.Publish(new Core.Events.LevelUnlockedEvent
                {
                    Timestamp = Time.time,
                    CompletedLevelName = currentLevelName,
                    UnlockedLevelName = nextLevelName
                });
            }

            // Return to level select if enabled
            if (autoReturnToLevelSelect)
            {
                // Use standalone SceneTransitionManager
                SceneTransitionManager.TransitionTo("LevelSelection");
            }
        }

        private void UpdateCompletionStats(float completionTime)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData == null) return;

            // Update best time for this level
            gameData.UpdateLevelBestTime(currentLevelName, completionTime);

            // Update best score for this level (assuming score is tracked elsewhere)
            gameData.UpdateLevelBestScore(currentLevelName, gameData.score);

            // Update overall max score
            gameData.UpdateMaxScore(gameData.score);

            Debug.Log(
                $"[EndLevelZone] Updated stats - Time: {completionTime:F2}s, Score: {gameData.score}, Best Time: {gameData.GetLevelBestTime(currentLevelName):F2}s");
        }

        private void UnlockNextLevel()
        {
            if (string.IsNullOrEmpty(nextLevelName))
            {
                Debug.Log("[EndLevelZone] No next level specified to unlock");
                return;
            }

            // Get current game data
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData == null)
            {
                Debug.LogWarning("[EndLevelZone] No game data service available");
                return;
            }

            // Initialize unlocked levels list if needed
            if (gameData.unlockedLevels == null)
            {
                gameData.unlockedLevels = new List<string>();
            }

            // Add next level to unlocked list if not already unlocked
            if (!gameData.unlockedLevels.Contains(nextLevelName))
            {
                gameData.unlockedLevels.Add(nextLevelName);
                Debug.Log($"[EndLevelZone] Unlocked next level: {nextLevelName}");
            }

            // Mark current level as completed
            if (gameData.completedLevels == null)
            {
                gameData.completedLevels = new List<string>();
            }

            if (!gameData.completedLevels.Contains(currentLevelName))
            {
                gameData.completedLevels.Add(currentLevelName);
                Debug.Log($"[EndLevelZone] Marked level as completed: {currentLevelName}");
            }

            // Save the data
            _gameDataService?.SaveData();
        }

        /// <summary>
        ///     Manually trigger level completion (for testing or external calls)
        /// </summary>
        public void TriggerCompletion()
        {
            if (!_hasTriggered)
            {
                StartCoroutine(CompleteLevel());
            }
        }

        /// <summary>
        ///     Reset the trigger so it can be activated again
        /// </summary>
        public void ResetTrigger()
        {
            _hasTriggered = false;
        }
    }


}

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

            // Unlock next level and save progress
            UnlockNextLevel();

            // Calculate completion time
            float completionTime =
                Time.time - Time.realtimeSinceStartup; // This should be calculated properly by GameFlowManager

            // Publish level completion event (for GameFlowManager)
            _eventBus?.Publish(new LevelCompletedEvent
            {
                Timestamp = Time.time,
                LevelName = currentLevelName,
                CompletionTime = completionTime
            });

            // Publish level unlocked event (for level selection system)
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                _eventBus?.Publish(new LevelUnlockedEvent
                {
                    Timestamp = Time.time,
                    CompletedLevelName = currentLevelName,
                    UnlockedLevelName = nextLevelName
                });
            }

            // Return to level select if enabled
            if (autoReturnToLevelSelect)
            {
                _eventBus?.Publish(new LevelLoadRequestedEvent
                {
                    Timestamp = Time.time,
                    LevelName = "Level Select",
                    SceneName = "LevelSelection"
                });
            }
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

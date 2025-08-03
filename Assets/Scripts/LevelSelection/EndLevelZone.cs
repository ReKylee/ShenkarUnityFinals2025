using System.Collections;
using System.Collections.Generic;
using Core;
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
        private GameFlowManager _gameFlowManager;
        private bool _hasTriggered;
        private float _levelStartTime;

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
        public void Construct(IEventBus eventBus, IGameDataService gameDataService, GameFlowManager gameFlowManager)
        {
            _eventBus = eventBus;
            _gameDataService = gameDataService;
            _gameFlowManager = gameFlowManager;
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

            // Calculate completion time using proper tracking
            float completionTime = Time.time - _levelStartTime;

            // Update game data with completion stats before notifying GameFlowManager
            UpdateCompletionStats(completionTime);
            UnlockNextLevel();

            // Use GameFlowManager's CompleteLevel method instead of manual event publishing
            if (_gameFlowManager != null)
            {
                _gameFlowManager.CompleteLevel(completionTime);
                Debug.Log($"[EndLevelZone] Notified GameFlowManager of level completion: {completionTime:F2}s");
            }
            else
            {
                // Fallback: publish event directly if GameFlowManager is not available
                _eventBus?.Publish(new LevelCompletedEvent
                {
                    Timestamp = Time.time,
                    LevelName = currentLevelName,
                    CompletionTime = completionTime
                });
            }

            // Publish level unlocked event for level selection system
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
                yield return new WaitForSeconds(1f); // Brief pause to show completion
                SceneTransitionManager.TransitionTo("Level Select");
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

        /// <summary>
        ///     Set the level start time (to be called by GameFlowManager)
        /// </summary>
        public void SetLevelStartTime(float startTime)
        {
            _levelStartTime = startTime;
        }

        private void Start()
        {
            // Subscribe to level started events to track start time
            _eventBus?.Subscribe<LevelStartedEvent>(OnLevelStarted);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
        }

        private void OnLevelStarted(LevelStartedEvent levelEvent)
        {
            if (levelEvent.LevelName == currentLevelName)
            {
                _levelStartTime = levelEvent.Timestamp;
                Debug.Log($"[EndLevelZone] Level {currentLevelName} started at {_levelStartTime}");
            }
        }
    }


}

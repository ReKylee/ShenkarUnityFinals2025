using System;
using GameEvents;
using GameEvents.Interfaces;
using UnityEngine;
using VContainer;

namespace Core
{
    public class LivesManager : MonoBehaviour
    {
        [Header("Lives Settings")]
        [SerializeField] private int maxLives = 3;
        
        private static int _currentLives;
        private static bool _initialized = false;
        
        private IEventBus _eventBus;

        public int CurrentLives => _currentLives;
        public int MaxLives => maxLives;
        public bool HasLives => _currentLives > 0;

        #region VContainer Injection
        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Initialize lives only once (persists across scene reloads)
            if (!_initialized)
            {
                _currentLives = maxLives;
                _initialized = true;
            }
        }

        private void Start()
        {
            // Subscribe to player death events
            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDied);
            
            // Publish current lives state
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = _currentLives,
                MaxLives = maxLives,
                Timestamp = Time.time
            });
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<PlayerDeathEvent>(OnPlayerDied);
        }
        #endregion

        #region Event Handlers
        private void OnPlayerDied(PlayerDeathEvent deathEvent)
        {
            LoseLife();
        }
        #endregion

        #region Public API
        public void LoseLife()
        {
            if (_currentLives <= 0) return;
            
            _currentLives--;
            
            // Publish lives changed event
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = _currentLives,
                MaxLives = maxLives,
                Timestamp = Time.time
            });

            // Check for game over
            if (_currentLives <= 0)
            {
                _eventBus?.Publish(new GameOverEvent
                {
                    Timestamp = Time.time
                });
            }
        }

        public void ResetLives()
        {
            _currentLives = maxLives;
            _initialized = true;
            
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = _currentLives,
                MaxLives = maxLives,
                Timestamp = Time.time
            });
        }

        public void AddLife()
        {
            _currentLives = Mathf.Min(_currentLives + 1, maxLives);
            
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = _currentLives,
                MaxLives = maxLives,
                Timestamp = Time.time
            });
        }
        #endregion

        #region Static Reset (for true game over)
        public static void ResetStatic()
        {
            _initialized = false;
            _currentLives = 0;
        }
        #endregion
    }
}

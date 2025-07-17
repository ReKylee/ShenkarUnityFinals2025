using System;
using System.IO;
using GameEvents;
using GameEvents.Interfaces;
using UnityEngine;
using VContainer;

namespace Core
{
    [Serializable]
    public class GameData
    {
        [Header("Player Data")]
        public int lives = 3;
        public int score = 0;
        public int coins = 0;
        
        [Header("Level Progress")]
        public string currentLevel = "Level_01";
        public float bestTime = float.MaxValue;
        
        [Header("Power-ups")]
        public bool hasFireball = false;
        public bool hasAxe = false;
        
        [Header("Settings")]
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;
    }

    public class PersistentDataManager : MonoBehaviour
    {
        [Header("Default Settings")]
        [SerializeField] private int defaultLives = 3;
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 30f; // Save every 30 seconds
        
        private GameData _gameData;
        private IEventBus _eventBus;
        private float _lastSaveTime;
        
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

        public GameData Data => _gameData;

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
            LoadData();
        }

        private void Start()
        {
            // Subscribe to events that affect persistent data
            _eventBus?.Subscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            
            // Publish initial state
            PublishDataEvents();
        }

        private void Update()
        {
            // Auto-save periodically
            if (autoSave && Time.time - _lastSaveTime > autoSaveInterval)
            {
                SaveData();
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<PlayerDeathEvent>(OnPlayerDied);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            
            // Save on exit
            SaveData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveData(); // Save when app loses focus
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveData(); // Save when app loses focus
        }
        #endregion

        #region Event Handlers
        private void OnPlayerDied(PlayerDeathEvent deathEvent)
        {
            LoseLife();
            if (autoSave) SaveData(); // Save immediately on important events
        }

        private void OnLevelCompleted(LevelCompletedEvent levelEvent)
        {
            if (levelEvent.CompletionTime < _gameData.bestTime)
            {
                _gameData.bestTime = levelEvent.CompletionTime;
                if (autoSave) SaveData();
            }
        }
        #endregion

        #region Public API - Lives
        public void LoseLife()
        {
            if (_gameData.lives <= 0) return;
            
            _gameData.lives--;
            
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = _gameData.lives,
                MaxLives = defaultLives,
                Timestamp = Time.time
            });

            if (_gameData.lives <= 0)
            {
                _eventBus?.Publish(new GameOverEvent
                {
                    Timestamp = Time.time
                });
            }
        }

        public void AddLife()
        {
            _gameData.lives = Mathf.Min(_gameData.lives + 1, defaultLives);
            PublishLivesChanged();
            if (autoSave) SaveData();
        }

        public void ResetLives()
        {
            _gameData.lives = defaultLives;
            PublishLivesChanged();
            if (autoSave) SaveData();
        }
        #endregion

        #region Public API - Score & Coins
        public void AddScore(int points)
        {
            _gameData.score += points;
            PublishScoreChanged();
        }

        public void AddCoins(int coinCount)
        {
            _gameData.coins += coinCount;
            PublishCoinsChanged();
        }

        public void ResetScore()
        {
            _gameData.score = 0;
            PublishScoreChanged();
        }
        #endregion

        #region Public API - Power-ups
        public void UnlockPowerUp(string powerUpName)
        {
            switch (powerUpName.ToLower())
            {
                case "fireball":
                    _gameData.hasFireball = true;
                    break;
                case "axe":
                    _gameData.hasAxe = true;
                    break;
            }
            PublishPowerUpChanged(powerUpName, true);
            if (autoSave) SaveData();
        }

        public void RemovePowerUp(string powerUpName)
        {
            switch (powerUpName.ToLower())
            {
                case "fireball":
                    _gameData.hasFireball = false;
                    break;
                case "axe":
                    _gameData.hasAxe = false;
                    break;
            }
            PublishPowerUpChanged(powerUpName, false);
        }

        public bool HasPowerUp(string powerUpName)
        {
            return powerUpName.ToLower() switch
            {
                "fireball" => _gameData.hasFireball,
                "axe" => _gameData.hasAxe,
                _ => false
            };
        }
        #endregion

        #region Save/Load System
        public void SaveData()
        {
            try
            {
                string json = JsonUtility.ToJson(_gameData, true);
                File.WriteAllText(SaveFilePath, json);
                _lastSaveTime = Time.time;
                
                Debug.Log($"Game data saved to: {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game data: {e.Message}");
            }
        }

        public void LoadData()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _gameData = JsonUtility.FromJson<GameData>(json);
                    
                    Debug.Log($"Game data loaded from: {SaveFilePath}");
                }
                else
                {
                    // Create new save file with default data
                    CreateNewSaveFile();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game data: {e.Message}");
                CreateNewSaveFile();
            }
        }

        private void CreateNewSaveFile()
        {
            _gameData = new GameData
            {
                lives = defaultLives,
                score = 0,
                coins = 0,
                currentLevel = "Level_01",
                bestTime = float.MaxValue,
                hasFireball = false,
                hasAxe = false,
                musicVolume = 1.0f,
                sfxVolume = 1.0f
            };
            
            SaveData();
            Debug.Log("Created new save file with default data");
        }

        public void ResetAllData()
        {
            CreateNewSaveFile();
            PublishDataEvents();
        }

        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("Save file deleted");
                }
                CreateNewSaveFile();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }

        // For debugging - get save file location
        public string GetSaveFilePath() => SaveFilePath;
        #endregion

        #region Event Publishing
        private void PublishDataEvents()
        {
            PublishLivesChanged();
            PublishScoreChanged();
            PublishCoinsChanged();
        }

        private void PublishLivesChanged()
        {
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = _gameData.lives,
                MaxLives = defaultLives,
                Timestamp = Time.time
            });
        }

        private void PublishScoreChanged()
        {
            Debug.Log($"Score: {_gameData.score}");
        }

        private void PublishCoinsChanged()
        {
            Debug.Log($"Coins: {_gameData.coins}");
        }

        private void PublishPowerUpChanged(string powerUpName, bool unlocked)
        {
            Debug.Log($"PowerUp {powerUpName}: {(unlocked ? "Unlocked" : "Removed")}");
        }
        #endregion
    }
}

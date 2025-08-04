using System;
using System.Collections.Generic;
using LevelSelection;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public class GameData
    {
        [Header("Player Data")] 
        public const int MaxLives = 3;
        public int lives = 3;
        public int score;
        public int maxScore; // Track highest score ever achieved

        [Header("Level Progress")] 
        public string currentLevel = "Level_01";

        // Level Selection Data
        public List<string> unlockedLevels = new() { "Level_01" };
        public List<string> completedLevels = new(); // Added for EndLevelZone
        public int selectedLevelIndex;

        // Enhanced timing and scoring data
        public Dictionary<string, float> LevelBestTimes = new();
        public Dictionary<string, int> LevelBestScores = new(); // Best score per level
        public Dictionary<string, bool> LevelCompleted = new();
        
        public float bestTime = float.MaxValue; // Overall best time

        [Header("Settings")] 
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;

        [Header("Collectables")] 
        public int fruitCollected;

        // Cached level discovery data
        public List<LevelData> cachedLevelData = new();
        public bool levelDataCacheValid;

        // Constructor for easy initialization
        public GameData()
        {
            // Default values are set by field initializers
        }

        // Copy constructor
        public GameData(GameData other)
        {
            lives = other.lives;
            score = other.score;
            maxScore = other.maxScore;
            currentLevel = other.currentLevel;
            bestTime = other.bestTime;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            fruitCollected = other.fruitCollected;
            unlockedLevels = new List<string>(other.unlockedLevels);
            completedLevels = new List<string>(other.completedLevels);
            selectedLevelIndex = other.selectedLevelIndex;
            LevelBestTimes = new Dictionary<string, float>(other.LevelBestTimes);
            LevelBestScores = new Dictionary<string, int>(other.LevelBestScores);
            LevelCompleted = new Dictionary<string, bool>(other.LevelCompleted);
            cachedLevelData = new List<LevelData>(other.cachedLevelData);
            levelDataCacheValid = other.levelDataCacheValid;
        }

        // Reset to default values
        public void Reset()
        {
            lives = MaxLives;
            score = 0;
            maxScore = 0;
            currentLevel = "Level_01";
            bestTime = float.MaxValue;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
            fruitCollected = 0;
            
            // Reset level selection data
            unlockedLevels = new List<string> { "Level_01" };
            completedLevels = new List<string>();
            selectedLevelIndex = 0;
            LevelBestTimes = new Dictionary<string, float>();
            LevelBestScores = new Dictionary<string, int>();
            LevelCompleted = new Dictionary<string, bool>();
            cachedLevelData = new List<LevelData>();
            levelDataCacheValid = false;
        }

        /// <summary>
        /// Update best time for a specific level
        /// </summary>
        public void UpdateLevelBestTime(string levelName, float completionTime)
        {
            if (string.IsNullOrEmpty(levelName)) return;
            
            if (!LevelBestTimes.ContainsKey(levelName) || completionTime < LevelBestTimes[levelName])
            {
                LevelBestTimes[levelName] = completionTime;
                
                // Update overall best time if this is better
                if (completionTime < bestTime)
                {
                    bestTime = completionTime;
                }
            }
        }

        /// <summary>
        /// Update best score for a specific level
        /// </summary>
        public void UpdateLevelBestScore(string levelName, int levelScore)
        {
            if (string.IsNullOrEmpty(levelName)) return;
            
            if (!LevelBestScores.ContainsKey(levelName) || levelScore > LevelBestScores[levelName])
            {
                LevelBestScores[levelName] = levelScore;
            }
        }

        /// <summary>
        /// Update max score if current score is higher
        /// </summary>
        public void UpdateMaxScore(int currentScore)
        {
            if (currentScore > maxScore)
            {
                maxScore = currentScore;
            }
        }

        /// <summary>
        /// Get best time for a specific level
        /// </summary>
        public float GetLevelBestTime(string levelName)
        {
            return LevelBestTimes.GetValueOrDefault(levelName, float.MaxValue);
        }

        /// <summary>
        /// Get best score for a specific level
        /// </summary>
        public int GetLevelBestScore(string levelName)
        {
            return LevelBestScores.GetValueOrDefault(levelName, 0);
        }

        /// <summary>
        /// Check if a level is unlocked
        /// </summary>
        public bool IsLevelUnlocked(string levelName)
        {
            return !string.IsNullOrEmpty(levelName) && unlockedLevels.Contains(levelName);
        }

        /// <summary>
        /// Check if a level is completed
        /// </summary>
        public bool IsLevelCompleted(string levelName)
        {
            return !string.IsNullOrEmpty(levelName) && completedLevels.Contains(levelName);
        }

        private static GameData CreateDefaultData() =>
            new()
            {
                lives = MaxLives,
                score = 0,
                currentLevel = "Level_01",
                bestTime = float.MaxValue,
                musicVolume = 1.0f,
                sfxVolume = 1.0f,
                fruitCollected = 0,
                unlockedLevels = new List<string> { "Level_01" },
                selectedLevelIndex = 0,
                LevelBestTimes = new Dictionary<string, float>(),
                LevelCompleted = new Dictionary<string, bool>()
            };
    }
}

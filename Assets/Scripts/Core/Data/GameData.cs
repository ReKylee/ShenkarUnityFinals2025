using System;
using System.Collections.Generic;
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
        public Dictionary<string, float> levelBestTimes = new();
        public Dictionary<string, int> levelBestScores = new(); // Best score per level
        public Dictionary<string, bool> levelCompleted = new();
        
        public float bestTime = float.MaxValue; // Overall best time

        [Header("Power-ups")] 
        public bool hasFireball;
        public bool hasAxe;

        [Header("Settings")] 
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;

        [Header("Collectables")] 
        public int fruitCollected;

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
            hasFireball = other.hasFireball;
            hasAxe = other.hasAxe;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            fruitCollected = other.fruitCollected;
            unlockedLevels = new List<string>(other.unlockedLevels);
            completedLevels = new List<string>(other.completedLevels);
            selectedLevelIndex = other.selectedLevelIndex;
            levelBestTimes = new Dictionary<string, float>(other.levelBestTimes);
            levelBestScores = new Dictionary<string, int>(other.levelBestScores);
            levelCompleted = new Dictionary<string, bool>(other.levelCompleted);
        }

        // Reset to default values
        public void Reset()
        {
            lives = MaxLives;
            score = 0;
            maxScore = 0;
            currentLevel = "Level_01";
            bestTime = float.MaxValue;
            hasFireball = false;
            hasAxe = false;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
            fruitCollected = 0;
            
            // Reset level selection data
            unlockedLevels = new List<string> { "Level_01" };
            completedLevels = new List<string>();
            selectedLevelIndex = 0;
            levelBestTimes = new Dictionary<string, float>();
            levelBestScores = new Dictionary<string, int>();
            levelCompleted = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Update best time for a specific level
        /// </summary>
        public void UpdateLevelBestTime(string levelName, float completionTime)
        {
            if (string.IsNullOrEmpty(levelName)) return;
            
            if (!levelBestTimes.ContainsKey(levelName) || completionTime < levelBestTimes[levelName])
            {
                levelBestTimes[levelName] = completionTime;
                
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
            
            if (!levelBestScores.ContainsKey(levelName) || levelScore > levelBestScores[levelName])
            {
                levelBestScores[levelName] = levelScore;
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
            return levelBestTimes.TryGetValue(levelName, out float time) ? time : float.MaxValue;
        }

        /// <summary>
        /// Get best score for a specific level
        /// </summary>
        public int GetLevelBestScore(string levelName)
        {
            return levelBestScores.TryGetValue(levelName, out int score) ? score : 0;
        }

        private static GameData CreateDefaultData() =>
            new()
            {
                lives = MaxLives,
                score = 0,
                currentLevel = "Level_01",
                bestTime = float.MaxValue,
                hasFireball = false,
                hasAxe = false,
                musicVolume = 1.0f,
                sfxVolume = 1.0f,
                fruitCollected = 0,
                unlockedLevels = new List<string> { "Level_01" },
                selectedLevelIndex = 0,
                levelBestTimes = new Dictionary<string, float>(),
                levelCompleted = new Dictionary<string, bool>()
            };
    }
}

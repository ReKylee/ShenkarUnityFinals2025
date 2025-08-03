using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public class GameData
    {
        [Header("Player Data")] public const int MaxLives = 3;
        [Header("Player Data")] public int lives = 3;

        public int score;

        [Header("Level Progress")] public string currentLevel = "Level_01";

        // Level Selection Data
        public List<string> unlockedLevels = new() { "Level_01" };
        public List<string> completedLevels = new(); // Added for EndLevelZone
        public int selectedLevelIndex;

        public float bestTime = float.MaxValue;

        [Header("Power-ups")] public bool hasFireball;

        public bool hasAxe;

        [Header("Settings")] public float musicVolume = 1.0f;

        public float sfxVolume = 1.0f;

        [Header("Collectables")] public int fruitCollected;
        public Dictionary<string, float> levelBestTimes = new();
        public Dictionary<string, bool> levelCompleted = new();

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
            currentLevel = other.currentLevel;
            bestTime = other.bestTime;
            hasFireball = other.hasFireball;
            hasAxe = other.hasAxe;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            fruitCollected = other.fruitCollected;
            unlockedLevels = new List<string>(other.unlockedLevels);
            selectedLevelIndex = other.selectedLevelIndex;
            levelBestTimes = new Dictionary<string, float>(other.levelBestTimes);
            levelCompleted = new Dictionary<string, bool>(other.levelCompleted);
        }

        // Reset to default values
        public void Reset()
        {
            lives = MaxLives;
            score = 0;
            currentLevel = "Level_01";
            bestTime = float.MaxValue;
            hasFireball = false;
            hasAxe = false;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
            fruitCollected = 0;

            // Reset level selection data
            unlockedLevels = new List<string> { "Level_01" };
            selectedLevelIndex = 0;
            levelBestTimes = new Dictionary<string, float>();
            levelCompleted = new Dictionary<string, bool>();
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

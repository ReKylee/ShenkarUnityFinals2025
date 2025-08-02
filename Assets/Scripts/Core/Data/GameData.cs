using System;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public class GameData
    {
        [Header("Player Data")] public int lives = 3;
        [Header("Player Data")] public const int MaxLives = 3;

        public int score;

        [Header("Level Progress")] public string currentLevel = "Level_01";

        public float bestTime = float.MaxValue;

        [Header("Power-ups")] public bool hasFireball;

        public bool hasAxe;

        [Header("Settings")] public float musicVolume = 1.0f;

        public float sfxVolume = 1.0f;

        [Header("Collectables")]
        public int fruitCollected = 0;

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
                fruitCollected = 0
            };
    }
}

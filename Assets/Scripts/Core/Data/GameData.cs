using System;
using UnityEngine;

namespace Core.Data
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
            coins = other.coins;
            currentLevel = other.currentLevel;
            bestTime = other.bestTime;
            hasFireball = other.hasFireball;
            hasAxe = other.hasAxe;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
        }

        // Reset to default values
        public void Reset()
        {
            lives = 3;
            score = 0;
            coins = 0;
            currentLevel = "Level_01";
            bestTime = float.MaxValue;
            hasFireball = false;
            hasAxe = false;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
        }
    }
}

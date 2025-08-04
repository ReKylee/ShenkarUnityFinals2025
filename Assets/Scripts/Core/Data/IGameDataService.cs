using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LevelSelection;
using LevelSelection.Services;

namespace Core.Data
{
    public interface IGameDataService
    {
        GameData CurrentData { get; }
        void UpdateLives(int lives);
        void UpdateScore(int score);
        void AddFruitCollected();
        void UpdateBestTime(string levelName, float time);
        void UpdateCurrentLevel(string levelName);
        void UnlockLevel(string levelName);
        void ResetAllData();
        void SaveData();
        void ResetProgressData(); // New method for selective reset
        event Action<GameData> OnDataChanged;

        // Level data operations that should go through GameDataService
        void UpdateLevelProgress(string levelName, bool isCompleted, float completionTime);
        Task<List<LevelData>> GetLevelDataAsync(ILevelDiscoveryService discoveryService);
    }
}

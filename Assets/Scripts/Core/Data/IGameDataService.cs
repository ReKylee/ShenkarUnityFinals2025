using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LevelSelection;

namespace Core.Data
{
    public interface IGameDataService
    {
        GameData CurrentData { get; }
        void UpdateLives(int lives);
        void UpdateScore(int score);
        void AddFruitCollected();
        void UpdateBestTime(float time);
        void UpdateCurrentLevel(string levelName);
        void ResetAllData();
        void SaveData();
        event Action<GameData> OnDataChanged;
        
        // Level data operations that should go through GameDataService
        void UpdateLevelProgress(string levelName, bool isCompleted, float completionTime);
        Task<List<LevelData>> DiscoverLevelsAsync();
    }

}

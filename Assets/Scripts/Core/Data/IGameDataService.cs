using System;

namespace Core.Data
{
    public interface IGameDataService
    {
        GameData CurrentData { get; }
        void UpdateLives(int lives);
        void UpdateScore(int score);
        void UpdateBestTime(float time);
        void UpdateCurrentLevel(string levelName);
        void ResetAllData();
        void SaveData();
        event Action<GameData> OnDataChanged;
    }

}

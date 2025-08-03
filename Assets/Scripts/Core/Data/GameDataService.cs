using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LevelSelection;
using LevelSelection.Services;
using VContainer;

namespace Core.Data
{
    public class GameDataService : IGameDataService
    {
        private readonly IGameDataRepository _repository;
        private readonly ILevelDiscoveryService _levelDiscoveryService;

        [Inject]
        public GameDataService(IGameDataRepository repository, ILevelDiscoveryService levelDiscoveryService)
        {
            _repository = repository;
            _levelDiscoveryService = levelDiscoveryService;
            CurrentData = _repository.LoadData();
        }

        public GameData CurrentData { get; private set; }

        public event Action<GameData> OnDataChanged;

        public void UpdateLives(int lives)
        {
            CurrentData.lives = lives;
            NotifyDataChanged();
        }

        public void UpdateScore(int score)
        {
            CurrentData.score = score;
            NotifyDataChanged();
        }


        public void UpdateCurrentLevel(string levelName)
        {
            CurrentData.currentLevel = levelName;
            NotifyDataChanged();
        }

        public void UpdateBestTime(float time)
        {
            if (time < CurrentData.bestTime)
            {
                CurrentData.bestTime = time;
                NotifyDataChanged();
            }
        }

        public void ResetAllData()
        {
            _repository.DeleteData();
            CurrentData = _repository.LoadData();
            NotifyDataChanged();
        }

        public void SaveData()
        {
            _repository.SaveData(CurrentData);
        }

        public void AddFruitCollected()
        {
            CurrentData.fruitCollected++;
            NotifyDataChanged();
        }

        public void UpdateLevelProgress(string levelName, bool isCompleted, float completionTime)
        {
            // Update level progress and cache the data
            _levelDiscoveryService?.UpdateLevelProgress(levelName, isCompleted, completionTime);
            NotifyDataChanged();
        }

        public async Task<List<LevelData>> DiscoverLevelsAsync()
        {
            // Delegate to the level discovery service but cache results in game data
            return await _levelDiscoveryService?.DiscoverLevelsAsync() ?? new List<LevelData>();
        }

        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(CurrentData);
        }
    }
}

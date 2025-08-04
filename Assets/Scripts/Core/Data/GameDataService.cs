using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LevelSelection;
using LevelSelection.Services;
using UnityEngine;
using VContainer;

namespace Core.Data
{
    public class GameDataService : IGameDataService
    {
        private readonly IGameDataRepository _repository;

        [Inject]
        public GameDataService(IGameDataRepository repository)
        {
            _repository = repository;
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

        public void UpdateBestTime(string levelName, float time)
        {
            var gameData = CurrentData;
            if (gameData == null) return;

            // Update overall best time
            if (time < gameData.bestTime)
            {
                gameData.bestTime = time;
            }

            // Update per-level best time
            if (!gameData.LevelBestTimes.ContainsKey(levelName) || time < gameData.LevelBestTimes[levelName])
            {
                gameData.LevelBestTimes[levelName] = time;
            }

            NotifyDataChanged();
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
            var gameData = CurrentData;
            if (gameData == null) return;

            // Update completed levels list
            if (isCompleted && !gameData.completedLevels.Contains(levelName))
            {
                gameData.completedLevels.Add(levelName);
            }

            // Update best time
            if (!gameData.LevelBestTimes.ContainsKey(levelName) || completionTime < gameData.LevelBestTimes[levelName])
            {
                gameData.LevelBestTimes[levelName] = completionTime;
            }

            NotifyDataChanged();
        }

        public void UnlockLevel(string levelName)
        {
            if (CurrentData != null && !string.IsNullOrEmpty(levelName) && !CurrentData.unlockedLevels.Contains(levelName))
            {
                CurrentData.unlockedLevels.Add(levelName);
                NotifyDataChanged();
            }
        }

        public async Task<List<LevelData>> GetLevelDataAsync(ILevelDiscoveryService discoveryService)
        {
            if (CurrentData.levelDataCacheValid && CurrentData.cachedLevelData.Any())
            {
                // Always re-apply default unlock status in case settings changed
                ApplyDefaultUnlockStatus(CurrentData.cachedLevelData);
                return CurrentData.cachedLevelData;
            }

            var discoveredLevels = await discoveryService.DiscoverLevelsFromSceneAsync();
            CacheLevelData(discoveredLevels);
            return discoveredLevels;
        }

        private void CacheLevelData(List<LevelData> levelData)
        {
            CurrentData.cachedLevelData = new List<LevelData>(levelData.OrderBy(l => l.levelIndex));
            CurrentData.levelDataCacheValid = true;

            // Apply default unlock status from level data
            ApplyDefaultUnlockStatus(levelData);

            SaveData();
        }

        /// <summary>
        /// Apply default unlock status from discovered level data
        /// </summary>
        private void ApplyDefaultUnlockStatus(List<LevelData> levelData)
        {
            foreach (LevelData level in levelData.Where(level =>
                         level.UnlockedByDefault && !CurrentData.unlockedLevels.Contains(level.levelName)))
            {
                CurrentData.unlockedLevels.Add(level.levelName);
                Debug.Log($"[GameDataService] Auto-unlocked level: {level.levelName} (UnlockedByDefault = true)");
            }
        }

        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(CurrentData);
        }
    }
}

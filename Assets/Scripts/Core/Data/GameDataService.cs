using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LevelSelection;
using LevelSelection.Services;
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
            var gameData = CurrentData;
            if (gameData?.cachedLevelData == null) return;

            var level = gameData.cachedLevelData.FirstOrDefault(l => l.levelName == levelName);
            if (level != null)
            {
                level.isCompleted = isCompleted;
                if (completionTime < level.bestTime)
                {
                    level.bestTime = completionTime;
                }
            }
            NotifyDataChanged();
        }

        public async Task<List<LevelData>> GetLevelDataAsync(ILevelDiscoveryService discoveryService)
        {
            if (CurrentData.levelDataCacheValid && CurrentData.cachedLevelData.Any())
            {
                return ApplyGameStateToLevelData(CurrentData.cachedLevelData);
            }

            var discoveredLevels = await discoveryService.DiscoverLevelsFromSceneAsync();
            CacheLevelData(discoveredLevels);
            return ApplyGameStateToLevelData(discoveredLevels);
        }

        private void CacheLevelData(List<LevelData> levelData)
        {
            CurrentData.cachedLevelData = new List<LevelData>(levelData);
            CurrentData.levelDataCacheValid = true;
            SaveData();
        }

        private List<LevelData> ApplyGameStateToLevelData(List<LevelData> baseLevelData)
        {
            var gameData = CurrentData;
            if (gameData == null) return baseLevelData;

            var result = new List<LevelData>();
            foreach (var levelData in baseLevelData)
            {
                var copy = new LevelData
                {
                    levelName = levelData.levelName,
                    sceneName = levelData.sceneName,
                    mapPosition = levelData.mapPosition,
                    displayName = levelData.displayName,
                    levelIndex = levelData.levelIndex,
                    isUnlocked = gameData.unlockedLevels.Contains(levelData.levelName),
                    isCompleted = gameData.completedLevels.Contains(levelData.levelName),
                    bestTime = gameData.LevelBestTimes.GetValueOrDefault(levelData.levelName, float.MaxValue)
                };
                result.Add(copy);
            }
            return result;
        }

        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(CurrentData);
        }
    }
}

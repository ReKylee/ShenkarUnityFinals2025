using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Efficient level discovery service that caches results to disk via GameDataService
    ///     Follows Single Responsibility Principle by focusing only on level discovery and caching
    /// </summary>
    public class LevelDiscoveryService : ILevelDiscoveryService
    {
        private readonly IGameDataService _gameDataService;
        private List<LevelData> _runtimeLevelData;

        public LevelDiscoveryService(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
        }

        public async Task<List<LevelData>> DiscoverLevelsAsync()
        {
            // Try to load from cache first
            if (TryLoadFromCache(out List<LevelData> cachedData))
            {
                _runtimeLevelData = cachedData;
                return _runtimeLevelData;
            }

            // Cache miss - discover levels in scene and cache them
            await Task.Yield();
            return await DiscoverAndCacheLevelsAsync();
        }

        public void ClearCache()
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.cachedLevelData.Clear();
                gameData.levelDataCacheValid = false;
                _gameDataService.SaveData();
            }

            _runtimeLevelData = null;
        }

        public void UpdateLevelProgress(string levelName, bool isCompleted, float bestTime)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData?.cachedLevelData == null) return;

            // Update cached data
            var cachedLevel = gameData.cachedLevelData.FirstOrDefault(l => l.levelName == levelName);
            if (cachedLevel != null)
            {
                cachedLevel.isCompleted = isCompleted;
                if (bestTime < cachedLevel.bestTime)
                {
                    cachedLevel.bestTime = bestTime;
                }
            }

            // Update runtime data if available
            var runtimeLevel = _runtimeLevelData?.FirstOrDefault(l => l.levelName == levelName);
            if (runtimeLevel != null)
            {
                runtimeLevel.isCompleted = isCompleted;
                if (bestTime < runtimeLevel.bestTime)
                {
                    runtimeLevel.bestTime = bestTime;
                }
            }
        }

        private bool TryLoadFromCache(out List<LevelData> cachedData)
        {
            cachedData = null;
            GameData gameData = _gameDataService?.CurrentData;

            if (!gameData.levelDataCacheValid || gameData.cachedLevelData == null || gameData.cachedLevelData.Count == 0)
            {
                return false;
            }

            cachedData = ApplyGameStateToLevelData(gameData.cachedLevelData);
            return true;
        }

        private async Task<List<LevelData>> DiscoverAndCacheLevelsAsync()
        {
            _runtimeLevelData = DiscoverLevelDataInScene();
            
            // Cache the discovered data
            CacheLevelData(_runtimeLevelData);
            
            await Task.CompletedTask;
            return _runtimeLevelData;
        }

        private List<LevelData> DiscoverLevelDataInScene()
        {
            var levelPoints = UnityEngine.Object.FindObjectsByType<LevelPoint>(
                FindObjectsInactive.Include, 
                FindObjectsSortMode.None);

            return levelPoints
                .OrderBy(lp => lp.LevelIndex)
                .Select(lp => lp.ToLevelData())
                .ToList();
        }

        private void CacheLevelData(List<LevelData> levelData)
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData == null) return;

            gameData.cachedLevelData = new List<LevelData>(levelData);
            gameData.levelDataCacheValid = true;
            _gameDataService.SaveData();
        }

        private List<LevelData> ApplyGameStateToLevelData(List<LevelData> baseLevelData)
        {
            GameData gameData = _gameDataService?.CurrentData;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private List<LevelPoint> _runtimeLevelPoints;

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

        public List<LevelPoint> GetSortedLevelPoints()
        {
            // Return cached runtime level points if available
            if (_runtimeLevelPoints != null)
            {
                return _runtimeLevelPoints;
            }

            // If we have runtime data but no points, rebuild points from data
            if (_runtimeLevelData != null)
            {
                RebuildLevelPointsFromData();
                return _runtimeLevelPoints;
            }

            // Fallback - discover level points directly from scene
            return DiscoverLevelPointsInScene();
        }

        /// <summary>
        ///     Invalidate the cache to force rediscovery on next call
        /// </summary>
        public void InvalidateCache()
        {
            var gameData = _gameDataService.CurrentData;
            gameData.levelPointsCacheValid = false;
            gameData.cachedLevelPoints.Clear();
            _gameDataService.SaveData();

            _runtimeLevelData = null;
            _runtimeLevelPoints = null;
        }

        /// <summary>
        ///     Update cached level progress data
        /// </summary>
        public void UpdateLevelProgress(string levelName, bool isCompleted, float bestTime = float.MaxValue)
        {
            var gameData = _gameDataService.CurrentData;

            // Update cached level data
            var cachedLevel = gameData.cachedLevelPoints.FirstOrDefault(l => l.levelName == levelName);
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

            // Save to disk
            _gameDataService.SaveData();
        }

        private bool TryLoadFromCache(out List<LevelData> cachedData)
        {
            cachedData = null;

            var gameData = _gameDataService.CurrentData;

            if (!gameData.levelPointsCacheValid || gameData.cachedLevelPoints == null || gameData.cachedLevelPoints.Count == 0)
            {
                return false;
            }

            // Apply current game state to cached data (unlock status, completion, etc.)
            cachedData = ApplyGameStateToLevelData(gameData.cachedLevelPoints);
            return true;
        }

        private async Task<List<LevelData>> DiscoverAndCacheLevelsAsync()
        {
            _runtimeLevelPoints = DiscoverLevelPointsInScene();
            _runtimeLevelData = _runtimeLevelPoints.Select(lp => lp.ToLevelData()).ToList();

            // Cache to game data
            await CacheLevelDataAsync(_runtimeLevelData);

            return _runtimeLevelData;
        }

        private List<LevelPoint> DiscoverLevelPointsInScene()
        {
            var levelPoints = Object.FindObjectsByType<LevelPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID
            );

            // Sort directly without creating intermediate collections
            var sortedPoints = levelPoints.OrderBy(lp => lp.LevelName, StringComparer.OrdinalIgnoreCase).ToList();
            
            return sortedPoints;
        }

        private void RebuildLevelPointsFromData()
        {
            // Find all level points in scene and update them with cached data
            var scenePoints = DiscoverLevelPointsInScene();
            
            // Use dictionary for O(1) lookup instead of O(n) for each point
            var dataLookup = _runtimeLevelData.ToDictionary(d => d.levelName, d => d);
            
            foreach (var point in scenePoints)
            {
                if (dataLookup.TryGetValue(point.LevelName, out var matchingData))
                {
                    point.UpdateFromLevelData(matchingData);
                }
            }
            
            _runtimeLevelPoints = scenePoints;
        }

        private List<LevelData> ApplyGameStateToLevelData(List<LevelData> cachedLevelData)
        {
            var gameData = _gameDataService.CurrentData;
            
            // Create new list to avoid modifying cached data
            var result = new List<LevelData>(cachedLevelData.Count);

            foreach (var levelData in cachedLevelData)
            {
                // Create a copy and apply current game state
                var updatedData = new LevelData
                {
                    levelName = levelData.levelName,
                    sceneName = levelData.sceneName,
                    mapPosition = levelData.mapPosition,
                    displayName = levelData.displayName,
                    levelIndex = levelData.levelIndex,
                    // Apply current game state
                    isUnlocked = gameData.unlockedLevels.Contains(levelData.levelName),
                    isCompleted = gameData.completedLevels.Contains(levelData.levelName),
                    bestTime = gameData.LevelBestTimes.TryGetValue(levelData.levelName, out float bestTime) ? bestTime : levelData.bestTime
                };
                
                result.Add(updatedData);
            }

            return result;
        }

        private async Task CacheLevelDataAsync(List<LevelData> levelData)
        {
            await Task.Yield();

            var gameData = _gameDataService.CurrentData;
            gameData.cachedLevelPoints = new List<LevelData>(levelData);
            gameData.levelPointsCacheValid = true;

            // Save to disk
            _gameDataService.SaveData();
        }
    }
}

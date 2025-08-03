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
    ///     Efficient level discovery service that caches results
    /// </summary>
    public class LevelDiscoveryService : ILevelDiscoveryService
    {
        private readonly IGameDataService _gameDataService;
        private List<LevelData> _cachedLevelData;
        private List<LevelPoint> _sortedLevelPoints;

        public LevelDiscoveryService(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task<List<LevelData>> DiscoverLevelsAsync()
        {
            if (_cachedLevelData != null)
            {
                return _cachedLevelData;
            }

            await Task.Yield();

            // Use a more efficient discovery method
            var levelPoints = Object.FindObjectsByType<LevelPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID
            );

            var levelObjects = levelPoints.Select(lp => lp.gameObject).ToList();

            // Sort by multiple criteria for consistent ordering
            levelObjects.Sort((a, b) =>
            {
                // First by name (Level_01, Level_02, etc.)
                int nameComparison = string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
                return nameComparison;
            });

            // Cache sorted level points for external use
            _sortedLevelPoints = levelObjects.Select(obj => obj.GetComponent<LevelPoint>()).ToList();

            // Build level data directly without director pattern
            _cachedLevelData = BuildLevelDataWithGameData(levelObjects);

            Debug.Log(
                $"[LevelDiscoveryService] Discovered {_cachedLevelData.Count} levels and cached {_sortedLevelPoints.Count} sorted level points");

            return _cachedLevelData;
        }

        public List<LevelPoint> GetSortedLevelPoints() => _sortedLevelPoints;

        private List<LevelData> BuildLevelDataWithGameData(List<GameObject> levelObjects)
        {
            var levelDataList = new List<LevelData>();

            // Use injected game data service instead of FindFirstObjectByType
            GameData gameData = _gameDataService?.CurrentData;

            for (int i = 0; i < levelObjects.Count; i++)
            {
                GameObject levelObject = levelObjects[i];
                if (levelObject == null) continue;

                LevelData levelData;

                if (gameData != null)
                {
                    // Use enhanced factory method with game data
                    levelData = LevelDataFactory.CreateFromGameObjectWithGameData(levelObject, i, gameData);
                }
                else
                {
                    // Fallback to basic creation
                    levelData = LevelDataFactory.CreateFromGameObject(levelObject, i);
                }

                if (levelData == null)
                {
                    // Final fallback - create from transform position and object name
                    levelData = LevelDataFactory.CreateFromTransform(levelObject, i);
                }

                if (levelData != null)
                {
                    Debug.Log(
                        $"Level {i}: {levelData.levelName} at position {levelData.mapPosition} (Unlocked: {levelData.isUnlocked}, Completed: {levelData.isCompleted})");

                    levelDataList.Add(levelData);
                }
            }

            return levelDataList;
        }
    }
}

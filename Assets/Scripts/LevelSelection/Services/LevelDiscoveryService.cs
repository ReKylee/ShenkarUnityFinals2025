using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Efficient level discovery service that caches results
    /// </summary>
    public class LevelDiscoveryService : ILevelDiscoveryService
    {
        private readonly LevelSelectionDirector _director = new();
        private List<LevelData> _cachedLevelData;
        private List<LevelPoint> _sortedLevelPoints;

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
                if (nameComparison != 0) return nameComparison;

                // Then by position (left to right, top to bottom)
                Vector3 posA = a.transform.position;
                Vector3 posB = b.transform.position;

                if (Mathf.Abs(posA.y - posB.y) > 0.1f)
                    return posB.y.CompareTo(posA.y); // Higher Y first

                return posA.x.CompareTo(posB.x); // Left to right
            });

            // Cache sorted level points for external use
            _sortedLevelPoints = levelObjects.Select(obj => obj.GetComponent<LevelPoint>()).ToList();

            _cachedLevelData = _director.BuildLevelData(levelObjects);

            Debug.Log($"[LevelDiscoveryService] Discovered {_cachedLevelData.Count} levels and cached {_sortedLevelPoints.Count} sorted level points");
            return _cachedLevelData;
        }

        public List<LevelPoint> GetSortedLevelPoints()
        {
            return _sortedLevelPoints;
        }
    }
}

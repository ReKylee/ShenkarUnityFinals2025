using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Efficient level discovery service that caches results to disk via GameDataService
    ///     Follows Single Responsibility Principle by focusing only on level discovery and caching
    /// </summary>
    public class LevelDiscoveryService : ILevelDiscoveryService
    {


        public async Task<List<LevelData>> DiscoverLevelsFromSceneAsync()
        {
            // This can be an expensive operation, so keep it async
            await Task.Yield(); 

            var levelPoints = UnityEngine.Object.FindObjectsByType<LevelPoint>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            return levelPoints
                .OrderBy(lp => lp.LevelIndex)
                .Select(lp => lp.ToLevelData())
                .ToList();
        }
    }
}

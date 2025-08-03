using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelSelection.Services
{

    /// <summary>
    ///     Service responsible for discovering and managing level data with disk caching
    /// </summary>
    public interface ILevelDiscoveryService
    {
        Task<List<LevelData>> DiscoverLevelsAsync();
        List<LevelPoint> GetSortedLevelPoints();
        void InvalidateCache();
        void UpdateLevelProgress(string levelName, bool isCompleted, float bestTime = float.MaxValue);
    }
}

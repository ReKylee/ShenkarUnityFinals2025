using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelSelection.Services
{

    /// <summary>
    ///     Service responsible for discovering and managing level data
    /// </summary>
    public interface ILevelDiscoveryService
    {
        Task<List<LevelData>> DiscoverLevelsAsync();
        List<LevelPoint> GetSortedLevelPoints();
    }
}

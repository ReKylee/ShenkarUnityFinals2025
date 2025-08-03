using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for discovering level data from the scene.
    /// </summary>
    public interface ILevelDiscoveryService
    {
        Task<List<LevelData>> DiscoverLevelsFromSceneAsync();
    }
}

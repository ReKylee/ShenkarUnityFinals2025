using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LevelSelection.Services
{

    /// <summary>
    ///     Service responsible for managing level display and visual updates
    /// </summary>
    public interface ILevelDisplayService : IDisposable
    {
        Task InitializeAsync(List<LevelData> levelData);
        void SetLevelPoints(List<LevelPoint> levelPoints);
        void SetConfig(LevelSelectionConfig config);
        void RefreshVisuals();
        void Activate();
        void Deactivate();
    }
}

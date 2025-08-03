using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    ///     Service responsible for discovering level components in the scene
    ///     Decoupled from specific GameObject hierarchy
    /// </summary>
    public interface ILevelDiscoveryService
    {
        Task<List<LevelData>> DiscoverLevelsAsync();
        List<LevelPoint> GetSortedLevelPoints();
    }

    /// <summary>
    ///     Service responsible for handling level navigation
    ///     Independent of input system implementation
    /// </summary>
    public interface ILevelNavigationService
    {
        int CurrentIndex { get; }
        LevelData CurrentLevel { get; }
        Task InitializeAsync(List<LevelData> levelData);
        void Activate();
        void Deactivate();
        void NavigateInDirection(Vector2 direction);
        void SelectCurrentLevel();
        void SetGridWidth(int gridWidth);
        void SetCurrentIndex(int index);
    }

    /// <summary>
    ///     Service responsible for visual display and feedback
    ///     Can work with any visual components in the scene
    /// </summary>
    public interface ILevelDisplayService
    {
        Task InitializeAsync(List<LevelData> levelData);
        void Activate();
        void Deactivate();
        void UpdateSelection(int newIndex);
        void SetConfig(LevelSelectionConfig config);
        void SetLevelPoints(List<LevelPoint> sortedLevelPoints);
        void RefreshVisuals();
        void Dispose();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelSelection.Services
{

    /// <summary>
    ///     Service responsible for handling level navigation logic
    /// </summary>
    public interface ILevelNavigationService
    {
        int CurrentIndex { get; }
        LevelData CurrentLevel { get; }

        Task InitializeAsync(List<LevelData> levelData);
        void NavigateInDirection(Vector2 direction);
        void SelectCurrentLevel();
        void SetCurrentIndex(int index);
        void SetGridWidth(int gridWidth);
        void Activate();
        void Deactivate();
    }
}

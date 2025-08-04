using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Navigation service that works independently of GameObject structure
    ///     Handles all navigation logic and input processing
    /// </summary>
    public class LevelNavigationService : ILevelNavigationService
    {
        private readonly GameDataCoordinator _gameDataCoordinator;

        private readonly GameFlowManager _gameFlowManager;
        private int _gridWidth = 4; // Default value, will be updated from config
        private bool _isActive;
        private List<LevelData> _levelData;

        public LevelNavigationService(GameFlowManager gameFlowManager, GameDataCoordinator gameDataCoordinator)
        {
            _gameFlowManager = gameFlowManager;
            _gameDataCoordinator = gameDataCoordinator;
        }

        public int CurrentIndex { get; private set; }

        public LevelData CurrentLevel => _levelData?[CurrentIndex];

        public async Task InitializeAsync(List<LevelData> levelData)
        {
            _levelData = levelData;

            // Load saved selection from game data coordinator
            int savedIndex = _gameDataCoordinator?.GetSelectedLevelIndex() ?? 0;
            CurrentIndex = Mathf.Clamp(savedIndex, 0, levelData.Count - 1);

            Debug.Log(
                $"[LevelNavigationService] Initialized with CurrentIndex: {CurrentIndex} (saved: {savedIndex}, total levels: {levelData.Count})");

            await Task.CompletedTask;
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void NavigateInDirection(Vector2 direction)
        {
            if (!_isActive || _levelData == null || _levelData.Count == 0) return;

            int newIndex = CalculateNewIndex(direction);
            UpdateSelection(newIndex);
        }

        public void SelectCurrentLevel()
        {
            if (!_isActive || _levelData == null || CurrentIndex >= _levelData.Count) return;

            LevelData selectedLevel = _levelData[CurrentIndex];

            bool isUnlocked = _gameDataCoordinator?.IsLevelUnlocked(selectedLevel.levelName) ?? false;

            Debug.Log(
                $"[LevelNavigationService] Attempting to select level: {selectedLevel.levelName} (Unlocked: {isUnlocked})");

            if (!isUnlocked)
            {
                Debug.Log($"[LevelNavigationService] Level {selectedLevel.levelName} is locked");
                return;
            }

            Debug.Log(
                $"[LevelNavigationService] Requesting level selection through GameFlowManager for: {selectedLevel.levelName}");

            _gameFlowManager?.SelectLevel(selectedLevel.levelName, CurrentIndex);
        }

        public void SetGridWidth(int gridWidth)
        {
            _gridWidth = gridWidth;
            Debug.Log($"[LevelNavigationService] Grid width set to {_gridWidth}");
        }

        public void SetCurrentIndex(int index)
        {
            if (_levelData == null || index < 0 || index >= _levelData.Count) return;

            UpdateSelection(index);
        }

        private int CalculateNewIndex(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement
                return direction.x > 0
                    ? Mathf.Min(CurrentIndex + 1, _levelData.Count - 1)
                    : Mathf.Max(CurrentIndex - 1, 0);
            }

            // Vertical movement - now uses the actual grid width from config
            int newIndex = direction.y > 0
                ? CurrentIndex - _gridWidth
                : CurrentIndex + _gridWidth;

            return Mathf.Clamp(newIndex, 0, _levelData.Count - 1);
        }

        private void UpdateSelection(int newIndex)
        {
            if (newIndex == CurrentIndex)
            {
                return;
            }

            int previousIndex = CurrentIndex;
            CurrentIndex = newIndex;

            // Only handle FLOW - let GameDataCoordinator handle the DATA when it receives this event
            _gameFlowManager?.NavigateLevel(previousIndex, CurrentIndex, Vector2.zero);
        }
    }
}

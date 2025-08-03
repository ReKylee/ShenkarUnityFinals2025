using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Data;
using Core.Events;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Navigation service that works independently of GameObject structure
    ///     Handles all navigation logic and input processing
    /// </summary>
    public class LevelNavigationService : ILevelNavigationService
    {

        private readonly IEventBus _eventBus;
        private readonly IGameDataService _gameDataService;
        private int _gridWidth = 4; // Default value, will be updated from config
        private bool _isActive;
        private List<LevelData> _levelData;

        public LevelNavigationService(IEventBus eventBus, IGameDataService gameDataService)
        {
            _eventBus = eventBus;
            _gameDataService = gameDataService;
        }

        public int CurrentIndex { get; private set; }

        public LevelData CurrentLevel => _levelData?[CurrentIndex];

        public async Task InitializeAsync(List<LevelData> levelData)
        {
            _levelData = levelData;

            // Load saved selection from game data
            GameData gameData = _gameDataService?.CurrentData;
            CurrentIndex = Mathf.Clamp(gameData?.selectedLevelIndex ?? 0, 0, levelData.Count - 1);

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

            if (!selectedLevel.isUnlocked)
            {
                Debug.Log($"Level {selectedLevel.levelName} is locked");
                return;
            }

            _eventBus?.Publish(new LevelSelectedEvent
            {
                Timestamp = Time.time,
                LevelName = selectedLevel.levelName,
                LevelIndex = CurrentIndex
            });
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
            if (newIndex == CurrentIndex) return;

            int previousIndex = CurrentIndex;
            CurrentIndex = newIndex;

            // Save selection immediately
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.selectedLevelIndex = CurrentIndex;
                _gameDataService?.SaveData();
            }

            // Publish navigation event
            _eventBus?.Publish(new LevelNavigationEvent
            {
                Timestamp = Time.time,
                PreviousIndex = previousIndex,
                NewIndex = CurrentIndex,
                Direction = Vector2.zero
            });
        }

        public void SetGridWidth(int gridWidth)
        {
            _gridWidth = gridWidth;
            Debug.Log($"[LevelNavigationService] Grid width set to {_gridWidth}");
        }
    }
}

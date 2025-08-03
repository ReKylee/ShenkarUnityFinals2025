using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Display service that manages level state without visuals
    ///     Since levels are invisible points, this focuses on data management
    /// </summary>
    public class LevelDisplayService : ILevelDisplayService
    {
        private readonly IEventBus _eventBus;
        private LevelSelectionConfig _config;
        private int _currentSelection;
        private bool _isActive;
        private List<LevelData> _levelData;
        private List<LevelPoint> _levelPoints;

        public LevelDisplayService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
        }

        public async Task InitializeAsync(List<LevelData> levelData)
        {
            _levelData = levelData;
            Debug.Log($"[LevelDisplayService] Initialized with {_levelData.Count} level data entries");
            await Task.CompletedTask;
        }

        public void SetLevelPoints(List<LevelPoint> sortedLevelPoints)
        {
            _levelPoints = sortedLevelPoints;
            Debug.Log($"[LevelDisplayService] Received {_levelPoints?.Count ?? 0} sorted level points");
            UpdateLevelStates();
        }

        public void SetConfig(LevelSelectionConfig config)
        {
            _config = config;
            // Config stored but no visual updates needed for invisible points
        }

        public void Activate()
        {
            _isActive = true;
            UpdateLevelStates();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void UpdateSelection(int newIndex)
        {
            if (!_isActive || newIndex == _currentSelection) return;

            _currentSelection = newIndex;
            UpdateLevelStates();
        }

        private void UpdateLevelStates()
        {
            // Update level point states without visual changes
            if (_levelPoints == null || _levelData == null) return;

            for (int i = 0; i < _levelPoints.Count && i < _levelData.Count; i++)
            {
                if (_levelPoints[i] != null)
                {
                    _levelPoints[i].SetUnlocked(_levelData[i].isUnlocked);
                    _levelPoints[i].SetSelected(i == _currentSelection);
                }
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<LevelNavigationEvent>(OnLevelNavigation);
        }

        private void OnLevelNavigation(LevelNavigationEvent navigationEvent)
        {
            UpdateSelection(navigationEvent.NewIndex);
        }

        public void RefreshVisuals()
        {
            // Renamed to RefreshStates since no visuals
            UpdateLevelStates();
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<LevelNavigationEvent>(OnLevelNavigation);
        }
    }
}

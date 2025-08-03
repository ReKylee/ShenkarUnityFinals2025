using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Display service that manages visual components
    ///     Components are assigned via inspector instead of auto-discovery
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

            // Find level points and sort them to match level data order
            var levelPointComponents = Object.FindObjectsByType<LevelPoint>(FindObjectsSortMode.None);
            _levelPoints = new List<LevelPoint>(levelPointComponents);
            _levelPoints.Sort((a, b) => string.Compare(a.gameObject.name, b.gameObject.name, StringComparison.Ordinal));

            Debug.Log($"[LevelDisplayService] Found {_levelPoints.Count} level points");

            UpdateAllVisuals();
            await Task.CompletedTask;
        }

        public void SetConfig(LevelSelectionConfig config)
        {
            _config = config;
            UpdateAllVisuals(); // Refresh visuals with new config
        }

        public void Activate()
        {
            _isActive = true;
            UpdateAllVisuals();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void UpdateSelection(int newIndex)
        {
            if (!_isActive || newIndex == _currentSelection) return;

            _currentSelection = newIndex;
            UpdateAllVisuals();
        }

        private void UpdateAllVisuals()
        {
            // Update all level point visuals using config colors
            for (int i = 0; i < _levelPoints.Count && i < _levelData.Count; i++)
            {
                if (_levelPoints[i] != null)
                {
                    _levelPoints[i].SetUnlocked(_levelData[i].isUnlocked);
                    _levelPoints[i].SetSelected(i == _currentSelection);

                    // Apply config colors if available
                    if (_config != null && _levelPoints[i].iconRenderer != null)
                    {
                        if (i == _currentSelection)
                        {
                            _levelPoints[i].iconRenderer.color = _config.selectedColor;
                        }
                        else if (_levelData[i].isUnlocked)
                        {
                            _levelPoints[i].iconRenderer.color = _config.unlockedColor;
                        }
                        else
                        {
                            _levelPoints[i].iconRenderer.color = _config.lockedColor;
                        }
                    }
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

        public void Dispose()
        {
            _eventBus?.Unsubscribe<LevelNavigationEvent>(OnLevelNavigation);
        }
    }
}

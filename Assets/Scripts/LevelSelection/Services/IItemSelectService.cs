using System;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for managing item select screen state
    /// </summary>
    public interface IItemSelectService
    {
        bool IsActive { get; }
        event Action<bool> OnStateChanged;

        void Initialize(ItemSelectScreen itemSelectScreen);
        void Initialize(ItemSelectScreen itemSelectScreen, ISceneLoadService sceneLoadService);
        void ShowItemSelect(string levelName, string sceneName, Action onComplete = null);
    }

    /// <summary>
    ///     Manages item select screen state and interactions (Single Responsibility)
    /// </summary>
    public class ItemSelectService : IItemSelectService
    {

        private ItemSelectScreen _itemSelectScreen;
        private ISceneLoadService _sceneLoadService;
        public bool IsActive { get; private set; }
        public event Action<bool> OnStateChanged;

        public void Initialize(ItemSelectScreen itemSelectScreen)
        {
            Initialize(itemSelectScreen, null);
        }

        public void Initialize(ItemSelectScreen itemSelectScreen, ISceneLoadService sceneLoadService)
        {
            _itemSelectScreen = itemSelectScreen;
            _sceneLoadService = sceneLoadService;
        }

        public void ShowItemSelect(string levelName, string sceneName, Action onComplete = null)
        {
            // If no item select screen is available, load directly
            if (_itemSelectScreen == null)
            {
                Debug.Log($"[ItemSelectService] No ItemSelectScreen available, loading level directly: {sceneName}");
                _sceneLoadService?.LoadLevel(sceneName);
                onComplete?.Invoke();
                return;
            }

            SetActive(true);
            _itemSelectScreen.ShowItemSelect(levelName, sceneName, () =>
            {
                SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void SetActive(bool isActive)
        {
            if (IsActive == isActive) return;

            IsActive = isActive;
            OnStateChanged?.Invoke(isActive);
            Debug.Log($"[ItemSelectService] State changed to: {isActive}");
        }
    }
}

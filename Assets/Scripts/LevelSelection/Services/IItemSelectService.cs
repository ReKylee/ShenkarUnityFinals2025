using System;

namespace LevelSelection.Services
{
    /// <summary>
    /// Service responsible for managing item select screen state
    /// </summary>
    public interface IItemSelectService
    {
        bool IsActive { get; }
        event Action<bool> OnStateChanged;
        
        void Initialize(ItemSelectScreen itemSelectScreen);
        void ShowItemSelect(string levelName, string sceneName, Action onComplete = null);
        void SetActive(bool isActive);
    }

    /// <summary>
    /// Manages item select screen state and interactions (Single Responsibility)
    /// </summary>
    public class ItemSelectService : IItemSelectService
    {
        public bool IsActive { get; private set; }
        public event Action<bool> OnStateChanged;

        private ItemSelectScreen _itemSelectScreen;

        public void Initialize(ItemSelectScreen itemSelectScreen)
        {
            _itemSelectScreen = itemSelectScreen;
        }

        public void ShowItemSelect(string levelName, string sceneName, Action onComplete = null)
        {
            if (_itemSelectScreen == null) return;

            SetActive(true);
            _itemSelectScreen.ShowItemSelect(levelName, sceneName, () => {
                SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void SetActive(bool isActive)
        {
            if (IsActive == isActive) return;

            IsActive = isActive;
            OnStateChanged?.Invoke(isActive);
            UnityEngine.Debug.Log($"[ItemSelectService] State changed to: {isActive}");
        }
    }
}

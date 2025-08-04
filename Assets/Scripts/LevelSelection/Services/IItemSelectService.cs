using System;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for managing item select screen state
    /// </summary>
    public interface IItemSelectService
    {
        bool IsActive { get; }
        event Action<bool> OnStateChanged;

        void Initialize(ItemSelectScreen itemSelectScreen, ISceneLoadService sceneLoadService);
        void ShowItemSelect(string levelName, string sceneName, Action onComplete = null);
    }

}

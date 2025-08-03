using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for scene loading and transitions
    /// </summary>
    public interface ISceneLoadService
    {
        void LoadLevel(string sceneName);
        string GetSceneNameForLevel(LevelData levelData);
    }

    /// <summary>
    ///     Handles scene loading logic (Single Responsibility)
    /// </summary>
    public class SceneLoadService : ISceneLoadService
    {
        public void LoadLevel(string sceneName)
        {
            Debug.Log($"[SceneLoadService] Loading scene: {sceneName}");
            SceneTransitionManager.TransitionTo(sceneName);
        }

        public string GetSceneNameForLevel(LevelData levelData)
        {
            // Use the scene name from level data if available
            if (!string.IsNullOrEmpty(levelData?.sceneName))
            {
                return levelData.sceneName;
            }

            // Fallback to level name conversion
            return levelData?.levelName?.Replace(" ", "").Replace("_", "") ?? "DefaultLevel";
        }
    }
}

using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    ///     Scene loading service that uses EasyTransitions for smooth transitions
    /// </summary>
    public class SceneLoadService : ISceneLoadService
    {
        private TransitionSettings _defaultTransition;
        private const float DefaultTransitionDelay = 0f;

        public SceneLoadService()
        {
        }

        // Constructor with TransitionSettings (for manual instantiation)
        public SceneLoadService(TransitionSettings transitionSettings)
        {
            _defaultTransition = transitionSettings;
        }

        public void Initialize(TransitionSettings transitionSettings)
        {
            _defaultTransition = transitionSettings;
        }

        public void LoadLevel(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoadService] Scene name is null or empty");
                return;
            }

            try
            {
                // Use EasyTransitions if available, otherwise fallback to direct scene loading
                if (_defaultTransition != null && TransitionManager.Instance() != null)
                {
                    Debug.Log($"[SceneLoadService] Loading scene with transition: {sceneName}");
                    TransitionManager.Instance().Transition(sceneName, _defaultTransition, DefaultTransitionDelay);
                }
                else
                {
                    Debug.Log($"[SceneLoadService] Loading scene directly (no transition): {sceneName}");
                    SceneManager.LoadScene(sceneName);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneLoadService] Failed to load scene {sceneName}: {e}");
                // Fallback to direct scene loading
                SceneManager.LoadScene(sceneName);
            }
        }

        public string GetSceneNameForLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogWarning("[SceneLoadService] LevelData is null, cannot get scene name");
                return string.Empty;
            }

            // Return the scene name from the level data
            return !string.IsNullOrEmpty(levelData.sceneName) ? levelData.sceneName : levelData.levelName;
        }
    }
}

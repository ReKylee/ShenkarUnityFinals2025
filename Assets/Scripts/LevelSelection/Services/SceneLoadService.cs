using System;
using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelSelection.Services
{
    /// <summary>
    ///     MonoBehaviour scene loading service that uses EasyTransitions for smooth transitions
    ///     Should be placed on the same GameObject as the TransitionManager
    /// </summary>
    public class SceneLoadService : MonoBehaviour, ISceneLoadService
    {

        private const float DefaultTransitionDelay = 0f;

        [Header("Transition Settings")] [SerializeField]
        private TransitionSettings defaultTransition;

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
                if (defaultTransition && TransitionManager.Instance())
                {
                    Debug.Log($"[SceneLoadService] Loading scene with transition: {sceneName}");
                    TransitionManager.Instance().Transition(sceneName, defaultTransition, DefaultTransitionDelay);
                }
                else
                {
                    Debug.Log($"[SceneLoadService] Loading scene directly (no transition): {sceneName}");
                    SceneManager.LoadScene(sceneName);
                }
            }
            catch (Exception e)
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

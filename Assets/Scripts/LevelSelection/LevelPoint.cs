using System;
using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    /// Simple Unity GameObject component for level placement in editor.
    /// Only contains basic editor-friendly data that gets converted to LevelData at runtime.
    /// </summary>
    public class LevelPoint : MonoBehaviour
    {
        [Header("Basic Level Info")]
        [SerializeField] private string levelName;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private int levelIndex;

        // Public properties for easy access during conversion
        public string LevelName => levelName;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public int LevelIndex => levelIndex;
        public Vector3 Position => transform.position;

        /// <summary>
        /// Convert this editor placement object to runtime data
        /// </summary>
        public LevelData ToLevelData()
        {
            return new LevelData
            {
                levelName = this.levelName,
                displayName = this.displayName,
                sceneName = this.sceneName,
                levelIndex = this.levelIndex,
                mapPosition = transform.position,
                // Runtime data will be populated from GameData
                isUnlocked = false,
                isCompleted = false,
                bestTime = float.MaxValue
            };
        }

        private void OnValidate()
        {
            // Auto-generate display name if empty
            if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(levelName))
            {
                displayName = levelName.Replace("_", " ");
            }

            // Auto-generate scene name if empty
            if (string.IsNullOrEmpty(sceneName) && !string.IsNullOrEmpty(levelName))
            {
                sceneName = levelName;
            }
        }

        private void OnDrawGizmos()
        {
            // Draw a simple gizmo in editor for easy visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            // Draw level index
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, levelIndex.ToString());
            #endif
        }
    }
}

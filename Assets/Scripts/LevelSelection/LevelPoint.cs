using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    ///     Simple level placement component for the level select scene.
    ///     Just drag this onto a GameObject and set the basic info.
    /// </summary>
    public class LevelPoint : MonoBehaviour
    {
        [Header("Level Info")]
        [SerializeField] private string levelName = "Level_01";
        [SerializeField] private string sceneName = "Level1";
        [SerializeField] private string displayName = "Level 1";

        [Header("Level Settings")]
        [SerializeField] private bool unlockedByDefault = false;

        // Auto-calculated index based on position in hierarchy or scene order
        public int LevelIndex { get; private set; }
        public string LevelName => levelName;
        public bool UnlockedByDefault => unlockedByDefault;

        private void OnValidate()
        {
            // Auto-generate display name from level name if empty
            if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(levelName))
            {
                displayName = levelName.Replace("_", " ").Replace("Level", "Level ");
            }

            // Auto-generate scene name from level name if empty
            if (string.IsNullOrEmpty(sceneName) && !string.IsNullOrEmpty(levelName))
            {
                // Convert "Level_01" to "Level1"
                sceneName = levelName.Replace("_", "");
            }
        }

        private void OnDrawGizmos()
        {
            // Draw a simple gizmo for easy visualization
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Draw level index if available
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, $"{LevelIndex}: {displayName}");
#endif
        }

        /// <summary>
        ///     Set the automatically calculated index
        /// </summary>
        public void SetCalculatedIndex(int index)
        {
            LevelIndex = index;
        }

        /// <summary>
        ///     Convert to runtime data
        /// </summary>
        public LevelData ToLevelData() =>
            new()
            {
                levelName = levelName,
                sceneName = sceneName,
                mapPosition = transform.position,
                displayName = displayName,
                levelIndex = LevelIndex
            };
    }
}

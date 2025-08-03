using System;
using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    /// Represents a level point in the level selection scene
    /// Note: MonoBehaviour components are automatically serialized by Unity
    /// </summary>
    public class LevelPoint : MonoBehaviour
    {
        [Header("Level Configuration")] 
        [SerializeField] private string levelName;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private int levelIndex;

        [Header("Level Progress")]
        [SerializeField] private bool isCompleted;
        [SerializeField] private float bestTime = float.MaxValue;

        [Header("Unlock Configuration")] 
        [SerializeField] private bool startUnlocked = true;
        [SerializeField] private bool overrideGameData;
        [Tooltip("If true, this level ignores saved game data and uses the inspector setting")]

        [SerializeField] private bool isUnlocked;

        // Public read-only properties for external access
        public string LevelName => levelName;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public int LevelIndex => levelIndex;
        public bool IsCompleted => isCompleted;
        public float BestTime => bestTime;
        public bool IsUnlocked => isUnlocked;
        public bool StartUnlocked => startUnlocked;
        public bool OverrideGameData => overrideGameData;
        public Vector2 MapPosition => transform.position;

        private void Awake()
        {
            // Set initial unlock state based on inspector setting
            if (overrideGameData)
            {
                SetUnlocked(startUnlocked);
            }
        }

        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
        }

        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
        }

        public void UpdateBestTime(float time)
        {
            if (time < bestTime)
            {
                bestTime = time;
            }
        }

        public void SetSelected(bool selected)
        {
            // Implementation for visual feedback when selected
        }

        /// <summary>
        /// Convert to LevelData for compatibility with existing systems
        /// </summary>
        public LevelData ToLevelData()
        {
            return new LevelData
            {
                levelName = this.levelName,
                sceneName = this.sceneName,
                mapPosition = this.MapPosition,
                isUnlocked = this.isUnlocked,
                isCompleted = this.isCompleted,
                bestTime = this.bestTime,
                displayName = this.displayName,
                levelIndex = this.levelIndex
            };
        }

        /// <summary>
        /// Update this LevelPoint from cached data
        /// </summary>
        public void UpdateFromLevelData(LevelData data)
        {
            if (data == null) return;

            levelName = data.levelName;
            sceneName = data.sceneName;
            displayName = data.displayName;
            levelIndex = data.levelIndex;
            
            // Use proper methods for state changes
            SetUnlocked(data.isUnlocked);
            SetCompleted(data.isCompleted);
            UpdateBestTime(data.bestTime);
        }
    }
}

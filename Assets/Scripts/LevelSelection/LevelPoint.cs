using UnityEngine;

namespace LevelSelection
{
    public class LevelPoint : MonoBehaviour
    {
        [Header("Level Configuration")] 
        public string levelName;

        public string displayName;
        public string sceneName;
        
        [Header("Unlock Configuration")]
        [SerializeField] private bool startUnlocked = true;
        [SerializeField] private bool overrideGameData = false;
        [Tooltip("If true, this level ignores saved game data and uses the inspector setting")]

        public bool IsUnlocked { get; private set; }
        
        /// <summary>
        /// Gets the inspector setting for whether this level starts unlocked
        /// </summary>
        public bool StartUnlocked => startUnlocked;
        
        /// <summary>
        /// Gets whether this level should override saved game data
        /// </summary>
        public bool OverrideGameData => overrideGameData;

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
            IsUnlocked = unlocked;
        }

        public void SetSelected(bool selected)
        {
 
        }
    }
}

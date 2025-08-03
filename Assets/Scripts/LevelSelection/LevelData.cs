using System;
using UnityEngine;

namespace LevelSelection
{
    [Serializable]
    public class LevelData
    {
        public string levelName;
        public string sceneName;
        public Vector2 mapPosition;
        public bool isUnlocked;
        public bool isCompleted;
        public float bestTime = float.MaxValue;
        public string displayName;
        public int levelIndex;
    }

}

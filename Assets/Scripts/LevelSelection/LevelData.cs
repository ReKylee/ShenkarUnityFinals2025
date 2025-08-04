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
        public string displayName;
        public int levelIndex;
        public bool unlockedByDefault;
    }
}

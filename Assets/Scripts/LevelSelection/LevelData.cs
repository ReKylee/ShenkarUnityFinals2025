using System;
using Core.Data;
using UnityEngine;

namespace LevelSelection
{
    [Serializable]
    public class LevelData
    {
        public string levelName;
        public string sceneName;
        public Vector2 mapPosition;
        public bool isUnlocked = false;
        public bool isCompleted = false;
        public float bestTime = float.MaxValue;
        public string displayName;
        public int levelIndex;

    }

}

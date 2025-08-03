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
        public float bestTime;
        public Sprite levelIcon;
        public string displayName;
        public int levelIndex;

        public LevelData()
        {
            isUnlocked = false;
            isCompleted = false;
            bestTime = float.MaxValue;
        }
    }

    public class LevelDataBuilder
    {
        private LevelData _levelData;

        public LevelDataBuilder()
        {
            _levelData = new LevelData();
        }

        public LevelDataBuilder WithName(string levelName)
        {
            _levelData.levelName = levelName;
            return this;
        }

        public LevelDataBuilder WithDisplayName(string displayName)
        {
            _levelData.displayName = displayName;
            return this;
        }

        public LevelDataBuilder WithScene(string sceneName)
        {
            _levelData.sceneName = sceneName;
            return this;
        }

        public LevelDataBuilder AtPosition(Vector2 position)
        {
            _levelData.mapPosition = position;
            return this;
        }

        public LevelDataBuilder WithIndex(int index)
        {
            _levelData.levelIndex = index;
            return this;
        }

        public LevelDataBuilder WithIcon(Sprite icon)
        {
            _levelData.levelIcon = icon;
            return this;
        }

        public LevelDataBuilder Unlocked(bool unlocked = true)
        {
            _levelData.isUnlocked = unlocked;
            return this;
        }

        public LevelDataBuilder Completed(bool completed = true)
        {
            _levelData.isCompleted = completed;
            return this;
        }

        public LevelDataBuilder WithBestTime(float time)
        {
            _levelData.bestTime = time;
            return this;
        }

        public LevelData Build()
        {
            return _levelData;
        }
    }

    public static class LevelDataFactory
    {
        public static LevelData CreateFromGameObject(GameObject levelObject, int index)
        {
            var levelPoint = levelObject.GetComponent<LevelPoint>();
            if (levelPoint == null)
            {
                Debug.LogWarning($"GameObject {levelObject.name} doesn't have a LevelPoint component");
                return null;
            }

            return new LevelDataBuilder()
                .WithName(levelPoint.levelName)
                .WithDisplayName(levelPoint.displayName)
                .WithScene(levelPoint.sceneName)
                .AtPosition(levelObject.transform.position) // Uses the actual GameObject position from scene
                .WithIndex(index)
                .WithIcon(levelPoint.levelIcon)
                .Build();
        }

        public static LevelData CreateDefault(string name, string scene, Vector2 position, int index)
        {
            return new LevelDataBuilder()
                .WithName(name)
                .WithDisplayName(name)
                .WithScene(scene)
                .AtPosition(position) // Uses the provided position (from transform)
                .WithIndex(index)
                .Unlocked(index == 0) // First level is always unlocked
                .Build();
        }

        /// <summary>
        /// Creates level data using only the GameObject's transform position and name
        /// Useful when LevelPoint component is not available
        /// </summary>
        public static LevelData CreateFromTransform(GameObject levelObject, int index, string sceneName = null)
        {
            return new LevelDataBuilder()
                .WithName(levelObject.name)
                .WithDisplayName(levelObject.name)
                .WithScene(sceneName ?? levelObject.name) // Use object name as scene if not provided
                .AtPosition(levelObject.transform.position) // Always use the actual transform position
                .WithIndex(index)
                .Unlocked(index == 0)
                .Build();
        }
    }
}

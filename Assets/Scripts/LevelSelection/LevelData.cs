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
        private readonly LevelData _levelData;

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

        public LevelData Build() => _levelData;
    }

    public static class LevelDataFactory
    {
        public static LevelData CreateFromGameObject(GameObject levelObject, int index)
        {
            LevelPoint levelPoint = levelObject.GetComponent<LevelPoint>();
            if (levelPoint == null)
            {
                Debug.LogWarning($"GameObject {levelObject.name} doesn't have a LevelPoint component");
                return null;
            }

            // Determine unlock status based on inspector settings and game data
            bool isUnlocked = DetermineUnlockStatus(levelPoint, index);

            return new LevelDataBuilder()
                .WithName(levelPoint.levelName)
                .WithDisplayName(levelPoint.displayName)
                .WithScene(levelPoint.sceneName)
                .AtPosition(levelObject.transform.position)
                .WithIndex(index)
                .Unlocked(isUnlocked)
                .Build();
        }

        public static LevelData CreateFromGameObjectWithGameData(GameObject levelObject, int index, Core.Data.GameData gameData)
        {
            LevelPoint levelPoint = levelObject.GetComponent<LevelPoint>();
            if (levelPoint == null)
            {
                Debug.LogWarning($"GameObject {levelObject.name} doesn't have a LevelPoint component");
                return null;
            }

            // Determine unlock status using game data
            bool isUnlocked = DetermineUnlockStatusWithGameData(levelPoint, index, gameData);
            bool isCompleted = gameData?.completedLevels?.Contains(levelPoint.levelName) ?? false;

            return new LevelDataBuilder()
                .WithName(levelPoint.levelName)
                .WithDisplayName(levelPoint.displayName)
                .WithScene(levelPoint.sceneName)
                .AtPosition(levelObject.transform.position)
                .WithIndex(index)
                .Unlocked(isUnlocked)
                .Completed(isCompleted)
                .Build();
        }

        private static bool DetermineUnlockStatus(LevelPoint levelPoint, int index)
        {
            // If level point overrides game data, use inspector setting
            if (levelPoint.OverrideGameData)
            {
                return levelPoint.StartUnlocked;
            }

            // Default behavior: first level is always unlocked
            return index == 0;
        }

        private static bool DetermineUnlockStatusWithGameData(LevelPoint levelPoint, int index, Core.Data.GameData gameData)
        {
            // If level point overrides game data, use inspector setting
            if (levelPoint.OverrideGameData)
            {
                return levelPoint.StartUnlocked;
            }

            // Check if level is in unlocked levels list
            if (gameData?.unlockedLevels != null && gameData.unlockedLevels.Contains(levelPoint.levelName))
            {
                return true;
            }

            // Default behavior: first level is always unlocked
            return index == 0;
        }

        /// <summary>
        ///     Creates level data using only the GameObject's transform position and name
        ///     Useful when LevelPoint component is not available
        /// </summary>
        public static LevelData CreateFromTransform(GameObject levelObject, int index, string sceneName = null) =>
            new LevelDataBuilder()
                .WithName(levelObject.name)
                .WithDisplayName(levelObject.name)
                .WithScene(sceneName ?? levelObject.name) // Use object name as scene if not provided
                .AtPosition(levelObject.transform.position) // Always use the actual transform position
                .WithIndex(index)
                .Unlocked(index == 0)
                .Build();
    }
}

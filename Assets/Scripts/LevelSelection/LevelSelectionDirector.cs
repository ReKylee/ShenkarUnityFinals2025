using System.Collections.Generic;
using UnityEngine;

namespace LevelSelection
{
    /// <summary>
    /// Director pattern implementation for building level data structures
    /// </summary>
    public class LevelSelectionDirector
    {
        public List<LevelData> BuildLevelData(List<GameObject> levelGameObjects)
        {
            var levelDataList = new List<LevelData>();
            
            for (int i = 0; i < levelGameObjects.Count; i++)
            {
                var levelObject = levelGameObjects[i];
                if (levelObject == null) continue;

                // Try to create from LevelPoint component first
                var levelData = LevelDataFactory.CreateFromGameObject(levelObject, i);
                
                if (levelData == null)
                {
                    // Fallback - create from transform position and object name
                    Debug.Log($"Creating level data from transform for {levelObject.name} at position {levelObject.transform.position}");
                    levelData = LevelDataFactory.CreateFromTransform(levelObject, i);
                }
                
                if (levelData != null)
                {
                    Debug.Log($"Level {i}: {levelData.levelName} at position {levelData.mapPosition}");
                    levelDataList.Add(levelData);
                }
            }
            
            return levelDataList;
        }

        public LevelData BuildSingleLevel(GameObject levelObject, int index)
        {
            if (levelObject == null) return null;
            
            // Try LevelPoint component first, fallback to transform-based creation
            return LevelDataFactory.CreateFromGameObject(levelObject, index) ??
                   LevelDataFactory.CreateFromTransform(levelObject, index);
        }
    }
}

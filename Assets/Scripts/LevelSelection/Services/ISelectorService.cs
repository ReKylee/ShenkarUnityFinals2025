using System.Collections.Generic;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for managing selector visual state and movement
    /// </summary>
    public interface ISelectorService
    {
        bool IsMoving { get; }
        void Initialize(GameObject selectorObject, LevelSelectionConfig config);
        void MoveToPosition(Vector3 targetPosition);
        void SetVisible(bool visible);
        void Update();
        void SetLevelPoints(List<LevelPoint> levelPoints);
        void MoveToLevel(int levelIndex);
    }
}

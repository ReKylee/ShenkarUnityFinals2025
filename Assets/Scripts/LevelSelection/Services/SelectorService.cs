using System.Collections.Generic;
using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Handles selector visual state and movement (Single Responsibility)
    /// </summary>
    public class SelectorService : ISelectorService
    {
        private LevelSelectionConfig _config;
        private List<LevelPoint> _levelPoints;
        private GameObject _selectorObject;
        private Vector3 _targetPosition;

        public bool IsMoving { get; private set; }

        public void Initialize(GameObject selectorObject, LevelSelectionConfig config)
        {
            _selectorObject = selectorObject;
            _config = config;
        }

        public void SetLevelPoints(List<LevelPoint> levelPoints)
        {
            _levelPoints = levelPoints;
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            if (_selectorObject == null) return;

            float distance = Vector3.Distance(_selectorObject.transform.position, targetPosition);
            if (distance > 0.01f)
            {
                _targetPosition = targetPosition;
                IsMoving = true;
                Debug.Log($"[SelectorService] Moving selector to position {targetPosition}");
            }
        }

        public void MoveToLevel(int levelIndex)
        {
            if (_levelPoints == null || levelIndex < 0 || levelIndex >= _levelPoints.Count) return;

            Vector3 targetPosition = _levelPoints[levelIndex].transform.position;
            MoveToPosition(targetPosition);
        }

        public void SetVisible(bool visible)
        {
            if (_selectorObject != null)
            {
                _selectorObject.SetActive(visible);
                Debug.Log($"[SelectorService] Selector visibility set to: {visible}");
            }
        }

        public void Update()
        {
            if (!IsMoving || _selectorObject == null) return;

            float moveSpeed = _config?.selectorMoveSpeed ?? 5f;
            float snapThreshold = _config?.snapThreshold ?? 0.1f;

            _selectorObject.transform.position = Vector3.MoveTowards(
                _selectorObject.transform.position,
                _targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(_selectorObject.transform.position, _targetPosition) < snapThreshold)
            {
                _selectorObject.transform.position = _targetPosition;
                IsMoving = false;
            }
        }
    }
}

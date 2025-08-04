using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Handles selector visual state and movement (Single Responsibility)
    /// </summary>
    public class SelectorService : ISelectorService
    {
        private const float DefaultMoveSpeed = 5f;
        private const float DefaultSnapThreshold = 0.1f;

        private GameObject _selectorObject;
        private Vector3 _targetPosition;

        public bool IsMoving { get; private set; }

        public void Initialize(GameObject selectorObject)
        {
            _selectorObject = selectorObject;
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            if (!_selectorObject) return;

            float distance = Vector3.Distance(_selectorObject.transform.position, targetPosition);
            if (distance > 0.01f)
            {
                _targetPosition = new Vector3(targetPosition.x, targetPosition.y, _selectorObject.transform.position.z);
                IsMoving = true;
            }
        }

        public void MoveToCurrentLevel(ILevelNavigationService navigationService)
        {
            if (navigationService?.CurrentLevel != null)
            {
                MoveToPosition(navigationService.CurrentLevel.mapPosition);
            }
        }

        public void SetVisible(bool visible)
        {
            if (_selectorObject != null)
            {
                _selectorObject.SetActive(visible);
            }
        }

        public void Update()
        {
            if (!IsMoving || _selectorObject == null) return;

            _selectorObject.transform.position = Vector3.MoveTowards(
                _selectorObject.transform.position,
                _targetPosition,
                DefaultMoveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(_selectorObject.transform.position, _targetPosition) < DefaultSnapThreshold)
            {
                _selectorObject.transform.position = _targetPosition;
                IsMoving = false;
            }
        }
    }
}

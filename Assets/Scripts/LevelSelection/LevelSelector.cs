using System.Collections.Generic;
using Core.Events;
using UnityEngine;
using VContainer;

namespace LevelSelection
{
    public class LevelSelector : MonoBehaviour
    {
        [Header("Selector Configuration")] public GameObject selectorObject;

        public float moveSpeed = 5f;
        public AudioClip navigationSound;
        public AudioClip selectionSound;
        public AudioClip lockedSound;

        [Header("Grid Navigation")] public int gridWidth = 4;

        public float snapThreshold = 0.1f;
        private AudioSource _audioSource;

        private List<LevelData> _availableLevels;
        private IEventBus _eventBus;
        private List<LevelPoint> _levelPoints;
        private Vector3 _targetPosition;

        public int CurrentIndex { get; private set; }

        public LevelData CurrentLevel => _availableLevels?[CurrentIndex];
        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (IsMoving && selectorObject != null)
            {
                selectorObject.transform.position = Vector3.MoveTowards(
                    selectorObject.transform.position,
                    _targetPosition,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(selectorObject.transform.position, _targetPosition) < snapThreshold)
                {
                    selectorObject.transform.position = _targetPosition;
                    IsMoving = false;
                }
            }
        }

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Initialize(List<LevelData> levels, List<LevelPoint> levelPoints, int selectedIndex = 0)
        {
            _availableLevels = levels;
            _levelPoints = levelPoints;
            CurrentIndex = Mathf.Clamp(selectedIndex, 0, levels.Count - 1);

            UpdateLevelStates();
            MoveToCurrentLevel(true); // Instant move on initialization
        }

        public void Navigate(Vector2 direction)
        {
            if (IsMoving || _availableLevels == null || _availableLevels.Count == 0)
                return;

            int newIndex = CalculateNewIndex(direction);

            if (newIndex != CurrentIndex && newIndex >= 0 && newIndex < _availableLevels.Count)
            {
                int previousIndex = CurrentIndex;
                CurrentIndex = newIndex;

                PlaySound(navigationSound);
                UpdateSelection();
                MoveToCurrentLevel();

                _eventBus?.Publish(new LevelNavigationEvent
                {
                    Timestamp = Time.time,
                    PreviousIndex = previousIndex,
                    NewIndex = CurrentIndex,
                    Direction = direction
                });
            }
        }

        private int CalculateNewIndex(Vector2 direction)
        {
            // Adventure Island III style navigation - mostly horizontal with some vertical
            int currentRow = CurrentIndex / gridWidth;
            int currentCol = CurrentIndex % gridWidth;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement
                if (direction.x > 0) // Right
                {
                    return Mathf.Min(CurrentIndex + 1, _availableLevels.Count - 1);
                }

                // Left
                return Mathf.Max(CurrentIndex - 1, 0);
            }

            // Vertical movement
            if (direction.y > 0) // Up
            {
                int newIndex = CurrentIndex - gridWidth;
                return newIndex >= 0 ? newIndex : CurrentIndex;
            }
            else // Down
            {
                int newIndex = CurrentIndex + gridWidth;
                return newIndex < _availableLevels.Count ? newIndex : CurrentIndex;
            }
        }

        public void SelectCurrentLevel()
        {
            if (_availableLevels == null || CurrentIndex >= _availableLevels.Count)
                return;

            LevelData selectedLevel = _availableLevels[CurrentIndex];

            if (!selectedLevel.isUnlocked)
            {
                PlaySound(lockedSound);
                return;
            }

            PlaySound(selectionSound);

            _eventBus?.Publish(new LevelSelectedEvent
            {
                Timestamp = Time.time,
                LevelName = selectedLevel.levelName,
                LevelIndex = CurrentIndex
            });
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < _levelPoints.Count; i++)
            {
                if (_levelPoints[i] != null)
                {
                    _levelPoints[i].SetSelected(i == CurrentIndex);
                }
            }
        }

        private void UpdateLevelStates()
        {
            for (int i = 0; i < _levelPoints.Count && i < _availableLevels.Count; i++)
            {
                if (_levelPoints[i] != null)
                {
                    _levelPoints[i].SetUnlocked(_availableLevels[i].isUnlocked);
                    _levelPoints[i].SetSelected(i == CurrentIndex);
                }
            }
        }

        private void MoveToCurrentLevel(bool instant = false)
        {
            if (CurrentIndex >= _levelPoints.Count || _levelPoints[CurrentIndex] == null)
                return;

            _targetPosition = _levelPoints[CurrentIndex].transform.position;

            if (instant)
            {
                selectorObject.transform.position = _targetPosition;
                IsMoving = false;
            }
            else
            {
                IsMoving = true;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource && clip)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
    }
}

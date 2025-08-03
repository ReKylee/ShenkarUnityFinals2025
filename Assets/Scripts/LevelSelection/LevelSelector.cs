using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using UnityEngine;
using VContainer;

namespace LevelSelection
{
    public class LevelSelector : MonoBehaviour
    {
        [Header("Selector Configuration")]
        public GameObject selectorObject;
        public float moveSpeed = 5f;
        public AudioClip navigationSound;
        public AudioClip selectionSound;
        public AudioClip lockedSound;
        
        [Header("Grid Navigation")]
        public int gridWidth = 4;
        public float snapThreshold = 0.1f;
        
        private List<LevelData> _availableLevels;
        private List<LevelPoint> _levelPoints;
        private int _currentIndex = 0;
        private bool _isMoving = false;
        private Vector3 _targetPosition;
        private IEventBus _eventBus;
        private AudioSource _audioSource;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void Initialize(List<LevelData> levels, List<LevelPoint> levelPoints, int selectedIndex = 0)
        {
            _availableLevels = levels;
            _levelPoints = levelPoints;
            _currentIndex = Mathf.Clamp(selectedIndex, 0, levels.Count - 1);
            
            UpdateLevelStates();
            MoveToCurrentLevel(true); // Instant move on initialization
        }

        public void Navigate(Vector2 direction)
        {
            if (_isMoving || _availableLevels == null || _availableLevels.Count == 0)
                return;

            int newIndex = CalculateNewIndex(direction);
            
            if (newIndex != _currentIndex && newIndex >= 0 && newIndex < _availableLevels.Count)
            {
                int previousIndex = _currentIndex;
                _currentIndex = newIndex;
                
                PlaySound(navigationSound);
                UpdateSelection();
                MoveToCurrentLevel();
                
                _eventBus?.Publish(new LevelNavigationEvent
                {
                    Timestamp = Time.time,
                    PreviousIndex = previousIndex,
                    NewIndex = _currentIndex,
                    Direction = direction
                });
            }
        }

        private int CalculateNewIndex(Vector2 direction)
        {
            // Adventure Island III style navigation - mostly horizontal with some vertical
            int currentRow = _currentIndex / gridWidth;
            int currentCol = _currentIndex % gridWidth;
            
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement
                if (direction.x > 0) // Right
                {
                    return Mathf.Min(_currentIndex + 1, _availableLevels.Count - 1);
                }
                else // Left
                {
                    return Mathf.Max(_currentIndex - 1, 0);
                }
            }
            else
            {
                // Vertical movement
                if (direction.y > 0) // Up
                {
                    int newIndex = _currentIndex - gridWidth;
                    return newIndex >= 0 ? newIndex : _currentIndex;
                }
                else // Down
                {
                    int newIndex = _currentIndex + gridWidth;
                    return newIndex < _availableLevels.Count ? newIndex : _currentIndex;
                }
            }
        }

        public void SelectCurrentLevel()
        {
            if (_availableLevels == null || _currentIndex >= _availableLevels.Count)
                return;

            var selectedLevel = _availableLevels[_currentIndex];
            
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
                LevelIndex = _currentIndex
            });
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < _levelPoints.Count; i++)
            {
                if (_levelPoints[i] != null)
                {
                    _levelPoints[i].SetSelected(i == _currentIndex);
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
                    _levelPoints[i].SetSelected(i == _currentIndex);
                }
            }
        }

        private void MoveToCurrentLevel(bool instant = false)
        {
            if (_currentIndex >= _levelPoints.Count || _levelPoints[_currentIndex] == null)
                return;

            _targetPosition = _levelPoints[_currentIndex].transform.position;
            
            if (instant)
            {
                selectorObject.transform.position = _targetPosition;
                _isMoving = false;
            }
            else
            {
                _isMoving = true;
            }
        }

        private void Update()
        {
            if (_isMoving && selectorObject != null)
            {
                selectorObject.transform.position = Vector3.MoveTowards(
                    selectorObject.transform.position, 
                    _targetPosition, 
                    moveSpeed * Time.deltaTime
                );
                
                if (Vector3.Distance(selectorObject.transform.position, _targetPosition) < snapThreshold)
                {
                    selectorObject.transform.position = _targetPosition;
                    _isMoving = false;
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource && clip)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        public int CurrentIndex => _currentIndex;
        public LevelData CurrentLevel => _availableLevels?[_currentIndex];
        public bool IsMoving => _isMoving;
    }
}

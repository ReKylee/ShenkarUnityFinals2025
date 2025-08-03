using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;

namespace LevelSelection
{
    public class LevelSelectionManager : MonoBehaviour
    {
        [Header("Level Configuration")] public List<GameObject> levelGameObjects = new();

        public Transform levelContainer;

        [Header("Components")] public LevelSelector levelSelector;

        public ItemSelectScreen itemSelectScreen;
        public NESCrossfade crossfade;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference navigateAction;
        [SerializeField] private InputActionReference submitAction;

        private List<LevelData> _levelData;
        private List<LevelPoint> _levelPoints;
        private IEventBus _eventBus;
        private IGameDataService _gameDataService;
        private bool _isActive = false;
        private LevelSelectionDirector _director;

        public bool IsActive
        {
            get => _isActive;
            private set => _isActive = value;
        }

        private void Awake()
        {
            _director = new LevelSelectionDirector();
            InitializeLevelSelection();
        }

        private void OnEnable()
        {
            if (navigateAction != null)
            {
                navigateAction.action.Enable();
                navigateAction.action.performed += OnNavigate;
            }

            if (submitAction != null)
            {
                submitAction.action.Enable();
                submitAction.action.performed += OnConfirm;
            }
        }

        private void OnDisable()
        {
            if (navigateAction != null)
            {
                navigateAction.action.performed -= OnNavigate;
                navigateAction.action.Disable();
            }

            if (submitAction != null)
            {
                submitAction.action.performed -= OnConfirm;
                submitAction.action.Disable();
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
            _eventBus?.Unsubscribe<LevelLoadRequestedEvent>(OnLevelLoadRequested);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        [Inject]
        public void Construct(IEventBus eventBus, IGameDataService gameDataService)
        {
            _eventBus = eventBus;
            _gameDataService = gameDataService;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<LevelSelectedEvent>(OnLevelSelected);
            _eventBus?.Subscribe<LevelLoadRequestedEvent>(OnLevelLoadRequested);
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void InitializeLevelSelection()
        {
            // Auto-discover level objects if container is specified
            if (levelContainer != null && levelGameObjects.Count == 0)
            {
                for (int i = 0; i < levelContainer.childCount; i++)
                {
                    Transform child = levelContainer.GetChild(i);
                    if (child.GetComponent<LevelPoint>() != null)
                    {
                        levelGameObjects.Add(child.gameObject);
                    }
                }
            }

            // Use director pattern to build level data
            _levelData = _director.BuildLevelData(levelGameObjects);
            _levelPoints = levelGameObjects.Select(go => go.GetComponent<LevelPoint>()).ToList();

            LoadLevelProgressFromGameData();

            GameData gameData = _gameDataService?.CurrentData;
            int selectedIndex = gameData?.selectedLevelIndex ?? 0;

            levelSelector?.Initialize(_levelData, _levelPoints, selectedIndex);
        }

        private void LoadLevelProgressFromGameData()
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData == null) return;

            // Update unlock status based on game data
            for (int i = 0; i < _levelData.Count; i++)
            {
                LevelData level = _levelData[i];
                level.isUnlocked = gameData.unlockedLevels.Contains(level.levelName);

                if (gameData.levelCompleted.TryGetValue(level.levelName, out bool completed))
                {
                    level.isCompleted = completed;
                }

                if (gameData.levelBestTimes.TryGetValue(level.levelName, out float bestTime))
                {
                    level.bestTime = bestTime;
                }
            }
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (!IsActive || levelSelector == null) return;

            Vector2 direction = context.ReadValue<Vector2>();
            levelSelector.Navigate(direction);
            SaveSelectedLevel();
        }

        private void OnConfirm(InputAction.CallbackContext context)
        {
            if (!IsActive || levelSelector == null) return;

            levelSelector.SelectCurrentLevel();
        }

        private void SaveSelectedLevel()
        {
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.selectedLevelIndex = levelSelector.CurrentIndex;
                _gameDataService?.SaveData();
            }
        }

        public void ActivateLevelSelection()
        {
            IsActive = true;
            gameObject.SetActive(true);
        }

        public void DeactivateLevelSelection()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }

        private void OnLevelSelected(LevelSelectedEvent levelEvent)
        {
            LevelData selectedLevel = _levelData.FirstOrDefault(l => l.levelName == levelEvent.LevelName);
            if (selectedLevel == null) return;

            // Update current level in game data
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData != null)
            {
                gameData.currentLevel = selectedLevel.levelName;
                gameData.selectedLevelIndex = levelEvent.LevelIndex;
                _gameDataService?.SaveData();
            }

            // Show item select screen
            _eventBus?.Publish(new ItemSelectScreenRequestedEvent
            {
                Timestamp = Time.time,
                LevelName = selectedLevel.levelName
            });

            itemSelectScreen?.ShowItemSelect(selectedLevel.levelName, selectedLevel.sceneName);
        }

        private void OnLevelLoadRequested(LevelLoadRequestedEvent loadEvent)
        {
            // Start crossfade and load scene
            crossfade?.FadeOutAndIn(
                () =>
                {
                    // Load the scene
                    SceneManager.LoadScene(loadEvent.SceneName);
                },
                () =>
                {
                    // Publish level started event
                    _eventBus?.Publish(new LevelStartedEvent
                    {
                        Timestamp = Time.time,
                        LevelName = loadEvent.LevelName
                    });
                }
            );
        }

        private void OnLevelCompleted(LevelCompletedEvent completedEvent)
        {
            // Update level completion status and unlock next level
            GameData gameData = _gameDataService?.CurrentData;
            if (gameData == null) return;

            // Mark level as completed
            gameData.levelCompleted[completedEvent.LevelName] = true;

            // Update best time
            if (!gameData.levelBestTimes.ContainsKey(completedEvent.LevelName) ||
                completedEvent.CompletionTime < gameData.levelBestTimes[completedEvent.LevelName])
            {
                gameData.levelBestTimes[completedEvent.LevelName] = completedEvent.CompletionTime;
            }

            // Unlock next level
            int completedIndex = _levelData.FindIndex(l => l.levelName == completedEvent.LevelName);
            if (completedIndex >= 0 && completedIndex + 1 < _levelData.Count)
            {
                string nextLevelName = _levelData[completedIndex + 1].levelName;
                if (!gameData.unlockedLevels.Contains(nextLevelName))
                {
                    gameData.unlockedLevels.Add(nextLevelName);
                }
            }

            _gameDataService?.SaveData();

            // Refresh level data if we're still in level selection
            if (IsActive)
            {
                LoadLevelProgressFromGameData();
                levelSelector?.Initialize(_levelData, _levelPoints, levelSelector.CurrentIndex);
            }
        }

        public List<LevelData> GetLevelData() => _levelData;
    }
}

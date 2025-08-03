using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using LevelSelection.Services;

namespace LevelSelection
{
    /// <summary>
    /// Orchestrates level selection functionality using focused services (SOLID principles)
    /// </summary>
    public class LevelSelectionController : MonoBehaviour
    {
        [Header("Auto Configuration")] 
        [SerializeField] private bool autoActivateOnStart = true;
        [SerializeField] private LevelSelectionConfig config;

        [Header("UI Components")] 
        [SerializeField] private GameObject selectorObject;
        [SerializeField] private ItemSelectScreen itemSelectScreen;
        [SerializeField] private NesCrossfade crossfade;

        [Header("Input Actions")] 
        [SerializeField] private InputActionReference navigateAction;
        [SerializeField] private InputActionReference submitAction;

        // Core services - injected via DI
        private ILevelDiscoveryService _discoveryService;
        private ILevelNavigationService _navigationService;
        private ILevelDisplayService _displayService;
        private IEventBus _eventBus;

        // New focused services
        private ISelectorService _selectorService;
        private IInputFilterService _inputFilterService;
        private IAudioFeedbackService _audioFeedbackService;
        private IItemSelectService _itemSelectService;
        private ISceneLoadService _sceneLoadService;

        private AudioSource _audioSource;
        private List<LevelPoint> _sortedLevelPoints;

        public bool IsActive { get; private set; }
        public LevelSelectionConfig Config => config;

        private async void Start()
        {
            await InitializeAsync();

            if (autoActivateOnStart)
            {
                Activate();
            }
        }

        private void Update()
        {
            // Delegate selector movement to the service
            _selectorService?.Update();
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
                submitAction.action.performed += OnSubmit;
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
                submitAction.action.performed -= OnSubmit;
                submitAction.action.Disable();
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<LevelNavigationEvent>(OnLevelNavigation);
            _eventBus?.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
            _eventBus?.Unsubscribe<LevelLoadRequestedEvent>(OnLevelLoadRequested);

            // Properly dispose of display service
            _displayService?.Dispose();
        }

        [Inject]
        public void Construct(
            ILevelDiscoveryService discoveryService,
            ILevelNavigationService navigationService,
            ILevelDisplayService displayService,
            IEventBus eventBus,
            ISelectorService selectorService,
            IInputFilterService inputFilterService,
            IAudioFeedbackService audioFeedbackService,
            IItemSelectService itemSelectService,
            ISceneLoadService sceneLoadService)
        {
            _discoveryService = discoveryService;
            _navigationService = navigationService;
            _displayService = displayService;
            _eventBus = eventBus;
            _selectorService = selectorService;
            _inputFilterService = inputFilterService;
            _audioFeedbackService = audioFeedbackService;
            _itemSelectService = itemSelectService;
            _sceneLoadService = sceneLoadService;

            SubscribeToEvents();
        }

        private async Task InitializeAsync()
        {
            // Setup audio source
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Initialize services
            InitializeServices();

            var levelData = await _discoveryService.DiscoverLevelsAsync();
            await _navigationService.InitializeAsync(levelData);
            await _displayService.InitializeAsync(levelData);

            _sortedLevelPoints = _discoveryService.GetSortedLevelPoints();
            _displayService.SetLevelPoints(_sortedLevelPoints);

            // Configure services with sorted level points
            _selectorService.SetLevelPoints(_sortedLevelPoints);

            // Pass config to legacy services
            if (config != null)
            {
                _displayService.SetConfig(config);
                _navigationService.SetGridWidth(config.gridWidth);

                if (itemSelectScreen)
                {
                    itemSelectScreen.SetConfig(config);
                }

                if (crossfade)
                {
                    crossfade.SetConfig(config);
                }
            }

            Debug.Log($"[LevelSelectionController] Initialized with {levelData.Count} levels using SOLID architecture");
        }

        private void InitializeServices()
        {
            // Initialize all services with their dependencies
            _selectorService.Initialize(selectorObject, config);
            _inputFilterService.Initialize(config);
            _audioFeedbackService.Initialize(_audioSource, config);
            _itemSelectService.Initialize(itemSelectScreen);

            // Subscribe to service events
            _itemSelectService.OnStateChanged += OnItemSelectStateChanged;
        }

        private void OnItemSelectStateChanged(bool isActive)
        {
            // When item select becomes active, hide selector and disable input
            _selectorService.SetVisible(!isActive);
            _inputFilterService.SetEnabled(!isActive);
            
            Debug.Log($"[LevelSelectionController] Item select state changed: {isActive}");
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (!IsActive || _itemSelectService.IsActive) return; // Block navigation when item select is active

            Vector2 direction = context.ReadValue<Vector2>();

            // Delegate to input filter service for processing
            if (_inputFilterService.ProcessNavigationInput(direction, out Vector2 filteredDirection))
            {
                Debug.Log($"[LevelSelectionController] Processing navigation input: {filteredDirection}");
                _navigationService.NavigateInDirection(filteredDirection);
            }
        }

        private void OnSubmit(InputAction.CallbackContext context)
        {
            if (!IsActive || _itemSelectService.IsActive) return; // Block submit when item select is active

            _navigationService.SelectCurrentLevel();
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<LevelNavigationEvent>(OnLevelNavigation);
            _eventBus?.Subscribe<LevelSelectedEvent>(OnLevelSelected);
            _eventBus?.Subscribe<LevelLoadRequestedEvent>(OnLevelLoadRequested);
        }

        private void OnLevelNavigation(LevelNavigationEvent navigationEvent)
        {
            // Play navigation sound from config
            _audioFeedbackService.PlayNavigationSound();

            // Only move selector if the index actually changed
            MoveSelectorToCurrentLevel();
        }

        private void OnLevelSelected(LevelSelectedEvent selectionEvent)
        {
            Debug.Log($"[LevelSelectionController] Level selected: {selectionEvent.LevelName}");

            // Check if level is unlocked for sound feedback
            LevelData levelData = _navigationService.CurrentLevel;
            if (levelData != null && !levelData.isUnlocked)
            {
                // Play locked sound from config
                _audioFeedbackService.PlayLockedSound();
                Debug.Log($"[LevelSelectionController] Level {selectionEvent.LevelName} is locked");
                return;
            }

            // Play selection sound from config
            _audioFeedbackService.PlaySelectionSound();

            // Get the scene name from the level data directly
            string sceneName = GetSceneNameForLevel(levelData);
            Debug.Log($"[LevelSelectionController] Loading level: {selectionEvent.LevelName} -> Scene: {sceneName}");

            // Show ItemSelectScreen if available, otherwise load directly
            if (itemSelectScreen != null)
            {
                _itemSelectService.ShowItemSelect(selectionEvent.LevelName, sceneName);
            }
            else
            {
                // Load level directly if no ItemSelectScreen
                LoadLevel(sceneName);
            }
        }

        private string GetSceneNameForLevel(LevelData levelData)
        {
            // Delegate to scene load service
            return _sceneLoadService.GetSceneNameForLevel(levelData);
        }

        private void LoadLevel(string sceneName)
        {
            Debug.Log($"[LevelSelectionController] Loading scene: {sceneName}");

            // Use scene load service
            _sceneLoadService.LoadLevel(sceneName);
        }

        private void OnLevelLoadRequested(LevelLoadRequestedEvent loadEvent)
        {
            Debug.Log(
                $"[LevelSelectionController] Level load requested: {loadEvent.LevelName} -> {loadEvent.SceneName}");

            LoadLevel(loadEvent.SceneName);
        }

        private void MoveSelectorToCurrentLevel()
        {
            if (selectorObject == null || _sortedLevelPoints == null) return;

            // Don't move if already moving to avoid redundant calls
            if (_selectorService.IsMoving) return;

            // Use the sorted level points from the discovery service
            if (_navigationService.CurrentIndex >= 0 && _navigationService.CurrentIndex < _sortedLevelPoints.Count)
            {
                Vector3 targetPosition = _sortedLevelPoints[_navigationService.CurrentIndex].transform.position;

                // Only start moving if we're not already at the target position
                if (Vector3.Distance(selectorObject.transform.position, targetPosition) > 0.01f)
                {
                    _selectorService.MoveToPosition(targetPosition);

                    Debug.Log(
                        $"[LevelSelectionController] Moving selector to level {_navigationService.CurrentIndex} at position {targetPosition}");
                }
                else
                {
                    Debug.Log(
                        $"[LevelSelectionController] Selector already at level {_navigationService.CurrentIndex} position");
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[LevelSelectionController] Invalid level index: {_navigationService.CurrentIndex} (max: {_sortedLevelPoints.Count - 1})");
            }
        }

        public void Activate()
        {
            Debug.Log(
                $"[LevelSelectionController] Activating - Current navigation index: {_navigationService?.CurrentIndex}");

            IsActive = true;
            _navigationService?.Activate();
            _displayService?.Activate();

            // Move selector to current position when activating
            MoveSelectorToCurrentLevel();
        }

        public void Deactivate()
        {
            IsActive = false;
            _navigationService?.Deactivate();
            _displayService?.Deactivate();
        }

        /// <summary>
        ///     Public method to refresh all visuals - useful for external calls
        /// </summary>
        public void RefreshVisuals()
        {
            _displayService?.RefreshVisuals();
        }

        /// <summary>
        ///     Public method to set level selection programmatically
        /// </summary>
        public void SetCurrentLevel(int levelIndex)
        {
            Debug.Log($"[LevelSelectionController] SetCurrentLevel called with index: {levelIndex}");
            _navigationService?.SetCurrentIndex(levelIndex);
        }
    }
}

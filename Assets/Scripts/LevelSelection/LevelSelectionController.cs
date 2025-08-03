using System.Threading.Tasks;
using Core.Events;
using LevelSelection.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace LevelSelection
{
    /// <summary>
    ///     Orchestrates level selection functionality using focused services (SOLID principles)
    /// </summary>
    public class LevelSelectionController : MonoBehaviour
    {
        [Header("Auto Configuration")] [SerializeField]
        private bool autoActivateOnStart = true;

        [SerializeField] private LevelSelectionConfig config;

        [Header("UI Components")] [SerializeField]
        private GameObject selectorObject; // Only needed for service initialization

        [SerializeField] private ItemSelectScreen itemSelectScreen; // Only needed for service initialization

        [Header("Input Actions")] [SerializeField]
        private InputActionReference navigateAction;

        [SerializeField] private InputActionReference submitAction;
        private IAudioFeedbackService _audioFeedbackService;

        // Core services - injected via DI
        private ILevelDiscoveryService _discoveryService;
        private ILevelDisplayService _displayService;
        private IEventBus _eventBus;
        private IInputFilterService _inputFilterService;
        private IItemSelectService _itemSelectService;
        private ILevelNavigationService _navigationService;
        private ISceneLoadService _sceneLoadService;

        // New focused services
        private ISelectorService _selectorService;

        public bool IsActive { get; private set; }

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
            // Initialize services first
            InitializeServices();

            var levelData = await _discoveryService.DiscoverLevelsAsync();
            await _navigationService.InitializeAsync(levelData);
            await _displayService.InitializeAsync(levelData);

            // Configure services with sorted level points
            _selectorService.SetLevelPoints(_discoveryService.GetSortedLevelPoints());

            // Pass config to legacy services
            if (config != null)
            {
                _displayService.SetConfig(config);
                _navigationService.SetGridWidth(config.gridWidth);
            }

            Debug.Log($"[LevelSelectionController] Initialized with {levelData.Count} levels using SOLID architecture");
        }

        private void InitializeServices()
        {
            // Setup audio source for AudioFeedbackService
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Initialize all services with their dependencies
            _selectorService.Initialize(selectorObject, config);
            _inputFilterService.Initialize(config);
            _audioFeedbackService.Initialize(audioSource, config);
            _itemSelectService.Initialize(itemSelectScreen, _sceneLoadService); // Pass SceneLoadService for fallback

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

            // Delegate selector movement to service
            _selectorService.MoveToLevel(_navigationService.CurrentIndex);
        }

        private void OnLevelSelected(LevelSelectedEvent selectionEvent)
        {
            Debug.Log($"[LevelSelectionController] Level selected: {selectionEvent.LevelName}");

            // Check if level is unlocked for sound feedback
            LevelData levelData = _navigationService.CurrentLevel;
            if (levelData != null && !levelData.isUnlocked)
            {
                _audioFeedbackService.PlayLockedSound();
                Debug.Log($"[LevelSelectionController] Level {selectionEvent.LevelName} is locked");
                return;
            }

            _audioFeedbackService.PlaySelectionSound();

            string sceneName = _sceneLoadService.GetSceneNameForLevel(levelData);
            Debug.Log($"[LevelSelectionController] Loading level: {selectionEvent.LevelName} -> Scene: {sceneName}");

            // Always delegate to ItemSelectService - it will handle whether to show item select or load directly
            _itemSelectService.ShowItemSelect(selectionEvent.LevelName, sceneName);
        }

        private void OnLevelLoadRequested(LevelLoadRequestedEvent loadEvent)
        {
            Debug.Log(
                $"[LevelSelectionController] Level load requested: {loadEvent.LevelName} -> {loadEvent.SceneName}");

            // Delegate directly to scene load service
            _sceneLoadService.LoadLevel(loadEvent.SceneName);
        }

        public void Activate()
        {
            Debug.Log(
                $"[LevelSelectionController] Activating - Current navigation index: {_navigationService?.CurrentIndex}");

            IsActive = true;
            _navigationService?.Activate();
            _displayService?.Activate();

            // Move selector to current position when activating - delegate to service
            if (_navigationService?.CurrentIndex >= 0)
            {
                _selectorService?.MoveToLevel(_navigationService.CurrentIndex);
            }
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

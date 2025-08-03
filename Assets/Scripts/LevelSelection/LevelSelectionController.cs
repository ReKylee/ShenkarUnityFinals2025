using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;

namespace LevelSelection
{
    /// <summary>
    ///     Centralized controller that manages all level selection functionality
    ///     Uses inspector assignments and input module instead of complex auto-discovery
    /// </summary>
    public class LevelSelectionController : MonoBehaviour
    {
        private const float InputCooldownTime = 0.2f; // Prevent input spam
        private const float InputDeadzone = 0.5f; // Input threshold

        [Header("Auto Configuration")] [SerializeField]
        private bool autoActivateOnStart = true;

        [SerializeField] private LevelSelectionConfig config;

        [Header("UI Components")] [SerializeField]
        private GameObject selectorObject;

        [SerializeField] private ItemSelectScreen itemSelectScreen;
        [SerializeField] private NESCrossfade crossfade;

        [Header("Input Actions")] [SerializeField]
        private InputActionReference navigateAction;

        [SerializeField] private InputActionReference submitAction;

        // Audio support
        private AudioSource _audioSource;

        private ILevelDiscoveryService _discoveryService;
        private ILevelDisplayService _displayService;
        private IEventBus _eventBus;

        // Selector movement
        private bool _isMovingSelector;
        private bool _isItemSelectActive; // Track if item select screen is active

        // Input filtering
        private Vector2 _lastInputDirection;
        private float _lastInputTime;
        private ILevelNavigationService _navigationService;
        private Vector3 _selectorTargetPosition;
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
            UpdateSelectorMovement();
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
            IEventBus eventBus)
        {
            _discoveryService = discoveryService;
            _navigationService = navigationService;
            _displayService = displayService;
            _eventBus = eventBus;

            SubscribeToEvents();
        }

        private async Task InitializeAsync()
        {
            // Setup audio source for config sounds
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            var levelData = await _discoveryService.DiscoverLevelsAsync();
            await _navigationService.InitializeAsync(levelData);
            await _displayService.InitializeAsync(levelData);

            // Get sorted level points from the discovery service - NO DUPLICATION!
            _sortedLevelPoints = _discoveryService.GetSortedLevelPoints();

            // Pass the sorted level points to the display service using DI pattern
            _displayService.SetLevelPoints(_sortedLevelPoints);

            // Pass config to services that need it
            if (config != null)
            {
                _displayService.SetConfig(config);

                // Set grid width for navigation using the interface method
                _navigationService.SetGridWidth(config.gridWidth);

                // Pass config to ItemSelectScreen if available
                if (itemSelectScreen)
                {
                    itemSelectScreen.SetConfig(config);
                }

                // Pass config to crossfade if available
                if (crossfade)
                {
                    crossfade.SetConfig(config);
                }
            }

            Debug.Log(
                $"[LevelSelectionController] Initialized with {levelData.Count} levels using discovery service sorted data");
        }

        private void UpdateSelectorMovement()
        {
            if (!_isMovingSelector || selectorObject == null) return;

            float moveSpeed = config?.selectorMoveSpeed ?? 5f;
            float snapThreshold = config?.snapThreshold ?? 0.1f;

            selectorObject.transform.position = Vector3.MoveTowards(
                selectorObject.transform.position,
                _selectorTargetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(selectorObject.transform.position, _selectorTargetPosition) < snapThreshold)
            {
                selectorObject.transform.position = _selectorTargetPosition;
                _isMovingSelector = false;
            }
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (!IsActive || _isItemSelectActive) return; // Block navigation when item select is active

            Vector2 direction = context.ReadValue<Vector2>();

            // Apply deadzone filtering
            if (direction.magnitude < InputDeadzone) return;

            // Apply input cooldown to prevent spam
            if (Time.time - _lastInputTime < InputCooldownTime) return;

            // Normalize direction for consistent behavior
            direction = direction.normalized;

            // Check if this is the same direction as last input (prevent repeats)
            if (Vector2.Dot(direction, _lastInputDirection) > 0.8f &&
                Time.time - _lastInputTime < InputCooldownTime * 2f) return;

            _lastInputDirection = direction;
            _lastInputTime = Time.time;

            Debug.Log($"[LevelSelectionController] Processing navigation input: {direction}");
            _navigationService.NavigateInDirection(direction);
        }

        private void OnSubmit(InputAction.CallbackContext context)
        {
            if (!IsActive || _isItemSelectActive) return; // Block submit when item select is active

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
            if (config?.navigationSound && _audioSource)
            {
                _audioSource.PlayOneShot(config.navigationSound);
            }

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
                if (config?.lockedSound != null && _audioSource != null)
                {
                    _audioSource.PlayOneShot(config.lockedSound);
                }

                Debug.Log($"[LevelSelectionController] Level {selectionEvent.LevelName} is locked");
                return;
            }

            // Play selection sound from config
            if (config?.selectionSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(config.selectionSound);
            }

            // Get the scene name from the level data directly
            string sceneName = GetSceneNameForLevel(levelData);
            Debug.Log($"[LevelSelectionController] Loading level: {selectionEvent.LevelName} -> Scene: {sceneName}");

            // Show ItemSelectScreen if available, otherwise load directly
            if (itemSelectScreen != null)
            {
                SetItemSelectActive(true); // Activate item select mode
                itemSelectScreen.ShowItemSelect(selectionEvent.LevelName, sceneName, () => {
                    SetItemSelectActive(false); // Deactivate when complete
                });
            }
            else
            {
                // Load level directly if no ItemSelectScreen
                LoadLevel(sceneName);
            }
        }

        private string GetSceneNameForLevel(LevelData levelData)
        {
            // Use the scene name from level data if available
            if (!string.IsNullOrEmpty(levelData?.sceneName))
            {
                return levelData.sceneName;
            }

            // Fallback to level name conversion
            return levelData?.levelName?.Replace(" ", "").Replace("_", "") ?? "DefaultLevel";
        }

        private void LoadLevel(string sceneName)
        {
            Debug.Log($"[LevelSelectionController] Loading scene: {sceneName}");

            // Use standalone SceneTransitionManager first
            SceneTransitionManager.TransitionTo(sceneName);
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
            if (_isMovingSelector) return;

            // Use the sorted level points from the discovery service
            if (_navigationService.CurrentIndex >= 0 && _navigationService.CurrentIndex < _sortedLevelPoints.Count)
            {
                Vector3 targetPosition = _sortedLevelPoints[_navigationService.CurrentIndex].transform.position;

                // Only start moving if we're not already at the target position
                if (Vector3.Distance(selectorObject.transform.position, targetPosition) > 0.01f)
                {
                    _selectorTargetPosition = targetPosition;
                    _isMovingSelector = true;

                    Debug.Log(
                        $"[LevelSelectionController] Moving selector to level {_navigationService.CurrentIndex} at position {_selectorTargetPosition}");
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

        /// <summary>
        /// Set the item select screen active state and control selector visibility
        /// </summary>
        private void SetItemSelectActive(bool isActive)
        {
            _isItemSelectActive = isActive;
            
            if (selectorObject != null)
            {
                selectorObject.SetActive(!isActive); // Hide selector when item select is active
            }
            
            Debug.Log($"[LevelSelectionController] Item select active: {isActive}, Selector visible: {!isActive}");
        }
    }
}

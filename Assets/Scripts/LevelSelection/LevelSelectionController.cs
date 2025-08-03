﻿using System.Threading.Tasks;
using Core.Events;
using LevelSelection.Services;
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
        private ILevelNavigationService _navigationService;
        private Vector3 _selectorTargetPosition;

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

            // Pass config to services that need it
            if (config != null)
            {
                _displayService.SetConfig(config);

                // Set grid width for navigation
                if (_navigationService is LevelNavigationService navService)
                {
                    navService.SetGridWidth(config.gridWidth);
                }

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

            Debug.Log($"[LevelSelectionController] Initialized with {levelData.Count} levels");
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (!IsActive) return;

            Vector2 direction = context.ReadValue<Vector2>();
            _navigationService.NavigateInDirection(direction);
        }

        private void OnSubmit(InputAction.CallbackContext context)
        {
            if (!IsActive) return;

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
            if (config?.navigationSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(config.navigationSound);
            }

            MoveSelectorToCurrentLevel();
        }

        private void OnLevelSelected(LevelSelectedEvent selectionEvent)
        {
            // Check if level is unlocked for sound feedback
            LevelData levelData = _navigationService.CurrentLevel;
            if (levelData != null && !levelData.isUnlocked)
            {
                // Play locked sound from config
                if (config?.lockedSound != null && _audioSource != null)
                {
                    _audioSource.PlayOneShot(config.lockedSound);
                }

                return;
            }

            // Play selection sound from config
            if (config?.selectionSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(config.selectionSound);
            }

            itemSelectScreen?.ShowItemSelect(selectionEvent.LevelName, "");
        }

        private void OnLevelLoadRequested(LevelLoadRequestedEvent loadEvent)
        {
            crossfade?.FadeOutAndIn(
                () => SceneManager.LoadScene(loadEvent.SceneName),
                () => Debug.Log($"Loaded scene: {loadEvent.SceneName}")
            );
        }

        private void MoveSelectorToCurrentLevel()
        {
            if (selectorObject == null) return;

            // Find the current level point
            var levelPoints = FindObjectsByType<LevelPoint>(FindObjectsSortMode.None);
            if (_navigationService.CurrentIndex < levelPoints.Length)
            {
                _selectorTargetPosition = levelPoints[_navigationService.CurrentIndex].transform.position;
                _isMovingSelector = true;
            }
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

        public void Activate()
        {
            IsActive = true;
            _navigationService?.Activate();
            _displayService?.Activate();
        }

        public void Deactivate()
        {
            IsActive = false;
            _navigationService?.Deactivate();
            _displayService?.Deactivate();
        }
    }
}

using System.Threading.Tasks;
using Audio.Data;
using Audio.Interfaces;
using Core;
using Core.Events;
using LevelSelection.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace LevelSelection
{
    /// <summary>
    ///     Controls level selection functionality using the new SOLID audio system directly
    /// </summary>
    public class LevelSelectionController : MonoBehaviour
    {
        [Header("Configuration")] [SerializeField]
        private bool autoActivateOnStart = true;

        [Header("UI References")] [SerializeField]
        private GameObject selectorObject;

        [SerializeField] private ItemSelectScreen itemSelectScreen;

        [Header("Input")] [SerializeField] private InputActionReference navigateAction;

        [SerializeField] private InputActionReference submitAction;

        [Header("Audio")] [SerializeField] private SoundData selectionSound;

        [SerializeField] private SoundData lockedSound;

        private IAudioService _audioService;
        private IEventBus _eventBus;
        private GameDataCoordinator _gameDataCoordinator;
        private GameFlowManager _gameFlowManager;
        private IInputFilterService _inputFilterService;
        private IItemSelectService _itemSelectService;
        private ILevelNavigationService _navigationService;
        private ISceneLoadService _sceneLoadService;
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
        }

        [Inject]
        public void Construct(
            ILevelNavigationService navigationService,
            IEventBus eventBus,
            ISelectorService selectorService,
            IInputFilterService inputFilterService,
            IAudioService audioService,
            IItemSelectService itemSelectService,
            ISceneLoadService sceneLoadService,
            GameFlowManager gameFlowManager,
            GameDataCoordinator gameDataCoordinator)
        {
            _navigationService = navigationService;
            _eventBus = eventBus;
            _selectorService = selectorService;
            _inputFilterService = inputFilterService;
            _audioService = audioService;
            _itemSelectService = itemSelectService;
            _sceneLoadService = sceneLoadService;
            _gameFlowManager = gameFlowManager;
            _gameDataCoordinator = gameDataCoordinator;

            SubscribeToEvents();
        }

        private async Task InitializeAsync()
        {
            InitializeServices();

            if (_gameFlowManager != null)
            {
                _gameFlowManager.PauseGame();
            }

            var levelData = await _gameDataCoordinator.DiscoverLevelsAsync();
            await _navigationService.InitializeAsync(levelData);

            // Configure navigation service with grid width (hardcoded since config removed)
            _navigationService.SetGridWidth(4); // Default grid width
        }

        private void InitializeServices()
        {
            _selectorService.Initialize(selectorObject);
            _inputFilterService.Initialize();
            _itemSelectService.Initialize(itemSelectScreen, _sceneLoadService);

            _itemSelectService.OnStateChanged += OnItemSelectStateChanged;
        }

        private void OnItemSelectStateChanged(bool isActive)
        {
            _selectorService.SetVisible(!isActive);
            _inputFilterService.SetEnabled(!isActive);
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (!IsActive || _itemSelectService.IsActive) return;

            Vector2 direction = context.ReadValue<Vector2>();

            if (_inputFilterService.ProcessNavigationInput(direction, out Vector2 filteredDirection))
            {
                _navigationService.NavigateInDirection(filteredDirection);
            }
        }

        private void OnSubmit(InputAction.CallbackContext context)
        {
            if (!IsActive || _itemSelectService.IsActive) return;

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
            _selectorService.MoveToCurrentLevel(_navigationService);
        }

        private void OnLevelSelected(LevelSelectedEvent selectionEvent)
        {
            LevelData levelData = _navigationService.CurrentLevel;
            bool isUnlocked = _gameDataCoordinator?.IsLevelUnlocked(levelData?.levelName) ?? false;

            if (levelData != null && !isUnlocked)
            {
                PlayLockedSound();
                return;
            }

            PlaySelectionSound();

            string sceneName = _sceneLoadService.GetSceneNameForLevel(levelData);
            _itemSelectService.ShowItemSelect(selectionEvent.LevelName, sceneName);
        }

        private void OnLevelLoadRequested(LevelLoadRequestedEvent loadEvent)
        {
            _gameDataCoordinator?.UpdateCurrentLevel(loadEvent.LevelName);

            if (_gameFlowManager != null)
            {
                // Use StartLevel with the correct level name when loading from level selection
                _gameFlowManager.StartLevel(loadEvent.LevelName);
            }

            _sceneLoadService.LoadLevel(loadEvent.SceneName);
        }

        public void Activate()
        {
            IsActive = true;
            _navigationService?.Activate();

            if (_navigationService?.CurrentIndex >= 0)
            {
                _selectorService?.MoveToCurrentLevel(_navigationService);
            }
        }

        public void Deactivate()
        {
            IsActive = false;
            _navigationService?.Deactivate();
        }

        public void SetCurrentLevel(int levelIndex)
        {
            _navigationService?.SetCurrentIndex(levelIndex);
        }

        private void PlaySelectionSound()
        {
            if (selectionSound?.clip && _audioService != null)
            {
                _audioService.PlaySound(
                    selectionSound.clip,
                    selectionSound.GetRandomizedVolume(),
                    selectionSound.GetRandomizedPitch()
                );
            }
        }

        private void PlayLockedSound()
        {
            if (lockedSound?.clip && _audioService != null)
            {
                _audioService.PlaySound(
                    lockedSound.clip,
                    lockedSound.GetRandomizedVolume(),
                    lockedSound.GetRandomizedPitch()
                );
            }
        }
    }
}

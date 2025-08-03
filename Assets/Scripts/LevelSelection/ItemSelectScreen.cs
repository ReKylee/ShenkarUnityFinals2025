using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace LevelSelection
{
    public class ItemSelectScreen : MonoBehaviour
    {
        [Header("UI References")] public Image itemSelectImage;

        [SerializeField] private Sprite itemSelectSprite;

        [Header("Display Settings")] public float displayDuration = 2f;

        public bool waitForInput = true;

        [Header("Audio")] public AudioClip confirmSound;

        [Header("Input Actions")] [SerializeField]
        private InputActionReference submitAction;

        private AudioSource _audioSource;
        private GameFlowManager _gameFlowManager;
        private Action _onComplete;
        private string _pendingLevelName;
        private string _pendingSceneName;

        private bool IsWaitingForInput { get; set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Setup the item select sprite if available
            if (itemSelectSprite && itemSelectImage)
            {
                itemSelectImage.sprite = itemSelectSprite;
            }

            // Hide the image component at start
            if (itemSelectImage != null)
            {
                itemSelectImage.enabled = false;
            }

            // Keep GameObject active but start with hidden image
            gameObject.SetActive(true);

            Debug.Log("[ItemSelectScreen] Initialized with hidden image");
        }

        private void OnEnable()
        {
            if (submitAction)
            {
                submitAction.action.Enable();
                submitAction.action.performed += OnConfirmInput;
            }
        }

        private void OnDisable()
        {
            if (submitAction)
            {
                submitAction.action.performed -= OnConfirmInput;
                submitAction.action.Disable();
            }
        }


        [Inject]
        public void Construct(GameFlowManager gameFlowManager)
        {
            _gameFlowManager = gameFlowManager;
        }

        public void ShowItemSelect(string levelName, string sceneName, Action onComplete = null)
        {
            _pendingLevelName = levelName;
            _pendingSceneName = sceneName;
            _onComplete = onComplete;

            // Show the item select image
            if (itemSelectImage != null)
            {
                itemSelectImage.enabled = true;
            }

            Debug.Log($"[ItemSelectScreen] Showing item select for level: {levelName}");

            if (waitForInput)
            {
                IsWaitingForInput = true;
            }
            else
            {
                StartCoroutine(AutoProgressAfterDelay());
            }
        }

        private IEnumerator AutoProgressAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            ConfirmAndProceed();
        }

        private void OnConfirmInput(InputAction.CallbackContext context)
        {
            if (IsWaitingForInput)
            {
                ConfirmAndProceed();
            }
        }

        private void ConfirmAndProceed()
        {
            if (_audioSource && confirmSound)
            {
                _audioSource.PlayOneShot(confirmSound);
            }

            CompleteSelection();
        }

        private void CompleteSelection()
        {
            Debug.Log($"[ItemSelectScreen] Completing selection for level: {_pendingLevelName}");

            IsWaitingForInput = false;

            // Hide the image component
            if (itemSelectImage != null)
            {
                itemSelectImage.enabled = false;
            }

            // Request level load through GameFlowManager instead of publishing directly
            _gameFlowManager?.RequestLevelLoad(_pendingLevelName, _pendingSceneName);

            _onComplete?.Invoke();
        }
    }
}

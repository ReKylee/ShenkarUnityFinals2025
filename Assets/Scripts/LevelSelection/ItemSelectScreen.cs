using System;
using System.Collections;
using Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace LevelSelection
{
    public class ItemSelectScreen : MonoBehaviour
    {
        [Header("UI References")] public Image itemSelectImage;

        public string itemSelectSpritePath = "item select screen";

        [Header("Display Settings")] public float displayDuration = 2f;

        public bool waitForInput = true;

        [Header("Audio")] public AudioClip confirmSound;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference submitAction;

        private IEventBus _eventBus;
        private AudioSource _audioSource;
        private bool _isWaitingForInput = false;
        private string _pendingLevelName;
        private string _pendingSceneName;
        private System.Action _onComplete;

        public bool IsWaitingForInput { get; private set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Load the item select screen sprite
            if (itemSelectImage != null && string.IsNullOrEmpty(itemSelectSpritePath) == false)
            {
                Sprite itemSelectSprite = Resources.Load<Sprite>(itemSelectSpritePath);
                if (itemSelectSprite != null)
                {
                    itemSelectImage.sprite = itemSelectSprite;
                }
            }

            // Start hidden
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (submitAction != null)
            {
                submitAction.action.Enable();
                submitAction.action.performed += OnConfirmInput;
            }
        }

        private void OnDisable()
        {
            if (submitAction != null)
            {
                submitAction.action.performed -= OnConfirmInput;
                submitAction.action.Disable();
            }
        }

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void ShowItemSelect(string levelName, string sceneName, Action onComplete = null)
        {
            _pendingLevelName = levelName;
            _pendingSceneName = sceneName;
            _onComplete = onComplete;

            gameObject.SetActive(true);

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

            IsWaitingForInput = false;
            gameObject.SetActive(false);

            // Publish level load request event
            _eventBus?.Publish(new LevelLoadRequestedEvent
            {
                Timestamp = Time.time,
                LevelName = _pendingLevelName,
                SceneName = _pendingSceneName
            });

            _onComplete?.Invoke();
        }
    }
}

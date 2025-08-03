using System.Collections;
using Core;
using ModularCharacterController.Core;
using UnityEngine;
using VContainer;

namespace LevelSelection
{
    /// <summary>
    ///     Trigger zone that detects when the player completes a level.
    ///     Its only responsibility is to notify the GameFlowManager.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EndLevelZone : MonoBehaviour
    {
        [Header("Level Completion Settings")] [SerializeField]
        private string currentLevelName;

        [SerializeField] private string nextLevelName;
        [SerializeField] private bool autoReturnToLevelSelect = true;
        [SerializeField] private float completionDelay = 2f;

        [Header("Audio Feedback")] [SerializeField]
        private AudioClip completionSound;

        [Header("UI Feedback")] [SerializeField]
        private GameObject completionUI;

        [SerializeField] private float uiDisplayDuration = 3f;

        private AudioSource _audioSource;
        private GameFlowManager _gameFlowManager;
        private bool _hasTriggered;

        private void Awake()
        {
            // Setup audio component
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Ensure trigger is set up correctly
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;

            // Hide completion UI initially
            if (completionUI != null)
            {
                completionUI.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if player entered
            if (other.CompareTag("Player") && !_hasTriggered)
            {
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                InputHandler input = other.GetComponent<InputHandler>();
                StartCoroutine(CompleteLevel(rb, input));
            }
        }

        [Inject]
        public void Construct(GameFlowManager gameFlowManager)
        {
            _gameFlowManager = gameFlowManager;
        }

        private IEnumerator CompleteLevel(Rigidbody2D rb, InputHandler input)
        {
            _hasTriggered = true;

            // Take control from the player
            if (input)
            {
                input.enabled = false; // Disable player input
            }

            // Make the player walk right
            const float walkDuration = 1f;
            float timer = 0f;
            while (timer < walkDuration)
            {
                if (rb)
                {
                    rb.linearVelocityX = 2f;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[EndLevelZone] Player completed level: {currentLevelName}");

            // Play completion sound
            if (completionSound && _audioSource)
            {
                _audioSource.PlayOneShot(completionSound);
            }

            // Show completion UI
            if (completionUI != null)
            {
                completionUI.SetActive(true);
                yield return new WaitForSeconds(uiDisplayDuration);
                completionUI.SetActive(false);
            }

            // Wait for completion delay
            yield return new WaitForSeconds(completionDelay);

            // Notify GameFlowManager, which will handle all subsequent logic.
            _gameFlowManager?.CompleteLevel(currentLevelName, nextLevelName, autoReturnToLevelSelect);
        }
    }
}

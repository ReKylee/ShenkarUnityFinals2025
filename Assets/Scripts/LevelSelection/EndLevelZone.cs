using System.Collections;
using Core;
using ModularCharacterController.Core;
using Player.Services;
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
        [SerializeField] private float completionDelay = 2f;

        [Header("Audio Feedback")] [SerializeField]
        private AudioClip completionSound;

        private AudioSource _audioSource;
        private GameFlowManager _gameFlowManager;
        private HealthBonusService _healthBonusService;
        private bool _hasTriggered;

        private void Awake()
        {
            // Setup audio component
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
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
        public void Construct(GameFlowManager gameFlowManager, HealthBonusService healthBonusService)
        {
            _gameFlowManager = gameFlowManager;
            _healthBonusService = healthBonusService;
        }

        private IEnumerator CompleteLevel(Rigidbody2D rb, InputHandler input)
        {
            _hasTriggered = true;

            // Take control from the player
            if (input)
            {
                input.enabled = false; 
            }
            
            if (rb)
            {
                rb.linearVelocity = Vector2.zero; 
            }

            // Make the player walk right
            const float walkDuration = 1.3f;
            float timer = 0f;
            while (timer < walkDuration)
            {
                if (rb)
                {
                    rb.linearVelocityX = 2f;
                }

                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            Debug.Log($"[EndLevelZone] Player completed level: {currentLevelName}");

            if (completionSound && _audioSource)
                _audioSource.PlayOneShot(completionSound);

      

            bool bonusComplete = false;
            if (_healthBonusService)
            {
                Debug.Log("[EndLevelZone] Starting health bonus calculation...");
                _healthBonusService.CalculateHealthBonus(() => bonusComplete = true);
                
                // Wait for bonus calculation to complete
                yield return new WaitUntil(() => bonusComplete);
                Debug.Log("[EndLevelZone] Health bonus calculation finished");
            }

            yield return new WaitForSeconds(completionDelay);

            _gameFlowManager?.CompleteLevel(currentLevelName);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InputSystem;

namespace Utilities
{
    /// <summary>
    ///     Advanced one-way platform that works with BoxCollider2D.
    ///     Provides better control than Platform Effector 2D with drop-through functionality.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [Header("Platform Settings")] [SerializeField]
        private float dropThroughTime = 0.5f;

        [SerializeField] private float platformThickness = 0.1f;
        [SerializeField] private LayerMask playerLayers = -1;
        [SerializeField] private bool allowDropThrough = true;

        [Header("Input Settings")] [SerializeField]
        private bool requireBothInputs = true;

        [Header("Debug")] [SerializeField] private bool showDebugGizmos = false;

        [SerializeField] private Color platformColor = Color.green;
        [SerializeField] private Color disabledColor = Color.red;

        // Events for integration (simple C# events, no external dependencies)
        public Action<string, Vector3> OnPlayerLanded;
        public Action<string, Vector3> OnPlayerDroppedThrough;

        private BoxCollider2D _platformCollider;
        private InputSystem_Actions _inputActions;
        private readonly Dictionary<Collider2D, PlayerState> _playersOnPlatform = new();
        private readonly List<Collider2D> _temporarilyIgnored = new();

        // Player state tracking
        private class PlayerState
        {
            public bool IsOnPlatform;
            public float LastGroundTime;
            public Vector2 LastVelocity;
            public bool WasAbovePlatform;
        }

        #region Unity Lifecycle

        private void Awake()
        {
            _platformCollider = GetComponent<BoxCollider2D>();

            // Ensure the platform is set up correctly
            if (_platformCollider.isTrigger)
            {
                Debug.LogWarning(
                    $"[OneWayPlatform] {gameObject.name}: BoxCollider2D should NOT be a trigger. Setting isTrigger to false.");

                _platformCollider.isTrigger = false;
            }

            _inputActions = new InputSystem_Actions();
            _inputActions.Enable();
        }

        private void Update()
        {
            UpdatePlayerStates();
            HandleDropThroughInput();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollisionEnter(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleCollisionStay(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            HandleCollisionExit(collision);
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        #endregion

        #region Collision Handling

        private void HandleCollisionEnter(Collision2D collision)
        {
            if (!IsPlayer(collision.collider)) return;

            Collider2D playerCollider = collision.collider;
            Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();

            if (playerRb == null) return;

            // Initialize player state if not exists
            if (!_playersOnPlatform.ContainsKey(playerCollider))
            {
                _playersOnPlatform[playerCollider] = new PlayerState();
            }

            PlayerState state = _playersOnPlatform[playerCollider];

            // Check if player is coming from above
            bool isComingFromAbove = IsPlayerComingFromAbove(collision, playerRb);

            if (!isComingFromAbove)
            {
                // Player is hitting from side or below - ignore collision
                StartCoroutine(TemporarilyIgnoreCollision(playerCollider));
                return;
            }

            // Player is landing on platform from above
            state.IsOnPlatform = true;
            state.WasAbovePlatform = true;
            state.LastGroundTime = Time.time;
            state.LastVelocity = playerRb.linearVelocity;

            // Publish platform landing event
            OnPlayerLanded?.Invoke(gameObject.name, playerCollider.transform.position);

            if (showDebugGizmos)
            {
                Debug.Log($"[OneWayPlatform] Player {playerCollider.name} landed on platform {gameObject.name}");
            }
        }

        private void HandleCollisionStay(Collision2D collision)
        {
            if (!IsPlayer(collision.collider)) return;

            Collider2D playerCollider = collision.collider;

            if (_playersOnPlatform.ContainsKey(playerCollider))
            {
                PlayerState state = _playersOnPlatform[playerCollider];
                state.LastGroundTime = Time.time;

                // Update velocity for better drop-through detection
                Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    state.LastVelocity = playerRb.linearVelocity;
                }
            }
        }

        private void HandleCollisionExit(Collision2D collision)
        {
            if (!IsPlayer(collision.collider)) return;

            Collider2D playerCollider = collision.collider;

            if (_playersOnPlatform.ContainsKey(playerCollider))
            {
                PlayerState state = _playersOnPlatform[playerCollider];
                state.IsOnPlatform = false;

                // Clean up after a short delay
                StartCoroutine(CleanupPlayerState(playerCollider, 0.1f));
            }
        }

        #endregion

        #region Player Detection and State Management

        private bool IsPlayer(Collider2D otherCollider) =>
            otherCollider.CompareTag("Player") ||
            (1 << otherCollider.gameObject.layer & playerLayers) != 0;

        private bool IsPlayerComingFromAbove(Collision2D collision, Rigidbody2D playerRb)
        {
            // Check if player's bottom is above platform's top
            Vector2 playerBottom = collision.collider.bounds.min;
            Vector2 platformTop = _platformCollider.bounds.max;

            // Player must be above the platform
            if (playerBottom.y < platformTop.y - platformThickness) return false;

            // Check velocity - player should be moving downward or have minimal upward velocity
            return playerRb.linearVelocity.y <= 0.5f;
        }

        private void UpdatePlayerStates()
        {
            var playersToRemove = new List<Collider2D>();

            foreach (var kvp in _playersOnPlatform)
            {
                if (kvp.Key == null)
                {
                    playersToRemove.Add(kvp.Key);
                    continue;
                }

                PlayerState state = kvp.Value;

                // Clean up old states
                if (Time.time - state.LastGroundTime > 1f && !state.IsOnPlatform)
                {
                    playersToRemove.Add(kvp.Key);
                }
            }

            foreach (Collider2D player in playersToRemove)
            {
                _playersOnPlatform.Remove(player);
            }
        }

        #endregion

        #region Drop Through Functionality

        private void HandleDropThroughInput()
        {
            if (!allowDropThrough) return;

            // Use Walk action to detect downward movement
            bool crouchPressed = _inputActions.Player.Crouch.ReadValue<bool>();
            bool jumpPressed = _inputActions.Player.Jump.triggered;

            bool shouldDropThrough =
                requireBothInputs ? (crouchPressed && jumpPressed) : (crouchPressed || jumpPressed);

            if (shouldDropThrough)
            {
                TriggerDropThrough();
            }
        }

        private void TriggerDropThrough()
        {
            var playersToDropThrough = new List<Collider2D>();

            foreach (var kvp in _playersOnPlatform)
            {
                if (kvp.Value.IsOnPlatform)
                {
                    playersToDropThrough.Add(kvp.Key);
                }
            }

            foreach (Collider2D playerCollider in playersToDropThrough)
            {
                StartCoroutine(DropThroughSequence(playerCollider));
            }
        }

        private IEnumerator DropThroughSequence(Collider2D playerCollider)
        {
            if (!playerCollider) yield break;

            // Publish drop through event
            OnPlayerDroppedThrough?.Invoke(gameObject.name, playerCollider.transform.position);

            // Temporarily ignore collision
            yield return StartCoroutine(TemporarilyIgnoreCollision(playerCollider));

            if (showDebugGizmos)
            {
                Debug.Log($"[OneWayPlatform] Player {playerCollider.name} dropped through platform {gameObject.name}");
            }
        }

        private IEnumerator TemporarilyIgnoreCollision(Collider2D playerCollider)
        {
            if (!playerCollider) yield break;

            // Add to ignored list
            if (!_temporarilyIgnored.Contains(playerCollider))
            {
                _temporarilyIgnored.Add(playerCollider);
            }

            // Ignore collision
            Physics2D.IgnoreCollision(_platformCollider, playerCollider, true);

            // Update player state
            if (_playersOnPlatform.TryGetValue(playerCollider, out PlayerState value))
            {
                value.IsOnPlatform = false;
            }

            // Wait for specified time
            yield return new WaitForSeconds(dropThroughTime);

            // Re-enable collision only if player is above platform
            if (playerCollider && IsPlayerAbovePlatform(playerCollider))
            {
                Physics2D.IgnoreCollision(_platformCollider, playerCollider, false);
            }
            else
            {
                // Wait a bit more and try again
                yield return new WaitForSeconds(0.1f);
                if (playerCollider)
                {
                    Physics2D.IgnoreCollision(_platformCollider, playerCollider, false);
                }
            }

            // Remove from ignored list
            _temporarilyIgnored.Remove(playerCollider);
        }

        private bool IsPlayerAbovePlatform(Collider2D playerCollider) =>
            playerCollider.bounds.min.y > _platformCollider.bounds.max.y;

        private IEnumerator CleanupPlayerState(Collider2D playerCollider, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!playerCollider || !_playersOnPlatform.TryGetValue(playerCollider, out PlayerState state))
                yield break;

            if (!state.IsOnPlatform)
            {
                _playersOnPlatform.Remove(playerCollider);
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        ///     Force a specific player to drop through the platform
        /// </summary>
        public void ForceDropThrough(Collider2D playerCollider)
        {
            if (IsPlayer(playerCollider))
            {
                StartCoroutine(DropThroughSequence(playerCollider));
            }
        }

        /// <summary>
        ///     Check if a specific player is currently on this platform
        /// </summary>
        public bool IsPlayerOnPlatform(Collider2D playerCollider) =>
            _playersOnPlatform.ContainsKey(playerCollider) &&
            _playersOnPlatform[playerCollider].IsOnPlatform;

        /// <summary>
        ///     Get all players currently on this platform
        /// </summary>
        public List<Collider2D> GetPlayersOnPlatform()
        {
            var result = new List<Collider2D>();
            foreach (var kvp in _playersOnPlatform)
            {
                if (kvp.Value.IsOnPlatform)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        #endregion

    }
}

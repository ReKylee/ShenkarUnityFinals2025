using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Modules
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    ///     Handles horizontal movement with acceleration/deceleration for a responsive feel
    ///     Supports pixel-perfect movement for 2D pixel art games
    /// </summary>
    public class HMoveAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        private const float WALL_DETECTION_DISTANCE = 0.1f;
        private const float MIN_INPUT_THRESHOLD = 0.01f;
        private const float SPEED_MULTIPLIER = 1.1f;
        private const float WALL_CAST_HEIGHT_MULTIPLIER = 0.7f;
        private const float WALL_CAST_DISTANCE = 0.01f;

        private static int? _groundLayerMask;

        [Header("Pixel Perfect Settings")] [Tooltip("Enable pixel-perfect movement to eliminate sub-pixel jitter")]
        public bool enablePixelPerfectMovement;

        [Tooltip("Pixels per unit - must match your sprite's PPU and Pixel Perfect Camera settings")] [Range(1f, 1000f)]
        public float pixelsPerUnit = 100f;

        [Tooltip("Minimum movement threshold for pixel snapping")]
        public float pixelSnapThreshold = 0.001f;

        private Bounds _colliderBounds;

        // Track direction facing for sprite flipping
        private int _facingDirection = 1;
        private bool _isRunning;
        private static int GroundLayerMask => _groundLayerMask ??= LayerMask.GetMask("Ground");


        public Vector2 ProcessMovement(
            Vector2 currentVelocity, bool isGrounded,
            InputContext inputContext)
        {
            // Early return if essential components are missing
            MccStats stats = Controller?.Stats;
            if (!stats) return currentVelocity;

            // Cache input values and calculate absolute values once
            float walkInput = inputContext.WalkInput;
            bool runInput = inputContext.AttackHeld;
            float absWalkInput = Mathf.Abs(walkInput);

            // Determine input types and movement input in one pass
            bool hasMovementInput = absWalkInput > MIN_INPUT_THRESHOLD;

            // Update facing direction and running state
            if (hasMovementInput)
            {
                int newFacingDirection = walkInput > 0 ? 1 : -1;

                _facingDirection = newFacingDirection;
            }


            // Cache speed and acceleration values based on current state
            float targetSpeed = runInput ? stats.runSpeed : stats.walkSpeed;
            float acceleration = isGrounded ? stats.groundAcceleration : stats.airAcceleration;
            float deceleration = isGrounded ? stats.groundDeceleration : stats.airDeceleration;

            // Calculate new horizontal velocity
            if (hasMovementInput )
            {

                // Accelerate towards target velocity
                float targetVelocity = targetSpeed * _facingDirection;
                currentVelocity.x = Mathf.MoveTowards(
                    currentVelocity.x,
                    targetVelocity,
                    acceleration * Time.deltaTime
                );
            }
            else
            {
                // Decelerate to stop
                currentVelocity.x = Mathf.MoveTowards(
                    currentVelocity.x,
                    0f,
                    deceleration * Time.deltaTime
                );
            }

            // Enforce speed limits with cached max speed
            float maxHorizontalSpeed = targetSpeed * SPEED_MULTIPLIER;
            currentVelocity.x = Mathf.Clamp(currentVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);

            // Apply pixel-perfect snapping if enabled
            if (enablePixelPerfectMovement)
            {
                currentVelocity.x = SnapToPixelGrid(currentVelocity.x);
            }

            return currentVelocity;
        }

        /// <summary>
        ///     Snaps a velocity value to the pixel grid to ensure pixel-perfect movement
        /// </summary>
        /// <param name="velocity">The velocity to snap</param>
        /// <returns>Pixel-aligned velocity</returns>
        private float SnapToPixelGrid(float velocity)
        {
            if (pixelsPerUnit <= 0) return velocity;

            // Calculate pixel size in world units
            float pixelSize = 1.0f / pixelsPerUnit;

            // Snap velocity to ensure movement increments align with pixel boundaries
            float snappedVelocity = Mathf.Round(velocity / pixelSize) * pixelSize;

            // Apply threshold to prevent micro-movements
            if (Mathf.Abs(snappedVelocity) < pixelSnapThreshold)
            {
                snappedVelocity = 0f;
            }

            return snappedVelocity;
        }

        private bool IsWallBlocking(int direction)
        {
            Collider2D collider = Controller?.Collider;
            if (!collider) return false;

            // Update bounds cache
            _colliderBounds = collider.bounds;

            // Calculate cast parameters
            float xOffset = _colliderBounds.extents.x * direction;
            Vector2 castOrigin = new(_colliderBounds.center.x + xOffset, _colliderBounds.center.y);
            Vector2 castSize = new(WALL_DETECTION_DISTANCE, _colliderBounds.size.y * WALL_CAST_HEIGHT_MULTIPLIER);
            Vector2 castDirection = direction > 0 ? Vector2.right : Vector2.left;

            // Use non-allocating boxcast - simple wall detection
            return Physics2D.BoxCast(
                castOrigin,
                castSize,
                0f,
                castDirection,
                WALL_CAST_DISTANCE,
                GroundLayerMask
            );
        }
    }
}

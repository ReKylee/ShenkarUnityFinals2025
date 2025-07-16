using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Modules
{
    /// <summary>
    ///     Basic walk ability - the default movement ability for Kirby
    ///     Handles horizontal movement with acceleration/deceleration for a responsive feel
    /// </summary>
    public class HMoveAbilityModule : AbilityModuleBase, IMovementAbilityModule
    {
        private const float WALL_DETECTION_DISTANCE = 0.1f;
        private const float MIN_INPUT_THRESHOLD = 0.01f;
        private const float SPEED_MULTIPLIER = 1.1f;
        private const float WALL_CAST_HEIGHT_MULTIPLIER = 0.7f;
        private const float WALL_CAST_DISTANCE = 0.01f;

        // Cached ground layer mask to avoid repeated GetMask calls
        private static int? _groundLayerMask;
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
            float runInput = inputContext.RunInput;
            float walkInput = inputContext.WalkInput;
            float absRunInput = Mathf.Abs(runInput);
            float absWalkInput = Mathf.Abs(walkInput);

            // Determine input types and movement input in one pass
            bool isRunInput = absRunInput > MIN_INPUT_THRESHOLD;
            float movementInput = isRunInput ? runInput : walkInput;
            float absMovementInput = isRunInput ? absRunInput : absWalkInput;
            bool hasMovementInput = absMovementInput > MIN_INPUT_THRESHOLD;

            // Update facing direction and running state
            if (hasMovementInput)
            {
                int newFacingDirection = movementInput > 0 ? 1 : -1;

                _facingDirection = newFacingDirection;
            }

            // Update running state
            _isRunning = hasMovementInput && (isRunInput || _isRunning);

            // Cache speed and acceleration values based on current state
            float targetSpeed = _isRunning ? stats.runSpeed : stats.walkSpeed;
            float acceleration = isGrounded ? stats.groundAcceleration : stats.airAcceleration;
            float deceleration = isGrounded ? stats.groundDeceleration : stats.airDeceleration;

            // Calculate new horizontal velocity
            if (hasMovementInput && !IsWallBlocking(_facingDirection))
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


            return currentVelocity;
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

            // Perform wall detection
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

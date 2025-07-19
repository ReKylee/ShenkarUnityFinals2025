using UnityEngine;

namespace Extensions
{
    /// <summary>
    /// A custom camera controller for pixel-perfect 2D games.
    /// This script makes the camera follow a target transform with a configurable offset,
    /// allows locking movement on individual axes, and snaps the final position to the
    /// pixel grid to prevent jitter.
    /// </summary>
    [ExecuteInEditMode]
    public class PixelPerfectFollowCamera : MonoBehaviour
    {
        [Tooltip("The target transform for the camera to follow.")]
        public Transform target;

        [Tooltip("The offset from the target's position.")]
        public Vector3 offset = new Vector3(0, 0, -10);

        [Header("Axis Following")] [Tooltip("If true, the camera will follow the target on the X axis.")]
        public bool followX = true;

        [Tooltip("If true, the camera will follow the target on the Y axis.")]
        public bool followY = true;

        [Tooltip("If true, the camera will follow the target on the Z axis.")]
        public bool followZ = false; // Usually false for 2D games

        [Header("Dampening")] [Tooltip("If true, camera movement will be smoothed with dampening.")]
        public bool useDampening = true;

        [Tooltip("Dampening strength (higher values = smoother but slower movement).")] [Range(0.1f, 10f)]
        public float dampeningStrength = 2f;

        [Tooltip("Maximum camera movement speed in pixels per second.")] [Range(10f, 1000f)]
        public float maxPixelSpeed = 300f;

        [Header("World Bounds")]
        [Tooltip("The collider that defines the world boundaries. Camera will not move outside these bounds.")]
        public Collider2D worldBoundsCollider;

        [Tooltip("If true, world bounds will be applied to camera movement.")]
        public bool useWorldBounds = true;

        [Header("Pixel Perfect Settings")]
        [Tooltip("The number of pixels per unit. Must match your sprite and Pixel Perfect Camera settings.")]
        [Range(1, 256)]
        public float pixelsPerUnit = 100;

        private Camera _cameraComponent;
        private Vector2 _velocity = Vector2.zero;

        private void Awake()
        {
            _cameraComponent = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (!target || pixelsPerUnit <= 0)
            {
                return;
            }

            // Start with the current camera position
            Vector3 newPosition = transform.position;
            Vector3 targetPosition = target.position;

            // Determine the desired position based on axis locks
            float desiredX = followX ? targetPosition.x + offset.x : newPosition.x;
            float desiredY = followY ? targetPosition.y + offset.y : newPosition.y;
            float desiredZ = followZ ? targetPosition.z + offset.z : newPosition.z;

            // Apply world bounds constraints if enabled
            if (useWorldBounds && worldBoundsCollider != null && _cameraComponent != null)
            {
                Vector2 constrainedPosition = ApplyWorldBounds(new Vector2(desiredX, desiredY));
                desiredX = constrainedPosition.x;
                desiredY = constrainedPosition.y;
            }

            // Apply dampening if enabled (only in play mode)
            if (useDampening && Application.isPlaying)
            {
                // Convert pixel speed to world units per second based on PPU
                float maxSpeed = maxPixelSpeed / pixelsPerUnit;

                // Calculate smooth time based on PPU and dampening strength
                // Higher PPU needs more smoothing to maintain pixel-perfect movement
                float smoothTime = dampeningStrength * (pixelsPerUnit / 100f) * 0.02f;

                // Apply dampening using SmoothDamp
                Vector2 currentPos = new Vector2(newPosition.x, newPosition.y);
                Vector2 targetPos = new Vector2(desiredX, desiredY);
                Vector2 smoothedPos = Vector2.SmoothDamp(currentPos, targetPos, ref _velocity, smoothTime, maxSpeed,
                    Time.deltaTime);

                desiredX = smoothedPos.x;
                desiredY = smoothedPos.y;
            }

            // Snap the desired position to the pixel grid
            float pixelSize = 1.0f / pixelsPerUnit;
            float snappedX = Mathf.Round(desiredX / pixelSize) * pixelSize;
            float snappedY = Mathf.Round(desiredY / pixelSize) * pixelSize;

            // Z-axis snapping is often not needed unless you have 2.5D elements
            float snappedZ = desiredZ;

            transform.position = new Vector3(snappedX, snappedY, snappedZ);
        }

        private Vector2 ApplyWorldBounds(Vector2 desiredPosition)
        {
            // Get camera bounds in world space
            float cameraHeight = _cameraComponent.orthographicSize * 2;
            float cameraWidth = cameraHeight * _cameraComponent.aspect;

            float halfWidth = cameraWidth * 0.5f;
            float halfHeight = cameraHeight * 0.5f;

            // Get world bounds from the collider
            Bounds worldBounds = worldBoundsCollider.bounds;

            // Calculate the constrained position
            float minX = worldBounds.min.x + halfWidth;
            float maxX = worldBounds.max.x - halfWidth;
            float minY = worldBounds.min.y + halfHeight;
            float maxY = worldBounds.max.y - halfHeight;

            // Clamp the desired position within the bounds
            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }
    }
}

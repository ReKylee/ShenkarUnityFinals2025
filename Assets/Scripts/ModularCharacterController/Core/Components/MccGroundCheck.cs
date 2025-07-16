using UnityEditor;
using UnityEngine;

namespace ModularCharacterController.Core.Components
{
    /// <summary>
    ///     Handles ground detection for the Kirby controller with slope information
    /// </summary>
    public class MccGroundCheck : MonoBehaviour
    {
        public enum SlopeType
        {
            None,
            Flat,
            Slope,
            DeepSlope
        }

        [Header("Ground Detection Settings")] [SerializeField]
        public LayerMask groundLayers;

        [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;
        [SerializeField] private Vector2 groundCheckSize = new(0.5f, 0.2f);
        [SerializeField] private bool drawGizmos = true;

        [Header("Slope Settings")] [SerializeField] [Range(1, 35)]
        private float slopeThreshold = 10f;

        [SerializeField] [Range(35, 89)] private float deepSlopeThreshold = 35f;


        /// <summary>
        ///     Returns true if the character is grounded
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        ///     Returns the current ground slope angle in degrees (0 = flat, positive = uphill, negative = downhill)
        /// </summary>
        public float GroundSlopeAngle { get; private set; }

        /// <summary>
        ///     Returns the normal vector of the ground surface
        /// </summary>
        public Vector2 GroundNormal { get; private set; } = Vector2.up;

        /// <summary>
        ///     Returns the surface types the character is standing on
        /// </summary>
        public SlopeType CurrentSlope { get; private set; } = SlopeType.None;


        private void FixedUpdate()
        {
            CheckGround();
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // Calculate the position of the ground check box using the offset
            Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;

            // Draw ground check box
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(boxPosition, groundCheckSize);


            if (IsGrounded)
            {
                // Draw ground normal from the bottom center of the box
                Vector3 normalOrigin = boxPosition - new Vector2(0, groundCheckSize.y * 0.5f);
                Gizmos.color = Color.blue;
                Vector3 normalEnd = normalOrigin + (Vector3)GroundNormal * 0.5f;
                Gizmos.DrawLine(normalOrigin, normalEnd);

                // Draw slope direction (tangent to the surface)
                Vector3 slopeDir = new Vector3(GroundNormal.y, -GroundNormal.x, 0).normalized;
                // Make sure slope direction points right when going uphill
                if (GroundSlopeAngle < 0) slopeDir = -slopeDir;

                // Color based on surface types
                Gizmos.color = CurrentSlope switch
                {
                    SlopeType.Flat => Color.green,
                    SlopeType.Slope => Color.yellow,
                    SlopeType.DeepSlope => new Color(1f, 0.5f, 0f), // Orange
                    _ => Color.white
                };

                Gizmos.DrawRay(normalOrigin, slopeDir * 0.5f);

#if UNITY_EDITOR
                // Draw text label with slope angle and types in the scene view
                Handles.Label(boxPosition + Vector2.up * 0.3f,
                    $"{CurrentSlope}: {GroundSlopeAngle:F1}°");
#endif
            }

            // Draw raycasts used for fallback detection
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Vector2 rayOrigin = new(
                    boxPosition.x,
                    boxPosition.y + groundCheckSize.y * 0.5f + 0.05f
                );

                Gizmos.DrawRay(rayOrigin, Vector2.down * (groundCheckSize.y + 0.1f));
            }
        }

        private void CheckGround()
        {
            // Reset values
            IsGrounded = false;
            GroundNormal = Vector2.up;
            GroundSlopeAngle = 0f;
            CurrentSlope = SlopeType.None;

            Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;

            RaycastHit2D hit = Physics2D.BoxCast(
                boxPosition,
                groundCheckSize,
                0f,
                Vector2.down,
                0f,
                groundLayers
            );

            if (!hit.collider)
            {
                return;
            }

            // We found ground
            IsGrounded = true;
            GroundNormal = hit.normal;

            // Calculate slope angle using the dot product method
            float slopeAngleRad = Mathf.Acos(Mathf.Clamp(Vector2.Dot(GroundNormal, Vector2.up), -1f, 1f));
            GroundSlopeAngle = slopeAngleRad * Mathf.Rad2Deg;

            // Apply correct sign based on normal direction
            if (GroundNormal.x != 0)
            {
                GroundSlopeAngle *= Mathf.Sign(-GroundNormal.x);
            }

            // Classify surface types based on absolute slope angle
            float absSlopeAngle = Mathf.Abs(GroundSlopeAngle);
            if (absSlopeAngle <= slopeThreshold)
            {
                CurrentSlope = SlopeType.Flat;
            }
            else if (absSlopeAngle < deepSlopeThreshold)
            {
                CurrentSlope = SlopeType.Slope;
            }
            else
            {
                CurrentSlope = SlopeType.DeepSlope;
            }
        }
    }
}

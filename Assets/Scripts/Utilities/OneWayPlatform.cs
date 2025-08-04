using UnityEngine;

namespace Utilities
{
    public class OneWayPlatform2D : MonoBehaviour
    {
        [Header("Platform Settings")] [SerializeField]
        private LayerMask platformLayers = -1;

        [SerializeField] private float detectionHeight = 0.1f;
        private ContactFilter2D _contactFilter;

        private BoxCollider2D _platformCollider;

        private void Start()
        {
            SetupPlatform();
        }

        private void FixedUpdate()
        {
            CheckCollisions();
        }

        private void OnDrawGizmosSelected()
        {
            if (_platformCollider == null)
                _platformCollider = GetComponent<BoxCollider2D>();

            if (_platformCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.matrix = transform.localToWorldMatrix;

                Vector3 size = new(_platformCollider.size.x, _platformCollider.size.y, 0.1f);
                Vector3 center = new(_platformCollider.offset.x, _platformCollider.offset.y, 0f);

                Gizmos.DrawWireCube(center, size);

                // Draw the platform top detection line
                Gizmos.color = Color.cyan;
                Vector3 topLineStart = center + Vector3.left * _platformCollider.size.x / 2f +
                                       Vector3.up * (_platformCollider.size.y / 2f + detectionHeight);

                Vector3 topLineEnd = center + Vector3.right * _platformCollider.size.x / 2f +
                                     Vector3.up * (_platformCollider.size.y / 2f + detectionHeight);

                Gizmos.DrawLine(topLineStart, topLineEnd);

                // Draw the solid platform surface
                Gizmos.color = Color.red;
                topLineStart = center + Vector3.left * _platformCollider.size.x / 2f +
                               Vector3.up * _platformCollider.size.y / 2f;

                topLineEnd = center + Vector3.right * _platformCollider.size.x / 2f +
                             Vector3.up * _platformCollider.size.y / 2f;

                Gizmos.DrawLine(topLineStart, topLineEnd);
            }
        }

        private void SetupPlatform()
        {
            // Get the collider
            _platformCollider = GetComponent<BoxCollider2D>();
            if (_platformCollider == null)
            {
                Debug.LogError("OneWayPlatform2D requires a BoxCollider2D component!");
                return;
            }

            // Make sure it's a solid collider
            _platformCollider.isTrigger = false;

            // Set up contact filter for the layers we care about
            _contactFilter = new ContactFilter2D();
            _contactFilter.useLayerMask = true;
            _contactFilter.layerMask = platformLayers;
            _contactFilter.useTriggers = false;
        }

        private void CheckCollisions()
        {
            if (_platformCollider == null) return;

            // Get all contacts with this collider
            var contacts = new ContactPoint2D[10];
            int contactCount = _platformCollider.GetContacts(_contactFilter, contacts);

            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint2D contact = contacts[i];
                Collider2D otherCollider = contact.otherCollider;

                if (otherCollider == null) continue;

                Rigidbody2D rb = otherCollider.GetComponent<Rigidbody2D>();
                if (rb == null) continue;

                // Calculate if the object is coming from above
                Vector2 contactPoint = contact.point;
                Vector2 platformTop = new(transform.position.x,
                    transform.position.y + _platformCollider.size.y * transform.localScale.y / 2f);

                // Check if contact is from above and object is moving downward or stationary
                bool comingFromAbove = contactPoint.y >= platformTop.y - detectionHeight;
                bool movingDown = rb.linearVelocity.y <= 0.1f; // Small tolerance for floating point errors

                // If not coming from above or moving upward, ignore collision
                if (!comingFromAbove || rb.linearVelocity.y > 0.1f)
                {
                    Physics2D.IgnoreCollision(_platformCollider, otherCollider, true);
                }
                else
                {
                    Physics2D.IgnoreCollision(_platformCollider, otherCollider, false);
                }
            }

            // Also check for nearby objects that might need collision re-enabled
            CheckNearbyObjects();
        }

        private void CheckNearbyObjects()
        {
            // Create a slightly larger area to check for objects that should have collision re-enabled
            Vector2 checkSize = _platformCollider.size * transform.localScale + Vector2.one * 0.2f;
            Vector2 checkCenter = (Vector2)transform.position + _platformCollider.offset;

            var nearbyColliders = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f, platformLayers);

            foreach (Collider2D otherCollider in nearbyColliders)
            {
                if (otherCollider == _platformCollider) continue;

                Rigidbody2D rb = otherCollider.GetComponent<Rigidbody2D>();
                if (rb == null) continue;

                // If object is far enough away from the platform, re-enable collision
                float objectBottom = otherCollider.bounds.min.y;
                float platformTop = transform.position.y + _platformCollider.size.y * transform.localScale.y / 2f;

                if (objectBottom > platformTop + 0.1f)
                {
                    Physics2D.IgnoreCollision(_platformCollider, otherCollider, false);
                }
            }
        }
    }
}

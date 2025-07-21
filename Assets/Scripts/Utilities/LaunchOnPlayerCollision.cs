using System.Collections;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Launches the container in an arc when kicked sideways or upwards when stomped.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class LaunchOnPlayerCollision : MonoBehaviour
    {
        [Tooltip("Horizontal launch speed when kicked from side.")] [SerializeField]
        private float horizontalSpeed = 2f;

        [Tooltip("Vertical launch speed when kicked or stomped.")] [SerializeField]
        private float verticalSpeed = 2f;

        [SerializeField, Tooltip("Time window to ignore collisions with the player after launch.")]
        private float ignoreDuration = 0.2f;

        private Rigidbody2D _rb;
        private Collider2D _col;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            // Compute weighted horizontal ratio based on hit point
            ContactPoint2D contact = collision.GetContact(0);
            float halfWidth = _col.bounds.extents.x;
            float offsetX = _col.bounds.center.x - contact.point.x;
            float ratio = Mathf.Clamp(offsetX / halfWidth, -1f, 1f);
            // Apply arc launch
            _rb.linearVelocity = new Vector2(horizontalSpeed * ratio, verticalSpeed);
            // Temporarily ignore collisions with the player so the container can pass through
            Collider2D playerCol = collision.collider;
            Physics2D.IgnoreCollision(_col, playerCol, true);
            StartCoroutine(RestoreCollision(playerCol));
        }
        private IEnumerator RestoreCollision(Collider2D playerCol)
        {
            yield return new WaitForSecondsRealtime(ignoreDuration);
            if (playerCol)
                Physics2D.IgnoreCollision(_col, playerCol, false);
        }
    }
}

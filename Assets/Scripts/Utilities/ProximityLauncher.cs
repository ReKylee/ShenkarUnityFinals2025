using Player.Components;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    ///     Hides the container and launches it upwards when the player exceeds a vertical threshold.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
    public class ProximityLauncher : MonoBehaviour
    {
        [Tooltip("Vertical distance above container at which it triggers launch.")]
        public float triggerHeight;

        [Tooltip("Launch velocity applied when triggered.")]
        public Vector2 launchVel = new(0f, 5f);

        private Collider2D _col;
        private Transform _player;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private bool _triggered;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<Collider2D>();
        }

        private void Start()
        {
            _player = PlayerLocator.PlayerTransform;
            // Hide until triggered
            _sr.enabled = false;
            _col.enabled = false;
            // Disable gravity until launch
            _rb.gravityScale = 0f;
        }

        private void Update()
        {
            if (_triggered || !_player)
                return;

            if (_player.position.y - transform.position.y > triggerHeight)
            {
                _triggered = true;
                // Show and launch
                _sr.enabled = true;
                _col.enabled = true;
                _rb.gravityScale = 1f;
                _rb.linearVelocity = new Vector2(launchVel.x, launchVel.y);
            }
        }
    }
}

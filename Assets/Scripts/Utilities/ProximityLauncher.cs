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
        public float triggerHeight = 2f;

        [Tooltip("Horizontal distance within which the launcher can be triggered.")]
        public float triggerProximity = 1f;

        [Tooltip("Launch velocity applied when triggered.")]
        public Vector2 launchVel = new(0f, 12f);

        private Collider2D _col;
        private float _originalGravityScale;
        private Transform _player;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private bool _triggered;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<Collider2D>();
            _originalGravityScale = _rb.gravityScale;
        }

        private void Start()
        {
            _player = PlayerLocator.PlayerTransform;
            _sr.enabled = false;
            _col.enabled = false;
            _rb.gravityScale = 0f;
        }

        private void Update()
        {
            if (_triggered || !_player)
                return;

            // Check if player is above and close horizontally
            float verticalDist = _player.position.y - transform.position.y;
            float horizontalDist = Mathf.Abs(_player.position.x - transform.position.x);
            if (verticalDist > triggerHeight && horizontalDist < triggerProximity)
            {
                _triggered = true;
                _sr.enabled = true;
                _col.enabled = true;
                _rb.gravityScale = _originalGravityScale;
                _rb.linearVelocity = launchVel;
            }
        }
    }
}

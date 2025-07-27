using UnityEngine;
using Enemies.Interfaces;

namespace Enemies.Behaviors
{
    // Makes the enemy jump forward at intervals (configurable for snakes, frogs, etc.)
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpMover : MonoBehaviour, IMovementBehavior
    {
        [SerializeField] private float jumpForceX = 2f;
        [SerializeField] private float jumpForceY = 5f;
        [SerializeField] private float jumpInterval = 2f;
        private Rigidbody2D _rb;
        private float _nextJumpTime;
        private bool _grounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _nextJumpTime = Time.time + jumpInterval;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Consider any collision as grounded (customize layer/tag if needed)
            _grounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _grounded = false;
        }

        public void Move()
        {
            if (Time.time >= _nextJumpTime && _grounded)
            {
                Vector2 jumpDir = new Vector2(transform.localScale.x * jumpForceX, jumpForceY);
                _rb.linearVelocity = jumpDir;
                _nextJumpTime = Time.time + jumpInterval;
            }
        }
    }
}

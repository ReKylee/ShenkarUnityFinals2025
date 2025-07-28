using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Makes the enemy jump forward at intervals (configurable for snakes, frogs, etc.)
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpMover : MonoBehaviour, IMovementBehavior
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float jumpForceX = 2f;
        [SerializeField] private float jumpForceY = 5f;
        [SerializeField] private float jumpInterval = 2f;
        private bool _grounded;
        private float _nextJumpTime;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _nextJumpTime = Time.time + jumpInterval;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((1 << collision.gameObject.layer & groundLayer) != 0)
                _grounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((1 << collision.gameObject.layer & groundLayer) != 0)
                _grounded = false;
        }

        public void Move()
        {
            if (Time.time >= _nextJumpTime && _grounded)
            {
                Vector2 jumpDir = new(transform.localScale.x * jumpForceX, jumpForceY);
                _rb.linearVelocity = jumpDir;
                _nextJumpTime = Time.time + jumpInterval;
            }
        }
    }
}

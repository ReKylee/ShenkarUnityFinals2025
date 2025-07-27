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
        [SerializeField] private LayerMask groundLayer;
        private Rigidbody2D _rb;
        private float _nextJumpTime;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _nextJumpTime = Time.time + jumpInterval;
        }

        public void Move()
        {
            if (Time.time >= _nextJumpTime && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(jumpForceX, jumpForceY);
                _nextJumpTime = Time.time + jumpInterval;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
                _isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
                _isGrounded = false;
        }
    }
}

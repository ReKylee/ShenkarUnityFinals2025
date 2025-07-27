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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _nextJumpTime = Time.time + jumpInterval;
        }

        public void Move()
        {
            if (Time.time >= _nextJumpTime && IsGrounded())
            {
                _rb.linearVelocity = new Vector2(jumpForceX, jumpForceY);
                _nextJumpTime = Time.time + jumpInterval;
            }
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        }
    }
}

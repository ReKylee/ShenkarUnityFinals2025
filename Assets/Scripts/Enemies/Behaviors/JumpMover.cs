using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Command to make the enemy jump forward
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpCommand : MonoBehaviour, IMovementCommand
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float jumpForceX = 2f;
        [SerializeField] private float jumpForceY = 5f;

        private bool _grounded;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
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

        public void Execute()
        {
            if (_grounded)
            {
                Vector2 jumpDir = new(transform.localScale.x * jumpForceX, jumpForceY);
                _rb.linearVelocity = jumpDir;
            }
        }

        public void ResetState()
        {
            _grounded = false;
        }
    }
}

using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Command to make the enemy jump forward
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpCommand : MonoBehaviour, IMovementCommand
    {
        [SerializeField] private float jumpForceX = 2f;
        [SerializeField] private float jumpForceY = 5f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Execute()
        {
            Vector2 jumpDir = new(transform.localScale.x * jumpForceX, jumpForceY);
            _rb.linearVelocity = jumpDir;
        }
    }
}

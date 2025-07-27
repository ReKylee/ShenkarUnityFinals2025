using UnityEngine;
using Enemies.Interfaces;

namespace Enemies.Behaviors
{
    // Moves the enemy up and down at a constant speed
    public class VerticalMover : MonoBehaviour, IMovementBehavior
    {
        [SerializeField] private float amplitude = 2f;
        [SerializeField] private float frequency = 0.8f; 

        private Rigidbody2D _rb;
        private float _startY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _startY = transform.position.y;
        }

        public void Move()
        {
            float t = Mathf.PingPong(Time.time * frequency, 1f);
            float triangle = 2f * Mathf.Abs(t - 0.5f);
            float y = _startY + (triangle - 0.5f) * 2f * amplitude;
            _rb.linearVelocityY = (y - _rb.position.y) / Time.fixedDeltaTime;
        }
    }
}

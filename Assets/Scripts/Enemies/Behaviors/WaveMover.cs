using UnityEngine;
using Enemies.Interfaces;

namespace Enemies.Behaviors
{
    // Moves the enemy left while oscillating up and down (wave/flap motion)
    public class WaveMover : MonoBehaviour, IMovementBehavior
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float amplitude = 1f;
        [SerializeField] private float frequency = 1.11f;
        private float _startY;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _startY = transform.position.y;
        }

        public void Move()
        {
            float y = _startY + Mathf.Sin(Time.time * frequency) * amplitude;
            _rb.linearVelocity = new Vector2(-speed, (y - _rb.position.y) / Time.fixedDeltaTime);
        }
    }
}

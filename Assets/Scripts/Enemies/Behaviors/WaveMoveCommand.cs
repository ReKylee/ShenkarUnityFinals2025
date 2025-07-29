using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Command to move the enemy in a wave pattern
    public class WaveMoveCommand : MonoBehaviour, IMovementCommand
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float amplitude = 1f;
        [SerializeField] private float frequency = 1.11f;

        private Rigidbody2D _rb;
        private float _startY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _startY = transform.position.y;
        }

        public void Execute()
        {
            float t = Mathf.PingPong(Time.time * frequency, 1f);
            float triangle = 2f * Mathf.Abs(t - 0.5f);
            float y = _startY + (triangle - 0.5f) * 2f * amplitude;
            _rb.linearVelocity = new Vector2(-speed, (y - _rb.position.y) / Time.fixedDeltaTime);
        }

  
    }
}

using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Command to move the enemy up and down
    public class VerticalMoveCommand : MonoBehaviour, IMovementCommand
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

        public void Execute()
        {
            float t = Mathf.PingPong(Time.time * frequency, 1f);
            float triangle = 2f * Mathf.Abs(t - 0.5f);
            float y = _startY + (triangle - 0.5f) * 2f * amplitude;
            _rb.linearVelocityY = (y - _rb.position.y) / Time.fixedDeltaTime;
        }

        public void ResetPosition()
        {
            _rb.position = new Vector2(_rb.position.x, _startY);
        }
    }
}

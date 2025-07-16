using UnityEngine;

namespace Enemy
{
    public class EnemyMovement : MonoBehaviour
    {

        private const float GroundCheckDistance = 0.1f;
        private const float GroundCheckRadius = 0.05f;
        [SerializeField] private float speed = 2f;
        [SerializeField] private LayerMask groundLayer;
        private Collider2D _col;
        private int _direction = 1;
        private Vector3 _originalScale;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _originalScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocityX = _direction * speed;

            Vector2 origin = new(
                _col.bounds.center.x + _direction * (_col.bounds.extents.x + 0.05f),
                _col.bounds.min.y
            );

            bool isGroundAhead = Physics2D.CircleCast(
                origin,
                GroundCheckRadius,
                Vector2.down,
                GroundCheckDistance,
                groundLayer
            );

            if (!isGroundAhead)
            {
                Flip();
            }
        }

        private void Flip()
        {
            _direction = -_direction;
            transform.localScale = new Vector3(_originalScale.x * _direction, _originalScale.y, _originalScale.z);
        }
    }
}

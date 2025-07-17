using System;
using System.Collections;
using Projectiles.Core;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileBoomerang : BaseProjectile
    {
        [SerializeField] private float returnDelay = 1.5f; // Time before boomerang starts returning
        [SerializeField] private float returnSpeed = 15f; // Speed when returning to player
        [SerializeField] private float rotationSpeed = 720f; // Degrees per second rotation
        
        [NonSerialized] public float Direction;
        [NonSerialized] public Transform PlayerTransform;
        
        public event Action OnBoomerangReturned;
        
        private bool _isReturning;
        private Coroutine _returnCoroutine;

        private void Update()
        {
            // Rotate the boomerang continuously
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // Handle returning logic
            if (_isReturning && PlayerTransform != null)
            {
                Vector3 directionToPlayer = (PlayerTransform.position - transform.position).normalized;
                Rb.linearVelocity = directionToPlayer * returnSpeed;
                
                // Check if close enough to player to "catch" the boomerang
                if (Vector3.Distance(transform.position, PlayerTransform.position) < 1f)
                {
                    OnBoomerangReturned?.Invoke();
                    ReturnToPool();
                }
            }
        }

        protected override void Move()
        {
            _isReturning = false;
            transform.localScale = new Vector3(Direction, 1, 1);
            Rb.AddForce(new Vector2(speed.x * Direction, speed.y), ForceMode2D.Impulse);
            
            // Start the return timer
            _returnCoroutine = StartCoroutine(StartReturning());
        }

        private IEnumerator StartReturning()
        {
            yield return new WaitForSeconds(returnDelay);
            _isReturning = true;
            Debug.Log("Boomerang starting to return to player");
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // Don't return to pool on collision with player while returning
            if (other.gameObject.CompareTag("Player"))
            {
                if (_isReturning)
                {
                    OnBoomerangReturned?.Invoke();
                    ReturnToPool();
                }
                return;
            }
            
            // For other collisions, start returning immediately
            if (!_isReturning)
            {
                if (_returnCoroutine != null)
                {
                    StopCoroutine(_returnCoroutine);
                }
                _isReturning = true;
                Debug.Log($"Boomerang hit {other.gameObject.name}, starting return");
            }
        }

        private void OnBecameInvisible()
        {
            // Only return to pool if we've been invisible for a while and not returning
            if (!_isReturning)
            {
                ReturnToPool();
            }
        }

        private void OnEnable()
        {
            _isReturning = false;
        }

        private void OnDisable()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }
    }
}

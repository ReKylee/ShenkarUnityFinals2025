using Health.Interfaces;
using UnityEngine;

namespace PowerUps
{
    public class PowerUpContainer : MonoBehaviour, IDamageable
    {
        [SerializeField] private Sprite crackedSprite;
        [SerializeField] private GameObject powerUpPrefab;
        [SerializeField] private Vector2 launchVel = new(2, 2);
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        public int CurrentHp { get; private set; } = 2;
        public int MaxHp { get; } = 2;
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player") && CurrentHp == MaxHp)
            {
                _rigidbody.linearVelocity = launchVel;
                Damage(1);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && CurrentHp < MaxHp)
            {
                BreakOpen();
            }
        }
        private void BreakOpen()
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        public void Damage(int amount)
        {
            CurrentHp -= 1;
            switch (CurrentHp)
            {
                case 1:
                    Crack();
                    break;
                case <= 0:
                    BreakOpen();
                    break;
            }
        }

        private void Crack()
        {
            _spriteRenderer.sprite = crackedSprite;

        }

    }
}

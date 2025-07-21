using Health.Interfaces;
using UnityEngine;

namespace PowerUps.Container
{
    public class PowerUpContainer : MonoBehaviour, IDamageable
    {
        [SerializeField] private Sprite crackedSprite;
        [SerializeField] private GameObject powerUpPrefab;
        private SpriteRenderer _spriteRenderer;
        public int CurrentHp { get; private set; } = 2;
        public int MaxHp { get; } = 2;
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // Player collision: damage on first hit
            if (other.gameObject.CompareTag("Player") && CurrentHp == MaxHp)
            {
                Damage(1);
            }
            // Ground collision: break open after hit
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

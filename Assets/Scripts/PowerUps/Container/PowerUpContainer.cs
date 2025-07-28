using Health.Interfaces;
using UnityEngine;

namespace PowerUps.Container
{
    public class PowerUpContainer : MonoBehaviour, IDamageable
    {
        [SerializeField] private LayerMask groundLayer;
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
            else if (((1 << other.gameObject.layer) & groundLayer) != 0 && CurrentHp < MaxHp)
            {
                BreakOpen();
            }
        }
        public void Damage(int amount, GameObject source = null)
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
        private void BreakOpen()
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private void Crack()
        {
            _spriteRenderer.sprite = crackedSprite;

        }
    }
}

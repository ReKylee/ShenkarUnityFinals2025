using System;
using Health.Interfaces;
using PowerUps._Base;
using UnityEngine;

namespace PowerUps
{
    public class PowerUpContainer : MonoBehaviour, IDamageable
    {
        [SerializeField] private PowerUpCollectibleBase powerUpCollectible;
        [SerializeField] private Vector2 launchVelocity;
        [SerializeField] private Sprite whole;
        [SerializeField] private Sprite broken;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _sr.sprite = whole;
        }
        private void AttemptBreak()
        {
            _rb.linearVelocity = launchVelocity;
            _sr.sprite = broken;

        }
        public int CurrentHp { get; private set; } = 2;
        public int MaxHp { get; } = 2;
        public void Damage(int amount)
        {
            CurrentHp -= 1;

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                BreakContainer();
            }
            else
            {
                AttemptBreak();
            }
        }
        private void BreakContainer()
        {
            if (!powerUpCollectible)
                return;

            PowerUpCollectibleBase pu = Instantiate(powerUpCollectible, transform.position, Quaternion.identity);
            Destroy(gameObject);

        }
    }
}

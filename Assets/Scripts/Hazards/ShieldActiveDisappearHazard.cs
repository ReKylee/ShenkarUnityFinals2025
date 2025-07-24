﻿using Health.Interfaces;
using Player.Components;
using UnityEngine;

namespace Hazards
{
    public class ShieldActiveDisappearHazard : MonoBehaviour, IDamageDealer
    {
        [SerializeField] private int damageAmount = 1;
        private bool _damaged;
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_damaged) return;
            if (other.gameObject.CompareTag("Player"))
            {
                _damaged = true;
                if (other.gameObject.TryGetComponent(out IDamageShield shield) && shield.IsActive)
                {
                    if (TryGetComponent(out IDamageable health))
                    {
                        health.Damage(health.MaxHp); 
                    }
                    else
                    {
                        gameObject.SetActive(false); 
                    }
                }
            }
        }
        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _damaged = false;
            }
        }
        public int GetDamageAmount() => damageAmount;
    }
}

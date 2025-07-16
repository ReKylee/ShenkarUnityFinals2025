using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    public class ContinuousDamage : MonoBehaviour
    {
        [SerializeField] private PlayerHealthController healthController;
        [SerializeField] private float damageInterval = 3f;
        private Coroutine _damageCoroutine;

        private void Start()
        {
            StartContinuousDamage();
        }

        public void StartContinuousDamage()
        {
            _damageCoroutine ??= StartCoroutine(DamageLoop());
        }

        public void StopContinuousDamage()
        {
            if (_damageCoroutine == null)
                return;

            StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }

        private IEnumerator DamageLoop()
        {
            while (healthController.CurrentHp > 0)
            {
                healthController.Damage(1);
                yield return new WaitForSeconds(damageInterval);
            }
            _damageCoroutine = null;
        }
    }
}

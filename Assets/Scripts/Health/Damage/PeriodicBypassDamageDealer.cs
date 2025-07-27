using System.Collections;
using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{
    /// <summary>
    ///     Applies damage every interval seconds to the attached GameObject, bypassing shield logic.
    /// </summary>
    [DisallowMultipleComponent]
    public class PeriodicBypassDamageDealer : MonoBehaviour
    {
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float interval = 3f;
        private IBypassableDamageable _bypassable;
        private Coroutine _damageRoutine;

        private void Awake()
        {
            _bypassable = GetComponent<IBypassableDamageable>();
        }

        private void OnEnable()
        {
            if (_bypassable != null)
                _damageRoutine = StartCoroutine(DamageLoop());
        }

        private void OnDisable()
        {
            if (_damageRoutine != null)
                StopCoroutine(_damageRoutine);
        }

        private IEnumerator DamageLoop()
        {
            while (true)
            {
                _bypassable.DamageBypass(damageAmount);
                yield return new WaitForSeconds(interval);
            }
        }
    }
}

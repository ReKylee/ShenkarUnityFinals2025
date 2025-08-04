using System.Collections;
using Core.Events;
using Health.Interfaces;
using UnityEngine;
using VContainer;

namespace Health.Damage
{
    public class PeriodicBypassDamageDealer : MonoBehaviour
    {
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float damageInterval = 1f;

        private Coroutine _damageCoroutine;
        private IEventBus _eventBus;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            StopDamageCoroutine();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IBypassableDamageable>(out _))
            {
                StartDamageCoroutine(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<IBypassableDamageable>(out _))
            {
                StopDamageCoroutine();
            }
        }

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            Debug.Log("[PeriodicBypassDamageDealer] Level completed, stopping damage.");
            StopDamageCoroutine();
            // Optionally, disable the component entirely
            enabled = false;
        }

        private void StartDamageCoroutine(GameObject target)
        {
            if (_damageCoroutine == null)
            {
                _damageCoroutine = StartCoroutine(DealDamagePeriodically(target));
            }
        }

        private void StopDamageCoroutine()
        {
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }
        }

        private IEnumerator DealDamagePeriodically(GameObject target)
        {
            while (target != null && target.activeInHierarchy)
            {
                if (target.TryGetComponent<IBypassableDamageable>(out var damageable))
                {
                    damageable.DamageBypass(damageAmount);
                }
                yield return new WaitForSeconds(damageInterval);
            }
            _damageCoroutine = null;
        }
    }
}

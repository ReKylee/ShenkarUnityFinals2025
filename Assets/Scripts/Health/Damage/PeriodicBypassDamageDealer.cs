using System.Collections;
using Core.Events;
using Health.Interfaces;
using UnityEngine;
using VContainer;

namespace Health.Damage
{
    /// <summary>
    /// Deals periodic bypass damage to the player automatically
    /// </summary>
    public class PeriodicBypassDamageDealer : MonoBehaviour
    {
        [Header("Damage Settings")] [SerializeField]
        private int damageAmount = 1;

        [SerializeField] private float damageInterval = 1f;

        private IEventBus _eventBus;
        private IBypassableDamageable _damagable;
        private Coroutine _damageCoroutine;
        private bool _isLevelCompleted;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Start()
        {
            _damagable = GetComponent<IBypassableDamageable>();

            if (_damagable != null)
            {
                StartDamageCoroutine();
                Debug.Log("[PeriodicBypassDamageDealer] Auto-started periodic damage");
            }
            else
            {
                Debug.LogWarning(
                    "[PeriodicBypassDamageDealer] No IBypassableDamageable found on player - damage will not start");
            }
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

        private void OnLevelCompleted(LevelCompletedEvent evt)
        {
            Debug.Log("[PeriodicBypassDamageDealer] Level completed, stopping damage.");
            _isLevelCompleted = true;
            StopDamageCoroutine();
            enabled = false;
        }

        private void StartDamageCoroutine()
        {
            if (_damageCoroutine == null && !_isLevelCompleted)
            {
                _damageCoroutine = StartCoroutine(DealDamagePeriodically());
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

        private IEnumerator DealDamagePeriodically()
        {
            while (_damagable != null && !_isLevelCompleted)
            {
                _damagable.DamageBypass(damageAmount);
                yield return new WaitForSeconds(damageInterval);
            }

            _damageCoroutine = null;
        }
    }
}

using Core.Events;
using Health.Interfaces;
using UnityEngine;
using VContainer;

namespace Health.Damage
{
    /// <summary>
    ///     Applies damage every interval seconds to the attached GameObject, bypassing shield logic.
    /// </summary>
    [DisallowMultipleComponent]
    public class PeriodicBypassDamageDealer : MonoBehaviour
    {
        [Header("Damage Settings")] [SerializeField]
        private int damageAmount = 1;

        [SerializeField] private float interval = 3f;
        private IBypassableDamageable _bypassable;
        private IEventBus _eventBus;

        private void Awake()
        {
            _bypassable = GetComponent<IBypassableDamageable>();
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);

            if (_bypassable != null)
            {
                InvokeRepeating(nameof(DealDamage), 0f, interval);
            }
        }

        private void OnDisable()
        {
            StopDamage();
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void OnLevelCompleted(LevelCompletedEvent obj)
        {
            StopDamage();
        }

        private void StopDamage()
        {
            CancelInvoke(nameof(DealDamage));
        }

        private void DealDamage() => _bypassable?.DamageBypass(damageAmount);
    }
}

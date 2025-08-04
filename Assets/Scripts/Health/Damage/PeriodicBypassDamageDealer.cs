using System.Collections;
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
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float interval = 3f;
        private IBypassableDamageable _bypassable;
        private Coroutine _damageRoutine;
        private IEventBus _eventBus;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        private void Awake()
        {
            _bypassable = GetComponent<IBypassableDamageable>();
        }
        private void OnEnable()
        {
            _eventBus?.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            
            if (_bypassable != null)
                _damageRoutine = StartCoroutine(DamageLoop());
        }
        private void OnLevelCompleted(LevelCompletedEvent obj)
        {
            if (_damageRoutine != null)
            {
                StopCoroutine(_damageRoutine);
                _damageRoutine = null;
            }
        }

        private void OnDisable()
        {
            if (_damageRoutine != null)
                StopCoroutine(_damageRoutine);
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
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

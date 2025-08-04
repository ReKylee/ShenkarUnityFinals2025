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
        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float interval = 3f;
        private IBypassableDamageable _bypassable;
        private Coroutine _damageRoutine;
        private IEventBus _eventBus;
        private bool _isLevelCompleted;

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
            _isLevelCompleted = false;
            
            if (_bypassable != null)
                _damageRoutine = StartCoroutine(DamageLoop());
        }
        
        private void OnLevelCompleted(LevelCompletedEvent obj)
        {
            _isLevelCompleted = true;
            StopDamageRoutine();
        }

        private void OnDisable()
        {
            _isLevelCompleted = true;
            StopDamageRoutine();
            _eventBus?.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
        }

        private void StopDamageRoutine()
        {
            if (_damageRoutine != null)
            {
                StopCoroutine(_damageRoutine);
                _damageRoutine = null;
            }
        }

        private IEnumerator DamageLoop()
        {
            while (_bypassable != null && !_isLevelCompleted)
            {
                _bypassable.DamageBypass(damageAmount);
                yield return new WaitForSeconds(interval);
            }
            
            _damageRoutine = null;
        }
    }
}

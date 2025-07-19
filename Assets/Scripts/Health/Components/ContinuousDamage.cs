using System.Collections;
using Health.Interfaces;
using UnityEngine;

namespace Health.Components
{
    /// <summary>
    /// Applies continuous damage to an IDamageable component at regular intervals
    /// </summary>
    public class ContinuousDamage : MonoBehaviour
    {
        [SerializeField] private float damageInterval = 3f;
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private bool startDamageOnStart = true;
        
        private IDamageable _healthController;
        private IBypassableDamageable _bypassableDamageable;
        private Coroutine _damageCoroutine;
        private WaitForSeconds _waitForDamageInterval;

        #region Unity Lifecycle

        private void Awake()
        {
            _healthController = GetComponent<IDamageable>();
            
            if (_healthController == null)
            {
                Debug.LogError($"[ContinuousDamage] No IDamageable component found on {gameObject.name}");
                enabled = false;
                return;
            }
            
            // Cache the bypassable interface if available
            _bypassableDamageable = _healthController as IBypassableDamageable;
            
            // Cache the wait time to avoid allocation in coroutine
            _waitForDamageInterval = new WaitForSeconds(damageInterval);
        }

        private void Start()
        {
            if (startDamageOnStart && _healthController != null)
            {
                StartContinuousDamage();
            }
        }

        private void OnDestroy()
        {
            StopContinuousDamage();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Start applying continuous damage
        /// </summary>
        public void StartContinuousDamage()
        {
            if (_healthController == null || _damageCoroutine != null) return;
            
            _damageCoroutine = StartCoroutine(DamageLoop());
        }

        /// <summary>
        /// Stop applying continuous damage
        /// </summary>
        public void StopContinuousDamage()
        {
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }
        }

        /// <summary>
        /// Change the damage interval at runtime
        /// </summary>
        public void SetDamageInterval(float newInterval)
        {
            if (newInterval <= 0)
            {
                Debug.LogWarning("[ContinuousDamage] Damage interval must be greater than 0");
                return;
            }
            
            damageInterval = newInterval;
            _waitForDamageInterval = new WaitForSeconds(damageInterval);
        }

        #endregion

        #region Private Methods

        private IEnumerator DamageLoop()
        {
            // Initial wait before first damage
            yield return _waitForDamageInterval;
            
            while (_healthController is { CurrentHp: > 0 })
            {
                // Apply damage using cached bypass capability
                if (_bypassableDamageable != null)
                {
                    _bypassableDamageable.DamageBypass(damageAmount);
                }
                else
                {
                    _healthController.Damage(damageAmount);
                }
                
                // Early exit if health reaches 0
                if (_healthController.CurrentHp <= 0) break;
                
                yield return _waitForDamageInterval;
            }
            
            // Clean up coroutine reference
            _damageCoroutine = null;
        }

        #endregion
    }
}

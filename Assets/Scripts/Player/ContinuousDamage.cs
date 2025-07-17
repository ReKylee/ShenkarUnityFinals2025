using System.Collections;
using UnityEngine;

namespace Player
{
    public class ContinuousDamage : MonoBehaviour
    {
        [SerializeField] private PlayerHealthController healthController;
        [SerializeField] private float damageInterval = 3f;
        [SerializeField] private bool startDamageOnStart = true;
        
        private Coroutine _damageCoroutine;

        #region Unity Lifecycle
        private void Start()
        {
            if (startDamageOnStart)
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
        public void StartContinuousDamage()
        {
            if (_damageCoroutine == null)
                _damageCoroutine = StartCoroutine(DamageLoop());
        }

        public void StopContinuousDamage()
        {
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator DamageLoop()
        {
            while (healthController && healthController.CurrentHp > 0)
            {
                yield return new WaitForSeconds(damageInterval);
                
                if (healthController && healthController.CurrentHp > 0)
                {
                    healthController.Damage(1);
                }
            }
            _damageCoroutine = null;
        }
        #endregion
    }
}

using Health.Interfaces;
using UnityEngine;

namespace Enemy
{
    public class EnemyDeath : MonoBehaviour
    {
        private IHealthEvents _healthEvents;
        private void Start()
        {
            _healthEvents = GetComponent<IHealthEvents>();
            if (_healthEvents != null)
                _healthEvents.OnDeath += HandleDeath;
        }
        private void OnDisable()
        {
            if (_healthEvents != null)
                _healthEvents.OnDeath -= HandleDeath;
        }
        private void HandleDeath()
        {
            gameObject.SetActive(false);
        }
    }
}

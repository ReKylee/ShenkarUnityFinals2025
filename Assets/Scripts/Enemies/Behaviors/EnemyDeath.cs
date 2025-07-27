using UnityEngine;

namespace Enemies.Behaviors
{
    // Disables the enemy GameObject forever when Die() is called
    public class EnemyDeath : MonoBehaviour
    {
        private Health.Interfaces.IHealthEvents _healthEvents;

        private void Awake()
        {
            _healthEvents = GetComponent<Health.Interfaces.IHealthEvents>();
            if (_healthEvents != null)
                _healthEvents.OnDeath += Die;
        }

        private void OnDestroy()
        {
            if (_healthEvents != null)
                _healthEvents.OnDeath -= Die;
        }

        private void Die()
        {
            gameObject.SetActive(false);
        }
    }
}

using Health.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Handles dropping a prefab when the enemy dies
    public class EnemyDropOnDeath : MonoBehaviour
    {
        public GameObject dropPrefab;
        private IHealthEvents _healthEvents;

        private void Awake()
        {
            _healthEvents = GetComponent<IHealthEvents>();
            if (_healthEvents != null)
                _healthEvents.OnDeath += Drop;
        }

        private void OnDestroy()
        {
            if (_healthEvents != null)
                _healthEvents.OnDeath -= Drop;
        }

        private void Drop()
        {
            if (dropPrefab)
                Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
    }
}

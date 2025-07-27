using UnityEngine;
using Health.Interfaces;
using System.Threading.Tasks;

namespace Enemies.Behaviors
{
    // Disables the enemy GameObject, then respawns it at the same position after a delay
    public class RespawningEnemyDeath : MonoBehaviour
    {
        [SerializeField] private float respawnDelay = 3f;
        private IHealthEvents _healthEvents;
        private IHealth _health;

        private void Awake()
        {
            _healthEvents = GetComponent<IHealthEvents>();
            _health = GetComponent<IHealth>();
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
            _ = RespawnTask();
        }

        private async Task RespawnTask()
        {
            gameObject.SetActive(false);
            await Task.Delay((int)(respawnDelay * 1000f));
            gameObject.SetActive(true);
            _health.Heal(_health.MaxHp);
            Debug.Log("Health reset to " + _health.MaxHp + " after respawn.");
        }
    }
}

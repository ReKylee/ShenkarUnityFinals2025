using UnityEngine;
using Health.Interfaces;
using System.Threading.Tasks;

namespace Enemies.Behaviors
{
    // Disables the enemy GameObject, then respawns it at the same position after a delay
    public class RespawningEnemyDeath : MonoBehaviour
    {
        [SerializeField] private float respawnDelay = 3f;
        private Vector3 _respawnPosition;
        private Quaternion _respawnRotation;
        private IHealthEvents _healthEvents;

        private void Awake()
        {
            _respawnPosition = transform.position;
            _respawnRotation = transform.rotation;
            _healthEvents = GetComponent<IHealthEvents>();
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
            transform.position = _respawnPosition;
            transform.rotation = _respawnRotation;
            gameObject.SetActive(true);
        }
    }
}

using Health.Interfaces;
using UnityEngine;

namespace Enemy
{
    public class EnemyDeath : MonoBehaviour
    {
        private IHealthEvents _simpleHealthController;
        private void Start()
        {
            _simpleHealthController = GetComponent<IHealthEvents>();
            _simpleHealthController.OnEmpty += HandleDeath;
        }
        private void OnDisable()
        {
            _simpleHealthController.OnEmpty -= HandleDeath;
        }

        private void HandleDeath()
        {
            gameObject.SetActive(false);
        }
    }
}

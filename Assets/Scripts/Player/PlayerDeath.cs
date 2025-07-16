using Health.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerDeath : MonoBehaviour
    {
        public UnityEvent onDeath;
        private IHealthEvents _livesController;

        private void Start()
        {
            _livesController = GetComponent<IHealthEvents>();
            _livesController.OnEmpty += HandleDeath;
        }
        private void OnDisable()
        {
            _livesController.OnEmpty -= HandleDeath;
        }
        private void HandleDeath()
        {
            onDeath?.Invoke();
        }
    }
}

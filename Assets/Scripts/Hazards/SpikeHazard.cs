using Health.Interfaces;
using UnityEngine;

namespace Hazards
{
    public class SpikeHazard : MonoBehaviour, IDamageDealer
    {
        [SerializeField] private int damageAmount = 1;
        private bool _damaged;
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Spike Collission With " + other.gameObject.name);
            if (_damaged) return;
            if (other.gameObject.CompareTag("Player"))
            {
                _damaged = true;
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _damaged = false;
            }
        }
        public int GetDamageAmount() => damageAmount;
    }
}

using Health.Interfaces;
using UnityEngine;

namespace Hazards
{
    public class EnemyHazard : MonoBehaviour, IDamageDealer
    {

        [SerializeField] private int damageAmount = 1;
        public int GetDamageAmount() => damageAmount;
    }
}

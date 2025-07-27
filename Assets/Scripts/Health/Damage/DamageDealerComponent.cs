using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{
    [DisallowMultipleComponent]
    public class DamageDealerComponent : MonoBehaviour, IDamageDealer
    {
        [SerializeField] private int damageAmount = 1;
        public int GetDamageAmount() => damageAmount;
    }
}

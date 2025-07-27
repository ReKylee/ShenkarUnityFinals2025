using System.Linq;
using Health.Interfaces;
using UnityEngine;

namespace Health.Damage
{
    [DisallowMultipleComponent]
    public class DamageConditionsComponent : MonoBehaviour
    {
        [Tooltip("Assign components implementing IDamageCondition here.")] [SerializeField]
        private MonoBehaviour[] damageConditions;

        private IDamageCondition[] _conditions;
        private void Awake() 
            => _conditions = damageConditions.Cast<IDamageCondition>().ToArray();
        public bool CanBeDamagedBy(GameObject damager)
            => _conditions.All(cond => cond == null || cond.CanBeDamagedBy(damager));
    }
}

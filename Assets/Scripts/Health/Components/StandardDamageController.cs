using Health.Interfaces;
using UnityEngine;

namespace Health.Components
{
    /// <summary>
    /// Standard damage controller: always processes incoming damage and applies it directly.
    /// </summary>
    [RequireComponent(typeof(IDamageable))]
    public class StandardDamageController : BaseDamageController
    {
        protected override bool ShouldProcessDealer(IDamageDealer dealer) => true;

        protected override void ProcessDamage(IDamageDealer dealer) => Damageable.Damage(dealer.GetDamageAmount());
    }
}

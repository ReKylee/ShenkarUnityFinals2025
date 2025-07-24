using Health.Interfaces;
using UnityEngine;

namespace Health.Components
{
    public class PlayerDamageController : BaseDamageController
    {
        public bool IsInvulnerable { get; set; }

        protected override bool ShouldProcessDealer(IDamageDealer dealer) => !IsInvulnerable;
        protected override void ProcessDamage(IDamageDealer dealer) => Damageable.Damage(dealer.GetDamageAmount());
    }
}

using Health.Interfaces;

namespace Health.Components
{
    /// <summary>
    ///     Damage controller for enemies: only processes damage from invincibility dealers (player when invincible).
    /// </summary>
    public class OnlyInvincibilityDamageController : BaseDamageController
    {
        protected override bool ShouldProcessDealer(IDamageDealer dealer) => dealer is IInvincibilityDealer;

        protected override void ProcessDamage(IDamageDealer dealer) => Damageable.Damage(dealer.GetDamageAmount());
    }
}

using UnityEngine;

namespace ModularCharacterController.Core.Abilities.Interfaces
{
    /// <summary>
    ///     Interface for abilities that provide attack functionality
    /// </summary>
    public interface IAttackAbilityModule : IAbilityModule
    {

        /// <summary>
        ///     Attack range in units
        /// </summary>
        float AttackRange { get; }

        /// <summary>
        ///     Base damage dealt by this attack
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        ///     Cooldown between attacks in seconds
        /// </summary>
        float AttackCooldown { get; }

        /// <summary>
        ///     Whether this attack is currently on cooldown
        /// </summary>
        bool IsOnCooldown { get; }

        /// <summary>
        ///     Types of attack (melee, projectile, etc.)
        /// </summary>
        AttackType AttackType { get; }

        /// <summary>
        ///     Perform the primary attack action
        /// </summary>
        /// <param name="direction">Direction of the attack (usually facing direction)</param>
        void PerformAttack(Vector2 direction);

        /// <summary>
        ///     Perform a secondary/special attack if available
        /// </summary>
        /// <param name="direction">Direction of the attack (usually facing direction)</param>
        /// <returns>True if a secondary attack was performed</returns>
        bool PerformSecondaryAttack(Vector2 direction);
    }

    /// <summary>
    ///     Categories of attack types
    /// </summary>
    public enum AttackType
    {
        Melee,
        Projectile,
        Area,
        Beam,
        Special
    }
}

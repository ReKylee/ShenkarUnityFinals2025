using System;

namespace Player.Components
{
    public interface IDamageShield
    {
        /// <summary>
        /// Event raised when the shield absorbs damage
        /// </summary>
        event Action<int> OnDamageAbsorbed;

        bool IsActive { get; }
        /// <summary>
        /// Activate the shield (called when entering transformation)
        /// </summary>
        void Activate();
        /// <summary>
        /// Try to absorb damage with the shield
        /// </summary>
        /// <param name="amount">Damage amount</param>
        /// <returns>True if damage was absorbed, false if shield is inactive</returns>
        bool TryAbsorbDamage(int amount);
        /// <summary>
        /// Manually deactivate the shield (e.g., when transformation ends)
        /// </summary>
        void Deactivate();
    }
}

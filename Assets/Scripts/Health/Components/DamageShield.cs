using System;
using UnityEngine;

namespace Player.Components
{

    /// <summary>
    /// Simple transformation shield that absorbs one hit and then deactivates itself.
    /// Used to provide extra hit protection during animal transformations.
    /// </summary>
    public class DamageShield : MonoBehaviour, IDamageShield
    {
        private bool _isActive = false;

        /// <summary>
        /// Event raised when the shield absorbs damage
        /// </summary>
        public event Action<int> OnDamageAbsorbed;

        public bool IsActive => _isActive;

        /// <summary>
        /// Activate the shield (called when entering transformation)
        /// </summary>
        public void Activate()
        {
            _isActive = true;
        }

        /// <summary>
        /// Try to absorb damage with the shield
        /// </summary>
        /// <param name="amount">Damage amount</param>
        /// <returns>True if damage was absorbed, false if shield is inactive</returns>
        public bool TryAbsorbDamage(int amount)
        {
            if (!_isActive) return false;

            // Shield absorbs the hit and deactivates
            _isActive = false;

            // Notify subscribers that damage was absorbed
            OnDamageAbsorbed?.Invoke(amount);

            return true;
        }

        /// <summary>
        /// Manually deactivate the shield (e.g., when transformation ends)
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
        }
    }
}

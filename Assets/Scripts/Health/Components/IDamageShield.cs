namespace Health.Components
{
    /// <summary>
    /// Interface for damage shields that can absorb hits
    /// </summary>
    public interface IDamageShield
    {
        /// <summary>
        /// Whether the shield is currently active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Activate the shield
        /// </summary>
        void Activate();
        
        /// <summary>
        /// Try to absorb damage with the shield
        /// </summary>
        /// <param name="amount">Damage amount</param>
        /// <returns>True if damage was absorbed, false if shield is inactive</returns>
        bool TryAbsorbDamage(int amount);
        
        /// <summary>
        /// Deactivate the shield
        /// </summary>
        void Deactivate();
    }
}


namespace Health.Interfaces
{
    /// <summary>
    ///     Extended interface for components that can take damage with bypass options
    /// </summary>
    public interface IBypassableDamageable : IDamageable
    {
        /// <summary>
        ///     Apply damage that bypasses any shields or protection
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        void DamageBypass(int amount);
    }
}

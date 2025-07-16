namespace Health.Interfaces
{
    /// <summary>
    ///     Comprehensive interface that combines all health-related functionality
    /// </summary>
    public interface IFullHealthSystem : IDamageable, IHealable, IHealthModifier, IHealthEvents
    {
    }
}

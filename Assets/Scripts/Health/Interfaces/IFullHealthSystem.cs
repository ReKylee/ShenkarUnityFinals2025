namespace Health.Interfaces
{
    /// <summary>
    ///     Interface that combines all health-related functionality
    /// </summary>
    public interface IFullHealthSystem : IDamageable, IHealable, IHealthModifier, IHealthEvents
    {
    }
}

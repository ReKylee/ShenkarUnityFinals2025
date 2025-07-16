namespace Health.Interfaces
{
    /// <summary>
    ///     Interface for objects that can take damage
    /// </summary>
    public interface IDamageable : IHealth
    {
        void Damage(int amount);
    }
}

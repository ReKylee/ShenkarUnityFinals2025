namespace Health.Interfaces
{
    /// <summary>
    ///     Interface for objects that can be healed
    /// </summary>
    public interface IHealable : IHealth
    {
        void Heal(int amount);
    }
}

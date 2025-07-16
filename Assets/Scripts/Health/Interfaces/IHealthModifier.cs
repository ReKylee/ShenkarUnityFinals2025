namespace Health.Interfaces
{
    /// <summary>
    ///     Interface for objects that can directly modify health
    /// </summary>
    public interface IHealthModifier : IHealth
    {
        void SetHp(int hp);
    }
}

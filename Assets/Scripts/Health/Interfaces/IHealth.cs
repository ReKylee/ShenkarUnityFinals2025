namespace Health.Interfaces
{
    /// <summary>
    ///     Interface for reading health state
    /// </summary>
    public interface IHealth
    {
        int CurrentHp { get; }
        int MaxHp { get; }
    }
}

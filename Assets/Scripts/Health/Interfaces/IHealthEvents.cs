using System;

namespace Health.Interfaces
{
    /// <summary>
    ///     Interface for health-related events
    /// </summary>
    public interface IHealthEvents
    {
        event Action<int, int> OnHealthChanged;
        event Action OnEmpty;
    }
}

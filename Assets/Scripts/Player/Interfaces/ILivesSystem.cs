using System;

namespace Player.Interfaces
{
    /// <summary>
    ///     Interface for tracking player lives
    /// </summary>
    public interface ILivesSystem
    {
        int CurrentLives { get; }
        int MaxLives { get; }
        event Action<int, int> OnLivesChanged;
        void Reset();
    }
}

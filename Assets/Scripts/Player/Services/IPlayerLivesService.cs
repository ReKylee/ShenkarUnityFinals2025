using System;

namespace Player.Services
{
    public interface IPlayerLivesService
    {
        int CurrentLives { get; }
        int MaxLives { get; }
        bool HasLivesRemaining { get; }
        
        bool TryUseLife();
        void ResetLives();
        
        event Action<int> OnLivesChanged;
    }

}



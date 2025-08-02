using System;
using UnityEngine;

namespace Player.Interfaces
{
    public interface IPlayerLivesService
    {
        int CurrentLives { get; }
        int MaxLives { get; }
        bool HasLivesRemaining { get; }

        bool TryUseLife();
        void ResetLives();
        void AddLife(Vector3 collectPosition);

        event Action<int> OnLivesChanged;
    }

}

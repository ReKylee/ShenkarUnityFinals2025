using System;
using Core;
using Core.Data;
using Player.Interfaces;
using UnityEngine;

namespace Player.Services
{
    public class PlayerLivesService : IPlayerLivesService
    {
        private readonly GameDataCoordinator _gameDataCoordinator;

        public PlayerLivesService(GameDataCoordinator gameDataCoordinator)
        {
            _gameDataCoordinator = gameDataCoordinator;
            MaxLives = GameData.MaxLives;

            Debug.Log("[PlayerLivesService] Initialized with max lives: " + MaxLives);
        }

        public int CurrentLives => _gameDataCoordinator.GetCurrentData()?.lives ?? MaxLives;
        public int MaxLives { get; }
        public bool HasLivesRemaining => CurrentLives > 0;

        public event Action<int> OnLivesChanged;
        public event Action<Vector3> OnOneUpAwarded;

        public bool TryUseLife()
        {
            if (CurrentLives <= 0) return false;

            int newLives = CurrentLives - 1;
            _gameDataCoordinator.UpdateLives(newLives);
            OnLivesChanged?.Invoke(newLives);

            return newLives > 0;
        }

        public void ResetLives()
        {
            _gameDataCoordinator.UpdateLives(MaxLives);
            OnLivesChanged?.Invoke(MaxLives);
        }

        public void AddLife(Vector3 collectPosition)
        {
            int newLives = CurrentLives + 1;
            _gameDataCoordinator.UpdateLives(newLives);
            OnLivesChanged?.Invoke(newLives);
            OnOneUpAwarded?.Invoke(collectPosition);
        }
    }
}

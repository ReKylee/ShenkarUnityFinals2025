using System;
using Core.Data;
using Player.Interfaces;
using UnityEngine;

namespace Player.Services
{
    public class PlayerLivesService : IPlayerLivesService
    {
        private readonly IGameDataService _gameDataService;

        public PlayerLivesService(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
            MaxLives = GameData.MaxLives;

            Debug.Log("[PlayerLivesService] Initialized with max lives: " + MaxLives);

            if (_gameDataService == null)
            {
                Debug.LogError("[PlayerLivesService] _gameDataService is null.");
            }
        }

        public int CurrentLives => _gameDataService.CurrentData.lives;
        public int MaxLives { get; }

        public bool HasLivesRemaining => CurrentLives > 0;

        public event Action<int> OnLivesChanged;
        public event Action<Vector3> OnOneUpAwarded;
        public bool TryUseLife()
        {
            if (CurrentLives <= 0) return false;

            int newLives = CurrentLives - 1;

            // Update lives before publishing the event
            _gameDataService.UpdateLives(newLives);

            OnLivesChanged?.Invoke(newLives);

            return newLives > 0;
        }

        public void ResetLives()
        {
            _gameDataService.UpdateLives(MaxLives);
            OnLivesChanged?.Invoke(MaxLives);
        }

        public void AddLife(Vector3 collectPosition)
        {
            int newLives = CurrentLives + 1;

            // Update lives before publishing the event
            _gameDataService.UpdateLives(newLives);

            OnLivesChanged?.Invoke(newLives);
            OnOneUpAwarded?.Invoke(collectPosition);
        }
    }
}

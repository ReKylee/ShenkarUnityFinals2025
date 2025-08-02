using System;
using Core.Data;
using Core.Events;
using Player.Interfaces;
using UnityEngine;

namespace Player.Services
{
    public class PlayerLivesService : IPlayerLivesService
    {
        private readonly IEventBus _eventBus;
        private readonly IGameDataService _gameDataService;

        public PlayerLivesService(IGameDataService gameDataService, IEventBus eventBus)
        {
            _gameDataService = gameDataService;
            _eventBus = eventBus;
            MaxLives = GameData.MaxLives;

            Debug.Log("[PlayerLivesService] Initialized with max lives: " + MaxLives);
            
            if (_gameDataService == null)
            {
                Debug.LogError("[PlayerLivesService] _gameDataService is null.");
            }

            if (_eventBus == null)
            {
                Debug.LogError("[PlayerLivesService] _eventBus is null.");
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
            _gameDataService.UpdateLives(newLives);

            OnLivesChanged?.Invoke(newLives);

            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = newLives,
                MaxLives = MaxLives,
                Timestamp = Time.time
            });

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
            _gameDataService.UpdateLives(newLives);
            OnLivesChanged?.Invoke(newLives);
            OnOneUpAwarded?.Invoke(collectPosition);
            _eventBus?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = newLives,
                MaxLives = MaxLives,
                Timestamp = Time.time
            });
        }
    }
}

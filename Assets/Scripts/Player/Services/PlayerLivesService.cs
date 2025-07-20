using System;
using Core.Data;
using Core.Events;

namespace Player.Services
{
    public class PlayerLivesService : IPlayerLivesService
    {
        private readonly IGameDataService _gameDataService;
        private readonly IEventBus _eventBus;
        private readonly int _maxLives;

        public int CurrentLives => _gameDataService.CurrentData.lives;
        public int MaxLives => _maxLives;
        public bool HasLivesRemaining => CurrentLives > 0;

        public event Action<int> OnLivesChanged;

        public PlayerLivesService(IGameDataService gameDataService, IEventBus eventBus)
        {
            _gameDataService = gameDataService;
            _eventBus = eventBus;
            _maxLives = _gameDataService.CurrentData.lives;

            UnityEngine.Debug.Log("[PlayerLivesService] Initialized with max lives: " + _maxLives);
            if (_gameDataService == null)
            {
                UnityEngine.Debug.LogError("[PlayerLivesService] _gameDataService is null.");
            }
            if (_eventBus == null)
            {
                UnityEngine.Debug.LogError("[PlayerLivesService] _eventBus is null.");
            }
        }

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
                Timestamp = UnityEngine.Time.time
            });

            return newLives > 0; // Return true if still has lives after using one
        }

        public void ResetLives()
        {
            _gameDataService.UpdateLives(_maxLives);
            OnLivesChanged?.Invoke(_maxLives);
        }
    }
}

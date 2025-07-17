using System;
using Core.Events;
using UnityEngine;

namespace Core.Lives
{
    public interface ILivesService
    {
        int CurrentLives { get; }
        int MaxLives { get; }
        void LoseLife();
        void AddLife();
        void SetLives(int lives);
        void ResetLives();
        event Action<int, int> OnLivesChanged;
        event Action OnAllLivesLost;
    }

    public class LivesService : ILivesService
    {
        private readonly IGameEventPublisher _gameEventPublisher;
        private readonly int _maxLives;
        private int _currentLives;

        public int CurrentLives => _currentLives;
        public int MaxLives => _maxLives;

        public event Action<int, int> OnLivesChanged;
        public event Action OnAllLivesLost;

        public LivesService(IGameEventPublisher gameEventPublisher, int maxLives = 3)
        {
            _gameEventPublisher = gameEventPublisher;
            _maxLives = maxLives;
            _currentLives = maxLives;
        }

        public void LoseLife()
        {
            if (_currentLives <= 0) return;
            
            _currentLives--;
            NotifyLivesChanged();

            if (_currentLives <= 0)
            {
                OnAllLivesLost?.Invoke();
            }
        }

        public void AddLife()
        {
            _currentLives = Mathf.Min(_currentLives + 1, _maxLives);
            NotifyLivesChanged();
        }

        public void ResetLives()
        {
            _currentLives = _maxLives;
            NotifyLivesChanged();
        }

        public void SetLives(int lives)
        {
            _currentLives = Mathf.Clamp(lives, 0, _maxLives);
            NotifyLivesChanged();
        }

        private void NotifyLivesChanged()
        {
            OnLivesChanged?.Invoke(_currentLives, _maxLives);
            _gameEventPublisher.PublishPlayerLivesChanged(_currentLives, _maxLives);
        }
    }
}

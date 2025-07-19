using System;
using VContainer;

namespace Core.Data
{
    public class GameDataService : IGameDataService
    {
        private readonly IGameDataRepository _repository;
        private GameData _currentData;

        public GameData CurrentData => _currentData;
        public event Action<GameData> OnDataChanged;

        [Inject]
        public GameDataService(IGameDataRepository repository)
        {
            _repository = repository;
            _currentData = _repository.LoadData();
        }

        public void UpdateLives(int lives)
        {
            _currentData.lives = lives;
            NotifyDataChanged();
        }

        public void UpdateScore(int score)
        {
            _currentData.score = score;
            NotifyDataChanged();
        }

        public void UpdateCoins(int coins)
        {
            _currentData.coins = coins;
            NotifyDataChanged();
        }
        public void UpdateCurrentLevel(string levelName)
        {
            _currentData.currentLevel = levelName;
            NotifyDataChanged();
        }
        public void UpdatePowerUp(string powerUpName, bool unlocked)
        {
            switch (powerUpName.ToLower())
            {
                case "fireball":
                    _currentData.hasFireball = unlocked;
                    break;
                case "axe":
                    _currentData.hasAxe = unlocked;
                    break;
            }
            NotifyDataChanged();
        }

        public void UpdateBestTime(float time)
        {
            if (time < _currentData.bestTime)
            {
                _currentData.bestTime = time;
                NotifyDataChanged();
            }
        }

        public bool HasPowerUp(string powerUpName)
        {
            return powerUpName.ToLower() switch
            {
                "fireball" => _currentData.hasFireball,
                "axe" => _currentData.hasAxe,
                _ => false
            };
        }

        public void ResetAllData()
        {
            _repository.DeleteData();
            _currentData = _repository.LoadData();
            NotifyDataChanged();
        }

        public void SaveData()
        {
            _repository.SaveData(_currentData);
        }

        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(_currentData);
        }
    }
}

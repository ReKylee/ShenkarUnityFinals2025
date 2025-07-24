﻿using System;
using VContainer;

namespace Core.Data
{
    public class GameDataService : IGameDataService
    {
        private readonly IGameDataRepository _repository;

        [Inject]
        public GameDataService(IGameDataRepository repository)
        {
            _repository = repository;
            CurrentData = _repository.LoadData();
        }

        public GameData CurrentData { get; private set; }

        public event Action<GameData> OnDataChanged;

        public void UpdateLives(int lives)
        {
            CurrentData.lives = lives;
            NotifyDataChanged();
        }

        public void UpdateScore(int score)
        {
            CurrentData.score = score;
            NotifyDataChanged();
        }


        public void UpdateCurrentLevel(string levelName)
        {
            CurrentData.currentLevel = levelName;
            NotifyDataChanged();
        }

        public void UpdateBestTime(float time)
        {
            if (time < CurrentData.bestTime)
            {
                CurrentData.bestTime = time;
                NotifyDataChanged();
            }
        }

        public void ResetAllData()
        {
            _repository.DeleteData();
            CurrentData = _repository.LoadData();
            NotifyDataChanged();
        }

        public void SaveData()
        {
            _repository.SaveData(CurrentData);
        }
        public void UpdatePowerUp(string powerUpName, bool unlocked)
        {
            switch (powerUpName.ToLower())
            {
                case "fireball":
                    CurrentData.hasFireball = unlocked;
                    break;
                case "axe":
                    CurrentData.hasAxe = unlocked;
                    break;
            }

            NotifyDataChanged();
        }

        public bool HasPowerUp(string powerUpName)
        {
            return powerUpName.ToLower() switch
            {
                "fireball" => CurrentData.hasFireball,
                "axe" => CurrentData.hasAxe,
                _ => false
            };
        }

        private void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(CurrentData);
        }
    }
}

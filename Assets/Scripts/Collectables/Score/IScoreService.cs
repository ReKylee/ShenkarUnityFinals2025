﻿using System;

namespace Collectables.Score
{
    public interface IScoreService
    {
        public int CurrentScore { get; }
        public void AddScore(int amount);
        public void ResetScore();
        public event Action<int> ScoreChanged;
    }

}

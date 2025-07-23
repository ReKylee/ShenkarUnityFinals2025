using UnityEngine;

namespace Core.Events
{
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver,
        Victory,
        Restarting
    }

    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;
        public float Timestamp;
    }

    public struct PlayerHealthChangedEvent
    {
        public int CurrentHp;
        public int MaxHp;
        public int Damage;
        public float Timestamp;
    }

    public struct PlayerLivesChangedEvent
    {
        public int CurrentLives;
        public int MaxLives;
        public float Timestamp;
    }

    public struct ScoreChangedEvent
    {
        public Vector3 Position;
        public int ScoreAmount; // Delta amount
        public int TotalScore;  // Overall score
    }

    // Base event structure for consistency
    public interface IGameEvent
    {
        float Timestamp { get; }
    }

    public struct PlayerDeathEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public Vector3 DeathPosition;
    }

    public struct LevelStartedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string LevelName;
    }

    public struct LevelCompletedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string LevelName;
        public float CompletionTime;
    }


    public struct GameOverEvent : IGameEvent
    {
        public float Timestamp { get; set; }
    }
}

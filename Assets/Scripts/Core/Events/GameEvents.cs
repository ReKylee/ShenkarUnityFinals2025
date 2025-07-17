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

    public struct PlayerDeathEvent
    {
        public float Timestamp;
        public Vector3 DeathPosition;
    }

    public struct LevelStartedEvent
    {
        public string LevelName;
        public float Timestamp;
    }

    public struct LevelCompletedEvent
    {
        public string LevelName;
        public float CompletionTime;
        public float Timestamp;
    }

    public struct LevelFailedEvent
    {
        public string LevelName;
        public string FailureReason;
        public float Timestamp;
    }

    public struct GameOverEvent
    {
        public float Timestamp;
    }
}

using UnityEngine;

namespace Core.Events
{
    public enum GameState
    {
        MainMenu,
        LevelSelection, // Added LevelSelection state
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
        public int PreviousLives;
        public int CurrentLives;
        public int MaxLives;
        public float Timestamp;
    }

    public struct ScoreChangedEvent
    {
        public Vector3 Position;
        public int ScoreAmount; // Delta amount
        public int TotalScore; // Overall score
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

    public struct GameCompletedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string FinalLevelName;
    }

    // Level Selection Events
    public struct LevelSelectedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string LevelName;
        public int LevelIndex;
    }

    public struct LevelNavigationEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public int PreviousIndex;
        public int NewIndex;
        public Vector2 Direction;
    }

    public struct ItemSelectScreenRequestedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string LevelName;
    }

    public struct LevelLoadRequestedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string LevelName;
        public string SceneName;
    }

    public struct LevelUnlockedEvent : IGameEvent
    {
        public float Timestamp { get; set; }
        public string CompletedLevelName;
        public string UnlockedLevelName;
    }
}

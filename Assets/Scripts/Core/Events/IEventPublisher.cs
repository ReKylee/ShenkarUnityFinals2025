using UnityEngine;

namespace Core.Events
{
    public interface IEventPublisher
    {
        void Publish<T>(T eventData) where T : struct;
    }

    public interface IGameEventPublisher
    {
        void PublishPlayerLivesChanged(int currentLives, int maxLives);
        void PublishGameOver();
        void PublishLevelFailed(string levelName, string reason);
        void PublishPlayerDeath(Vector3 position);
        void PublishLevelCompleted(string levelName, float completionTime);
        void PublishLevelStarted(string levelName);
    }

    public class GameEventPublisher : IGameEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;

        public GameEventPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public void PublishPlayerLivesChanged(int currentLives, int maxLives)
        {
            _eventPublisher?.Publish(new PlayerLivesChangedEvent
            {
                CurrentLives = currentLives,
                MaxLives = maxLives,
                Timestamp = Time.time
            });
        }

        public void PublishGameOver()
        {
            _eventPublisher?.Publish(new GameOverEvent
            {
                Timestamp = Time.time
            });
        }

        public void PublishLevelFailed(string levelName, string reason)
        {
            _eventPublisher?.Publish(new LevelFailedEvent
            {
                LevelName = levelName,
                FailureReason = reason,
                Timestamp = Time.time
            });
        }

        public void PublishPlayerDeath(Vector3 position)
        {
            _eventPublisher?.Publish(new PlayerDeathEvent
            {
                DeathPosition = position,
                Timestamp = Time.time
            });
        }

        public void PublishLevelCompleted(string levelName, float completionTime)
        {
            _eventPublisher?.Publish(new LevelCompletedEvent
            {
                LevelName = levelName,
                CompletionTime = completionTime,
                Timestamp = Time.time
            });
        }

        public void PublishLevelStarted(string levelName)
        {
            _eventPublisher?.Publish(new LevelStartedEvent
            {
                LevelName = levelName,
                Timestamp = Time.time
            });
        }
    }
}

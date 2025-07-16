using System;

namespace GameEvents.Interfaces
{
    public interface IEventPublisher
    {
        void Publish<T>(T eventData) where T : struct;
    }

    public interface IEventSubscriber
    {
        void Subscribe<T>(Action<T> handler) where T : struct;
        void Unsubscribe<T>(Action<T> handler) where T : struct;
    }

    public interface IEventBus : IEventPublisher, IEventSubscriber
    {
        void Clear();
    }
}

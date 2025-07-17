using System;

namespace Core.Events
{
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

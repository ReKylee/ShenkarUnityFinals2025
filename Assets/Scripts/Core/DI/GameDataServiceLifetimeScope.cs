using Core.Data;
using Core.Events;
using Core.Services;
using LevelSelection.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.DI
{
    /// <summary>
    /// Minimal lifetime scope for GameDataService only
    /// </summary>
    public class GameDataServiceLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Register Core Services
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // Register EventBus as IEventPublisher as well (since EventBus implements IEventPublisher)
            builder.Register<IEventPublisher>(resolver => resolver.Resolve<IEventBus>(), Lifetime.Singleton);

            builder.Register<IGameDataRepository, JsonGameDataRepository>(Lifetime.Singleton);
            builder.Register<IGameDataService, GameDataService>(Lifetime.Singleton);
            builder.Register<IAutoSaveService, AutoSaveService>(Lifetime.Singleton);
            builder.Register<ILevelDiscoveryService, LevelDiscoveryService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<GameDataCoordinator>();
        }
    }
}


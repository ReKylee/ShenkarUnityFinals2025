using Core.Data;
using Core.Events;
using Core.Services;
using LevelSelection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace LevelSelection.DI
{
    public class LevelSelectionLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[LevelSelectionLifetimeScope] Configuring level selection DI container...");
            // Register Core Services
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // Register EventBus as IEventPublisher as well (since EventBus implements IEventPublisher)
            builder.Register<IEventPublisher>(resolver => resolver.Resolve<IEventBus>(), Lifetime.Singleton);

            builder.Register<IGameDataRepository, JsonGameDataRepository>(Lifetime.Singleton);
            builder.Register<IGameDataService, GameDataService>(Lifetime.Singleton);
            builder.Register<IAutoSaveService, AutoSaveService>(Lifetime.Singleton);
            // Register level selection specific components using RegisterComponentInHierarchy
            builder.RegisterComponentInHierarchy<LevelSelectionManager>();
            builder.RegisterComponentInHierarchy<LevelSelector>();
            builder.RegisterComponentInHierarchy<ItemSelectScreen>();
            builder.RegisterComponentInHierarchy<NESCrossfade>();

            // Register level selection director as singleton for this scope
            builder.Register<LevelSelectionDirector>(Lifetime.Singleton);

            Debug.Log("[LevelSelectionLifetimeScope] Level selection DI container configured successfully.");
        }

    }
}

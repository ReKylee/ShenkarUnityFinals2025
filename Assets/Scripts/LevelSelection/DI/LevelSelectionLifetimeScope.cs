using Core.Data;
using Core.Events;
using Core.Services;
using LevelSelection.Services;
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

            // Register core services that level selection depends on
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
            builder.Register<IGameDataRepository, JsonGameDataRepository>(Lifetime.Singleton);
            builder.Register<IGameDataService, GameDataService>(Lifetime.Singleton);
            builder.Register<IAutoSaveService, AutoSaveService>(Lifetime.Singleton);

            // Register the new service-based architecture
            builder.Register<ILevelDiscoveryService, LevelDiscoveryService>(Lifetime.Singleton);
            builder.Register<ILevelNavigationService, LevelNavigationService>(Lifetime.Singleton);
            builder.Register<ILevelDisplayService, LevelDisplayService>(Lifetime.Singleton);

            // Register the main controller
            builder.RegisterComponentInHierarchy<LevelSelectionController>();

            // Register supporting components that are still used
            builder.RegisterComponentInHierarchy<ItemSelectScreen>();
            builder.RegisterComponentInHierarchy<NESCrossfade>();
            
            // Register the scene transition manager
            builder.RegisterComponentInHierarchy<SceneTransitionManager>();

            Debug.Log("[LevelSelectionLifetimeScope] Level selection DI container configured successfully.");
        }
    }
}

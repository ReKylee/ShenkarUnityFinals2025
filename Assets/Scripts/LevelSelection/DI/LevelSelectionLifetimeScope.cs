using Core;
using Core.Data;
using Core.Events;
using Core.Services;
using LevelSelection.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace LevelSelection.DI
{
    /// <summary>
    ///     VContainer lifetime scope for Level Selection services
    /// </summary>
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

            // Register core game management components
            builder.RegisterComponentInHierarchy<GameFlowManager>();
            builder.RegisterComponentInHierarchy<GameDataCoordinator>();

            // Register the new service-based architecture
            builder.Register<ILevelDiscoveryService, LevelDiscoveryService>(Lifetime.Scoped);
            builder.Register<ILevelNavigationService, LevelNavigationService>(Lifetime.Scoped);
            builder.Register<ILevelDisplayService, LevelDisplayService>(Lifetime.Scoped);

            // Register NEW focused services following SOLID principles
            builder.Register<ISelectorService, SelectorService>(Lifetime.Scoped);
            builder.Register<IInputFilterService, InputFilterService>(Lifetime.Scoped);
            builder.Register<IAudioFeedbackService, AudioFeedbackService>(Lifetime.Scoped);
            builder.Register<IItemSelectService, ItemSelectService>(Lifetime.Scoped);
            builder.Register<ISceneLoadService, SceneLoadService>(Lifetime.Scoped);

            // Register the main controller
            builder.RegisterComponentInHierarchy<LevelSelectionController>();

            // Register supporting components that are still used
            builder.RegisterComponentInHierarchy<ItemSelectScreen>();
            builder.RegisterComponentInHierarchy<NesCrossfade>();

            // Register the scene transition manager
            builder.RegisterComponentInHierarchy<SceneTransitionManager>();

            Debug.Log("[LevelSelectionLifetimeScope] Level selection DI container configured successfully.");
        }
    }
}

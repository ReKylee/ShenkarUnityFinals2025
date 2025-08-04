using Collectables.Score;
using Core.Data;
using Core.Events;
using Core.Services;
using Health.Damage;
using LevelSelection;
using LevelSelection.Services;
using Player.Components;
using Player.Interfaces;
using Player.Services;
using Player.UI;
using Pooling;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapons.Models;
using Weapons.Services;

namespace Core.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Awake()
        {
            Debug.Log("[GameLifetimeScope] Awake called.");
            AddToAutoInject<FireballWeapon>();
            base.Awake();
        }

        private void AddToAutoInject<T>() where T : Component
        {

            var components = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (T comp in components)
            {
                autoInjectGameObjects.Add(comp.gameObject);
            }
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[GameLifetimeScope] Configuring DI container...");

            // Register Core Services
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // Register EventBus as IEventPublisher as well (since EventBus implements IEventPublisher)
            builder.Register<IEventPublisher>(resolver => resolver.Resolve<IEventBus>(), Lifetime.Singleton);

            builder.Register<IGameDataRepository, JsonGameDataRepository>(Lifetime.Singleton);
            builder.Register<IGameDataService, GameDataService>(Lifetime.Singleton);
            builder.Register<IAutoSaveService, AutoSaveService>(Lifetime.Singleton);
            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
            builder.Register<ILevelDiscoveryService, LevelDiscoveryService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<SceneLoadService>().As<ISceneLoadService>();


            // Register Player Services
            builder.Register<IPlayerLivesService>(resolver
                => new PlayerLivesService(
                    resolver.Resolve<GameDataCoordinator>()
                ), Lifetime.Singleton);


            // Game Management
            builder.RegisterComponentInHierarchy<GameFlowManager>();
            builder.RegisterComponentInHierarchy<GameDataCoordinator>();
            builder.RegisterComponentInHierarchy<EndLevelZone>();
            builder.RegisterComponentInHierarchy<HealthBonusService>();
            // Pooling System
            builder.RegisterComponentInHierarchy<PoolManager>().As<IPoolService>();

            // Weapons
            builder.RegisterComponentInHierarchy<AxeWeapon>();
            builder.RegisterComponentInHierarchy<WeaponManagerService>();

            // Health
            builder.RegisterComponentInHierarchy<PlayerHealthController>();
            builder.RegisterComponentInHierarchy<PlayerLivesUIController>();
            builder.RegisterComponentInHierarchy<PlayerAnimationController>();
            builder.RegisterComponentInHierarchy<PeriodicBypassDamageDealer>();

            // Score System
            builder.RegisterComponentInHierarchy<ScoreController>();
            builder.RegisterComponentInHierarchy<PopupTextService>();


            Debug.Log("[GameLifetimeScope] DI container configured successfully.");
        }
    }
}

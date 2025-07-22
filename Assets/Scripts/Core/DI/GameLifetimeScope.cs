using Collectables.Score;
using Core.Data;
using Core.Events;
using Core.Services;
using Player.Components;
using Player.Interfaces;
using Player.Services;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace Core.DI
{
    public class GameLifetimeScope : LifetimeScope
    {


        protected override void Awake()
        {
            Debug.Log("[GameLifetimeScope] Awake called.");
            base.Awake();
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

            // Register Player Services
            builder.Register<IPlayerLivesService>(resolver
                => new PlayerLivesService(
                    resolver.Resolve<IGameDataService>(),
                    resolver.Resolve<IEventBus>()
                ), Lifetime.Singleton);

            // Game Management
            builder.RegisterComponentInHierarchy<GameFlowManager>();
            builder.RegisterComponentInHierarchy<GameDataCoordinator>();
            
            // Weapons
            builder.RegisterComponentInHierarchy<Weapons.Models.AxeWeapon>();
            builder.RegisterComponentInHierarchy<Weapons.Models.FireballWeapon>();
            builder.RegisterComponentInHierarchy<Weapons.Services.WeaponManagerService>();
            
            // Health
            builder.RegisterComponentInHierarchy<PlayerHealthController>();
            builder.RegisterComponentInHierarchy<Player.UI.PlayerLivesUIController>();
            
            // Score System
            builder.RegisterComponentInHierarchy<PopupTextService>();
            builder.Register<IPopupTextService>(resolver
                => resolver.Resolve<PopupTextService>(), Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<ScoreController>();
            builder.RegisterComponentInHierarchy<ScoreCollectable>();
            
           

            Debug.Log("[GameLifetimeScope] DI container configured successfully.");
        }


    }
}

using Core;
using Core.Data;
using Core.Events;
using Core.Flow;
using Core.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Game Settings")] 
        [SerializeField] private string currentLevelName = "Level_01";
        [SerializeField] private float autoSaveInterval = 30f;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register Core Services
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // Register EventBus as IEventPublisher as well (since EventBus implements IEventPublisher)
            builder.Register<IEventPublisher>(resolver => resolver.Resolve<IEventBus>(), Lifetime.Singleton);

            builder.Register<IGameDataRepository, JsonGameDataRepository>(Lifetime.Singleton);
            builder.Register<IGameDataService, GameDataService>(Lifetime.Singleton);
            builder.Register<IAutoSaveService, AutoSaveService>(Lifetime.Singleton);

            // Register Game Event Publisher
            builder.Register<IGameEventPublisher, GameEventPublisher>(Lifetime.Singleton);


            // Register Game Flow Controller with level name parameter
            builder.Register<IGameFlowController>(resolver =>
                    new GameFlowController(
                        resolver.Resolve<IGameEventPublisher>(),
                        currentLevelName),
                Lifetime.Singleton);

            // Register MonoBehaviour components in the scene for injection
            builder.RegisterComponentInHierarchy<GameManager>();
            builder.RegisterComponentInHierarchy<GameDataCoordinator>();
            builder.RegisterComponentInHierarchy<Weapons.Models.AxeWeapon>();
            builder.RegisterComponentInHierarchy<Weapons.Models.FireballWeapon>();
            builder.RegisterComponentInHierarchy<Weapons.Services.WeaponManagerService>();
            builder.RegisterComponentInHierarchy<Player.PlayerHealthController>();
            builder.RegisterComponentInHierarchy<Player.PlayerLivesController>();
            builder.RegisterComponentInHierarchy<Collectables.Coin.CoinController>();

        }

        protected override void Awake()
        {
            base.Awake();

            // Configure auto-save settings after container is built
            var autoSaveService = Container.Resolve<IAutoSaveService>();
            autoSaveService.SaveInterval = autoSaveInterval;
            autoSaveService.IsEnabled = true;
        }
    }
}

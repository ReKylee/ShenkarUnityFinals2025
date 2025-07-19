using Core.Data;
using Core.Events;
using Core.Services;
using Player.Services;
using VContainer;
using VContainer.Unity;

namespace Core.DI
{
    public class GameLifetimeScope : LifetimeScope
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

            // Register Player Services 
            builder.Register<IPlayerLivesService>(resolver
                => new PlayerLivesService(
                    resolver.Resolve<IGameDataService>(),
                    resolver.Resolve<IEventBus>()
                ), Lifetime.Singleton);

            // Register MonoBehaviour components in the scene for injection
            builder.RegisterComponentInHierarchy<GameFlowManager>();
            builder.RegisterComponentInHierarchy<GameDataCoordinator>();
            builder.RegisterComponentInHierarchy<Weapons.Models.AxeWeapon>();
            builder.RegisterComponentInHierarchy<Weapons.Models.FireballWeapon>();
            builder.RegisterComponentInHierarchy<Weapons.Services.WeaponManagerService>();
            builder.RegisterComponentInHierarchy<Player.PlayerHealthController>();
            builder.RegisterComponentInHierarchy<Player.UI.PlayerLivesUIController>();
            builder.RegisterComponentInHierarchy<Collectables.Coin.CoinController>();

        }

        protected override void Awake()
        {
            base.Awake();

            var autoSaveService = Container.Resolve<IAutoSaveService>();
            autoSaveService.SaveInterval = 30f; 
            autoSaveService.IsEnabled = true;
        }
    }
}

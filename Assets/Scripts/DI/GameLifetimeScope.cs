using Core;
using Core.Data;
using Core.Events;
using Core.Lives;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Game Components")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameDataCoordinator gameDataCoordinator;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register core systems as singletons
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            
            // Register SOLID-compliant data services
            builder.Register<JsonGameDataRepository>(Lifetime.Singleton).As<IGameDataRepository>();
            builder.Register<GameDataService>(Lifetime.Singleton).As<IGameDataService>();
            builder.Register<LivesService>(Lifetime.Singleton).As<ILivesService>();
            
            // Register managers
            if (gameManager)
                builder.RegisterComponent(gameManager);
                
            if (gameDataCoordinator)
                builder.RegisterComponent(gameDataCoordinator);
        }
    }
}

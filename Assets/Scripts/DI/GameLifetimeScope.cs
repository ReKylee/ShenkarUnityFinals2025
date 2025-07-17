using Core;
using GameEvents;
using GameEvents.Interfaces;
using Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Game Components")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PersistentDataManager persistentDataManager;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register core systems as singletons
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            
            // Register managers
            if (gameManager)
                builder.RegisterComponent(gameManager);
                
            if (persistentDataManager)
                builder.RegisterComponent(persistentDataManager);
        }
    }
}

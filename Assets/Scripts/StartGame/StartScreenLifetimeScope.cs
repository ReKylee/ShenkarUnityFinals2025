using Core;
using Core.Data;
using Core.Events;
using Core.Services;
using EasyTransition;
using LevelSelection.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace StartGame
{
    /// <summary>
    ///     VContainer lifetime scope for Start Game screen
    /// </summary>
    public class StartScreenLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[LevelSelectionLifetimeScope] Configuring level selection DI container...");

   
            // Register SceneLoadService as a component in hierarchy
            builder.RegisterComponentInHierarchy<SceneLoadService>().As<ISceneLoadService>();

            // Register start game specific components
            builder.RegisterComponentInHierarchy<StartGameListener>();

            Debug.Log("[StartScreenLifetimeScope] Start screen DI container configured successfully.");
        }
    }
}

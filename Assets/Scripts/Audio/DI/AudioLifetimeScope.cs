using Audio.Interfaces;
using Audio.Services;
using LevelSelection;
using Player.Components;
using Player.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Audio.DI
{
    /// <summary>
    ///     Dependency injection configuration for audio system
    ///     Follows Dependency Inversion Principle by registering interfaces
    ///     Registers ALL classes that need audio injection
    ///     AudioService is now a proper singleton managed by VContainer
    /// </summary>
    public class AudioLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<AudioService>().As<IAudioService>();

            builder.RegisterComponentInHierarchy<PlayerSoundController>();

            builder.RegisterComponentInHierarchy<HealthBonusService>();

            builder.RegisterComponentInHierarchy<LevelSelectionController>();

            builder.RegisterComponentInHierarchy<EndLevelZone>();
        }

   
    }
}

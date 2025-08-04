using Audio.Interfaces;
using Audio.Services;
using LevelSelection;
using Player.Components;
using Player.Services;
using VContainer;
using VContainer.Unity;

namespace Audio.DI
{
    /// <summary>
    ///     Dependency injection configuration for audio system
    ///     Follows Dependency Inversion Principle by registering interfaces
    ///     Registers ALL classes that need audio injection
    /// </summary>
    public class AudioLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<AudioService>()
                .As<IAudioService>()
                .AsSelf();

            RegisterAudioDependentComponents(builder);
        }

        private static void RegisterAudioDependentComponents(IContainerBuilder builder)
        {
            // Register Player Components that need audio
            builder.RegisterComponentInHierarchy<PlayerSoundController>();

            // Register Player Services that need audio
            builder.RegisterComponentInHierarchy<HealthBonusService>();

            // Register Level Selection Controller that needs audio
            builder.RegisterComponentInHierarchy<LevelSelectionController>();
        }
    }
}

using GabrielBigardi.SpriteAnimator;

namespace ModularCharacterController.Core.Abilities.Interfaces
{
    /// <summary>
    ///     Interface for abilities that provide animation data
    /// </summary>
    public interface IAnimatedAbilityModule : IAbilityModule
    {
        /// <summary>
        ///     The animation set for this ability
        /// </summary>
        SpriteAnimationObject AnimationSet { get; }

        /// <summary>
        ///     Play the appropriate animation for the current state
        /// </summary>
        /// <param name="animator">The sprite animator component</param>
        /// <param name="state">The current animation state</param>
        void PlayAnimation(SpriteAnimator animator, AnimationState state);

        /// <summary>
        ///     Get the animation name for a specific state
        /// </summary>
        /// <param name="state">The animation state</param>
        /// <returns>The name of the animation to play</returns>
        string GetAnimationName(AnimationState state);
    }

    /// <summary>
    ///     States for animations
    /// </summary>
    public enum AnimationState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Float,
        FlySustained,
        FlyEnd,
        Crouch,
        Slide,
        Inhale,
        Full,
        Spit,
        Swallow,
        Attack,
        SecondaryAttack,
        Special,
        Hurt,
        SlopeL,
        SlopeR,
        DeepSlopeL,
        DeepSlopeR
    }
}

using System.Collections.Generic;

namespace ModularCharacterController.Core.Abilities.Interfaces
{
    /// <summary>
    ///     Interface for abilities that provide special/unique functionality
    /// </summary>
    public interface ISpecialAbilityModule : IAbilityModule
    {
        /// <summary>
        ///     Whether this ability has a special action available
        /// </summary>
        bool HasSpecialAction { get; }

        /// <summary>
        ///     The types of special ability
        /// </summary>
        SpecialAbilityType SpecialType { get; }

        /// <summary>
        ///     Duration of the special ability effect in seconds (0 = permanent)
        /// </summary>
        float SpecialDuration { get; }

        /// <summary>
        ///     Perform the special action
        /// </summary>
        /// <returns>True if the special action was successfully performed</returns>
        bool PerformSpecialAction();

        /// <summary>
        ///     Get custom parameters specific to this special ability
        /// </summary>
        /// <returns>Dictionary of parameter name to value</returns>
        Dictionary<string, object> GetSpecialParameters();
    }

    /// <summary>
    ///     Types of special abilities
    /// </summary>
    public enum SpecialAbilityType
    {
        Transformation,
        AreaEffect,
        EnvironmentalInteraction,
        StatusEffect,
        Custom
    }
}

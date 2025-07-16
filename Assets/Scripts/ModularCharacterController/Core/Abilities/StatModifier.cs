using System;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities
{
    /// <summary>
    ///     Defines a modification to a stat - used across abilities and stats.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public enum ModType
        {
            Additive, // Add to the base value
            Multiplicative, // Multiply the base value
            Override // Completely override the value
        }

        [SerializeField] private StatType statType;
        [SerializeField] private ModType modificationType;
        [SerializeField] private float value;

        private StatModifier()
        {
        }

        private StatModifier(StatType statType, float value, ModType modificationType)
        {
            this.statType = statType;
            this.value = value;
            this.modificationType = modificationType;
        }

        public float Value => value;
        public ModType ModificationType => modificationType;
        public StatType StatType => statType;

        /// <summary>
        ///     Factory method to create a new StatModifier.
        /// </summary>
        public static StatModifier Create(StatType statType, float value, ModType modificationType) =>
            new(statType, value, modificationType);

        /// <summary>
        ///     Applies a modification to a base value.
        /// </summary>
        public static float ApplyModifier(float baseValue, float modifierValue, ModType modificationType)
        {
            return modificationType switch
            {
                ModType.Additive => baseValue + modifierValue,
                ModType.Multiplicative => baseValue * modifierValue,
                ModType.Override => modifierValue,
                _ => baseValue
            };
        }

        /// <summary>
        ///     Combines multiple modifiers into a single value.
        /// </summary>
        public static float CombineModifiers(float baseValue, params StatModifier[] modifiers)
        {
            float result = baseValue;
            foreach (StatModifier modifier in modifiers)
            {
                result = ApplyModifier(result, modifier.Value, modifier.ModificationType);
            }

            return result;
        }

        /// <summary>
        ///     Validates the modifier to ensure it has valid values.
        /// </summary>
        public void Validate()
        {
            if (Value < 0 && ModificationType != ModType.Override)
            {
                Debug.LogWarning("Modifier value should not be negative unless it overrides the base value.");
            }
        }
    }
}

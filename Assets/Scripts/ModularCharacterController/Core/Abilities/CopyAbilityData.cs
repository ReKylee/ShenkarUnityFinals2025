using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities
{
    /// <summary>
    ///     Data for a Modular Character Controller copy ability
    /// </summary>
    [CreateAssetMenu(fileName = "NewCopyAbility", menuName = "ModularCharacterController/Copy Ability")]
    public class CopyAbilityData : ScriptableObject
    {
        public enum AbilityAddResult
        {
            Success,
            DuplicateNotAllowed,
            InvalidAbility
        }

        public enum AbilityType
        {
            Melee,
            Projectile,
            Transformation,
            Utility,
            Special
        }

        [Header("Contained Abilities")] public List<AbilityModuleBase> abilities = new();

        [Header("Basic Info")] public string abilityName;

        public Sprite abilityIcon;

        [Header("Ability Capabilities")] [Header("Stat Modifiers")] [SerializeField]
        private List<StatModifier> statModifiers = new();

        [Header("Ability Specifics")] public AbilityType abilityType = AbilityType.Melee;


        private void OnEnable()
        {
            if (string.IsNullOrEmpty(abilityName))
            {
                abilityName = name;
            }
        }

        /// <summary>
        ///     Checks if an ability module of the given types can be added
        /// </summary>
        /// <param name="addedAbilityType">The types of ability module to check</param>
        /// <returns>Result indicating if addition is allowed and why if not</returns>
        public AbilityAddResult CanAddAbilityModule(Type addedAbilityType)
        {
            // Quick validation check
            if (addedAbilityType == null || !typeof(AbilityModuleBase).IsAssignableFrom(addedAbilityType))
            {
                return AbilityAddResult.InvalidAbility;
            }

            return abilities.Any(module =>
                module?.GetType() == addedAbilityType &&
                !module.AllowMultipleInstances)
                ? AbilityAddResult.DuplicateNotAllowed
                : AbilityAddResult.Success;
        }

        /// <summary>
        ///     Returns the existing module that prevents adding a new one of the given types
        /// </summary>
        /// <param name="abilityType">The types of ability module to check</param>
        /// <returns>The existing module that prevents adding a new one, or null if no conflict</returns>
        public AbilityModuleBase GetConflictingModule(Type abilityType)
        {
            if (abilityType == null || !typeof(AbilityModuleBase).IsAssignableFrom(abilityType))
            {
                return null;
            }

            return abilities.FirstOrDefault(module =>
                module?.GetType() == abilityType &&
                !module.AllowMultipleInstances);
        }

        /// <summary>
        ///     Attempts to add a new ability module instance to this ability
        /// </summary>
        /// <param name="module">The ability module to add</param>
        /// <returns>Result of the add attempt</returns>
        public AbilityAddResult AddAbilityModule(AbilityModuleBase module)
        {
            if (!module)
            {
                return AbilityAddResult.InvalidAbility;
            }

            // Check if an ability of this types already exists
            AbilityAddResult result = CanAddAbilityModule(module.GetType());
            if (result != AbilityAddResult.Success)
            {
                return result;
            }

            // Add the module
            abilities.Add(module);
            return AbilityAddResult.Success;
        }

        /// <summary>
        ///     Apply all modifiers to the provided stats
        /// </summary>
        public MccStats ApplyModifiers(MccStats baseStats)
        {
            // Create a copy of the base stats
            MccStats modifiedStats = Instantiate(baseStats);

            // Combine all stat modifiers and apply them
            foreach (object statType in Enum.GetValues(typeof(StatType)))
            {
                var modifiersForStat = statModifiers.Where(m => m.StatType == (StatType)statType).ToArray();
                if (modifiersForStat.Length > 0)
                {
                    float baseValue = modifiedStats.GetStat((StatType)statType);
                    float combinedValue = StatModifier.CombineModifiers(baseValue, modifiersForStat);
                    modifiedStats.SetStat((StatType)statType, combinedValue);
                }
            }

            return modifiedStats;
        }
    }

}

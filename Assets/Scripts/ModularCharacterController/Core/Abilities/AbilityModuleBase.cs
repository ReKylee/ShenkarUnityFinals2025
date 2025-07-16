using System.Collections.Generic;
using System.Linq;
using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities
{
    /// <summary>
    ///     Base class for all ability ScriptableObjects
    /// </summary>
    public abstract class AbilityModuleBase : ScriptableObject, IAbilityModule
    {

        [SerializeField] private List<StatModifier> abilityDefinedModifiers = new();

        [Header("Module Settings")]
        [Tooltip("If false, CopyAbilityData cannot contain multiple instances of this module types")]
        [SerializeField]
        private bool allowMultipleInstances;

        [Header("Basic Information")] [SerializeField]
        private string abilityID = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        // Reference to the controller using this ability
        protected Components.ModularCharacterController Controller { get; private set; }
        protected Rigidbody2D Rigidbody { get; private set; }

        /// <summary>
        ///     Gets or sets whether multiple instances of this module types can be added to a CopyAbilityData
        /// </summary>
        public bool AllowMultipleInstances
        {
            get => allowMultipleInstances;
            set => allowMultipleInstances = value;
        }


        /// <summary>
        ///     Gets or sets the ability's unique identifier
        /// </summary>
        public string AbilityID
        {
            get => abilityID;
            set => abilityID = value;
        }

        /// <summary>
        ///     Gets or sets the ability's display name
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        /// <summary>
        ///     Initialize the ability with controller reference
        /// </summary>
        public virtual void Initialize(Components.ModularCharacterController controller)
        {
            Controller = controller;
            Rigidbody = controller.Rigidbody;
        }

        /// <summary>
        ///     Called when the ability becomes active
        /// </summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>
        ///     Called when the ability becomes inactive
        /// </summary>
        public virtual void OnDeactivate()
        {
        }

        /// <summary>
        ///     Update logic for the ability
        /// </summary>
        public virtual void ProcessAbility(InputContext inputContext)
        {
        }

        /// <summary>
        ///     Applies the stat modifiers defined directly on this ability's ScriptableObject.
        /// </summary>
        /// <param name="stats">The KirbyStats object to modify.</param>
        public void ApplyAbilityDefinedModifiers(MccStats stats)
        {
            // Group modifiers by StatType and only process stats that have modifiers
            foreach (var group in abilityDefinedModifiers.GroupBy(m => m.StatType))
            {
                StatType statType = group.Key;
                var modifiersForStat = group.ToArray();

                float baseValue = stats.GetStat(statType);
                float combinedValue = StatModifier.CombineModifiers(baseValue, modifiersForStat);
                stats.SetStat(statType, combinedValue);
            }
        }

        /// <summary>
        ///     Explicitly initializes any relevant fields with default values.
        /// </summary>
        public void InitializeNameAndIDWithDefaultValue()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
            }

            if (string.IsNullOrEmpty(abilityID))
            {
                abilityID = name.ToLowerInvariant().Replace(" ", "_");
            }
        }
    }
}

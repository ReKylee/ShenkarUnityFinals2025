using System.Collections.Generic;
using System.Linq;
using ModularCharacterController.Core.Abilities;
using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEngine;

namespace ModularCharacterController.Core.Components
{
    public class ModularCharacterController : MonoBehaviour
    {

        [Header("Core Stats & Abilities")] [SerializeField]
        private MccStats baseStats;

        [SerializeField] private CopyAbilityData currentCopyAbility;
        private readonly List<IAbilityModule> _activeAbilities = new();
        private readonly List<IMovementAbilityModule> _movementAbilities = new();

        private InputContext _fixedInput;

        private MccGroundCheck _groundCheck;

        private InputHandler _inputHandler;
        internal Collider2D Collider;

        internal Rigidbody2D Rigidbody;

        public InputContext CurrentInput { get; private set; }

        public MccStats Stats { get; private set; }

        // public bool IsGrounded => _groundCheck?.IsGrounded ?? false;
        private bool IsGrounded => _groundCheck.IsGrounded;

        public MccGroundCheck.SlopeType GroundType =>
            _groundCheck?.CurrentSlope ?? MccGroundCheck.SlopeType.None;

        public Vector2 GroundNormal => _groundCheck?.GroundNormal ?? Vector2.zero;
        public Vector2 Velocity => Rigidbody?.linearVelocity ?? Vector2.zero;
        public LayerMask GroundLayers => _groundCheck?.groundLayers ?? 0;
        private void Awake()
        {
            _groundCheck = GetComponent<MccGroundCheck>();
            Rigidbody = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();
            _inputHandler = GetComponent<InputHandler>();


            if (!_inputHandler)
            {
                Debug.LogError(
                    "InputHandler not assigned and not found on the same GameObject. Please assign it in the ModularCharacterController Inspector.");

                enabled = false;
                return;
            }

            // Initial stat setup. EquipAbility will call RefreshRuntimeStats.
            // Ensure Stats is initialized before EquipAbility if baseStats is available.
            if (baseStats)
            {
                Stats = Instantiate(baseStats); // Initial copy
            }
            else
            {
                Debug.LogError(
                    "ModularCharacterController: baseStats is not assigned in the Inspector. Creating a default KirbyStats instance.");

                Stats = ScriptableObject.CreateInstance<MccStats>(); // Fallback
            }

            EquipAbility(currentCopyAbility);
        }

        private void Update()
        {
            if (!_inputHandler) return;

            RefreshRuntimeStats();

            CurrentInput = _inputHandler.CurrentInput;


            // Process all non-movement abilities in Update
            foreach (IAbilityModule ability in _activeAbilities.Where(a => a is not IMovementAbilityModule))
            {
                ability.ProcessAbility(CurrentInput);
            }


        }
        private void FixedUpdate()
        {
            if (!_inputHandler) return;

            _fixedInput = _inputHandler.FixedInput;


            // Use Aggregate to apply movement abilities sequentially
            // Each ability gets the current velocity and returns the modified velocity
            Rigidbody.linearVelocity = _movementAbilities.Aggregate(
                Rigidbody.linearVelocity,
                (current, movementAbility) =>
                    movementAbility.ProcessMovement(current, IsGrounded, _fixedInput));

        }


        public void EquipAbility(CopyAbilityData newAbilityData)
        {
            // Deactivate and clear previous abilities
            foreach (IAbilityModule ability in _activeAbilities)
            {
                ability.OnDeactivate();
            }

            _activeAbilities.Clear();
            _movementAbilities.Clear();

            currentCopyAbility = newAbilityData;


            // Stats are refreshed by RefreshRuntimeStats below
            if (currentCopyAbility)
            {
                // Initialize and categorize abilities from CopyAbilityData
                foreach (AbilityModuleBase abilitySo in currentCopyAbility.abilities)
                {
                    if (abilitySo is IAbilityModule abilityInstance)
                    {
                        abilityInstance.Initialize(this);
                        _activeAbilities.Add(abilityInstance);
                        if (abilityInstance is IMovementAbilityModule movementAbility)
                        {
                            _movementAbilities.Add(movementAbility);
                        }

                        abilityInstance.OnActivate();
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"AbilityModule '{abilitySo.name}' does not implement IAbilityModule and won't be activated.",
                            this);
                    }
                }
            }

            RefreshRuntimeStats();
        }

        /// <summary>
        ///     Refreshes Stats based on baseStats and any active copy ability.
        ///     This allows live updates if baseStats asset is changed in the inspector.
        /// </summary>
        private void RefreshRuntimeStats()
        {
            if (!baseStats)
            {
                if (!Stats) // Ensure Stats is not null
                {
                    Stats = ScriptableObject.CreateInstance<MccStats>();
                }

                // ApplyStatsToComponents might be needed here if there's a default state for components
                ApplyStatsToComponents();
                return;
            }

            if (currentCopyAbility)
            {
                // ApplyModifiers from CopyAbilityData should return a NEW INSTANCE based on baseStats
                Stats = currentCopyAbility.ApplyModifiers(baseStats);

                // Apply modifiers defined directly on the AbilityModuleBase ScriptableObjects
                // These are applied to the Stats instance that already has CopyAbilityData modifiers
                foreach (AbilityModuleBase abilitySo in currentCopyAbility.abilities)
                {
                    abilitySo.ApplyAbilityDefinedModifiers(Stats);
                }
            }
            else
            {
                // No ability, Stats is a direct instance/clone of baseStats
                Stats = Instantiate(baseStats);
            }

            // Apply stats that directly affect components
            ApplyStatsToComponents();
        }


        /// <summary>
        ///     Applies stats that directly affect components like Rigidbody
        /// </summary>
        private void ApplyStatsToComponents()
        {
            if (Stats is null) return; // Changed from Stats to Stats
            if (Rigidbody)
            {
                Rigidbody.gravityScale = Stats.gravityScale; // Changed from Stats to Stats
            }
        }
    }
}

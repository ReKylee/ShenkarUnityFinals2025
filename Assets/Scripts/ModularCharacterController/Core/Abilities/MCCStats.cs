using System;
using System.Collections.Generic;
using System.Reflection;
using ModularCharacterController.Core.Abilities;
using UnityEngine;

namespace ModularCharacterController.Core.Abilities
{
    /// <summary>
    ///     Attribute to mark a stat field in MCCStats and associate it with a StatType
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ModularCharacterControllerStatAttribute : Attribute
    {
        public ModularCharacterControllerStatAttribute(StatType type, string category)
        {
            Type = type;
            Category = category;
        }
        public StatType Type { get; }
        public string Category { get; }
    }

    /// <summary>
    ///     Simple stat value container - used for original values and modified values
    /// </summary>
    [CreateAssetMenu(fileName = "NewModularCharacterControllerStats", menuName = "ModularCharacterController/Stats")]
    public class MccStats : ScriptableObject
    {
        private static readonly Dictionary<StatType, (FieldInfo field, string category)> StatInfoCache;

        [Header("Movement Settings")] [ModularCharacterControllerStat(StatType.WalkSpeed, "Movement")]
        public float walkSpeed = 4.0f;

        [ModularCharacterControllerStat(StatType.RunSpeed, "Movement")]
        public float runSpeed = 6f;

        [ModularCharacterControllerStat(StatType.GroundAcceleration, "Movement")]
        public float groundAcceleration = 50f;

        [ModularCharacterControllerStat(StatType.GroundDeceleration, "Movement")]
        public float groundDeceleration = 70f;

        [ModularCharacterControllerStat(StatType.AirAcceleration, "Movement")]
        public float airAcceleration = 25f;

        [ModularCharacterControllerStat(StatType.AirDeceleration, "Movement")]
        public float airDeceleration = 10f;

        [Header("Jump Settings")] [ModularCharacterControllerStat(StatType.JumpVelocity, "Jump")]
        public float jumpVelocity = 14f;

        [ModularCharacterControllerStat(StatType.JumpReleaseVelocityMultiplier, "Jump")]
        public float jumpReleaseVelocityMultiplier = 0.5f;

        [ModularCharacterControllerStat(StatType.MaxFallSpeed, "Jump")]
        public float maxFallSpeed = 15f;

        [ModularCharacterControllerStat(StatType.CoyoteTime, "Jump")]
        public float coyoteTime = 0.08f;

        [ModularCharacterControllerStat(StatType.JumpBufferTime, "Jump")]
        public float jumpBufferTime = 0.1f;

        [Header("Fly Settings")] [ModularCharacterControllerStat(StatType.FlapImpulse, "Float")]
        public float flapImpulse = 5.5f;

        [ModularCharacterControllerStat(StatType.FloatDescentSpeed, "Float")]
        public float floatDescentSpeed = 1.0f;


        [Header("Physics")] [ModularCharacterControllerStat(StatType.GravityScale, "Physics")]
        public float gravityScale = 2.8f;

        [Header("Combat")] [ModularCharacterControllerStat(StatType.AttackDamage, "Combat")]
        public float attackDamage = 10f;

        [ModularCharacterControllerStat(StatType.AttackRange, "Combat")]
        public float attackRange = 0.5f;

        [ModularCharacterControllerStat(StatType.AttackSpeed, "Combat")]
        public float attackSpeed = 1.0f;

        [Header("Other")] [ModularCharacterControllerStat(StatType.InhaleRange, "Other")]
        public float inhaleRange = 2.5f;

        [ModularCharacterControllerStat(StatType.InhalePower, "Other")]
        public float inhalePower = 5f;

        // Initialize the reflection cache
        static MccStats()
        {
            StatInfoCache = new Dictionary<StatType, (FieldInfo field, string category)>();

            // Get all fields with KirbyStatAttribute
            var fields = typeof(MccStats).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                ModularCharacterControllerStatAttribute attribute = field.GetCustomAttribute<ModularCharacterControllerStatAttribute>();
                if (attribute != null)
                {
                    StatInfoCache[attribute.Type] = (field, attribute.Category);
                }
            }
        }

        /// <summary>
        ///     Get a stat value by its enum types
        /// </summary>
        public float GetStat(StatType statType)
        {
            (FieldInfo field, _) = GetStatInfo(statType);
            return field != null ? (float)field.GetValue(this) : 1.0f;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        ///     Set a stat value by its enum types
        /// </summary>
        public void SetStat(StatType statType, float value)
        {
            (FieldInfo field, _) = GetStatInfo(statType);
            if (field != null)
            {
                field.SetValue(this, value);
            }
            else
            {
                Debug.LogWarning($"Trying to set unknown stat: {statType}");
            }
        }

        private static (FieldInfo field, string category) GetStatInfo(StatType statType) =>
            StatInfoCache.GetValueOrDefault(statType, (null, "Other"));

        public static string GetStatCategory(StatType statType) => GetStatInfo(statType).category;
    }
}

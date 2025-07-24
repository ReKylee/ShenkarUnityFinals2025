using System;

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
}
using System;
using System.Collections.Generic;
using System.Linq;
using ModularCharacterController.Core.Abilities;
using UnityEditor;
using UnityEngine;

namespace ModularCharacterController.Editor
{
#if UNITY_EDITOR
    /// <summary>
    ///     Shared editor utility for StatModifier related functionalities.
    /// </summary>
    public static class StatModifierEditorUtility // Made static as it now only contains static members
    {
        // Static helper methods remain the same
        public static List<StatType> GetAllStatTypes() => Enum.GetValues(typeof(StatType)).Cast<StatType>().ToList();

        public static string[] GetAllCategories() =>
            Enum.GetValues(typeof(StatType))
                .Cast<StatType>()
                .Select(MccStats.GetStatCategory)
                .Distinct()
                .ToArray();

        public static string[] GetAllCategoriesPlusAll()
        {
            var categories = new List<string> { "All Categories" }; // For filtering UI
            categories.AddRange(GetAllCategories());
            return categories.ToArray();
        }

        /// <summary>
        ///     Shows a categorized GenericMenu for selecting a StatType and applies it to the given property.
        /// </summary>
        /// <param name="statTypeProperty">The SerializedProperty for the StatType to change.</param>
        /// <param name="filterOutStats">Optional list of StatTypes to exclude from the menu.</param>
        public static void ShowStatTypeSelectionMenu(SerializedProperty statTypeProperty,
            List<StatType> filterOutStats = null)
        {
            GenericMenu menu = new();
            var availableStats = GetAllStatTypes();

            if (filterOutStats != null)
            {
                availableStats = availableStats.Except(filterOutStats).ToList();
            }

            if (availableStats.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No available stats"));
            }
            else
            {
                var statsByCategory = availableStats
                    .GroupBy(MccStats.GetStatCategory)
                    .OrderBy(g => g.Key);

                foreach (var group in statsByCategory)
                {
                    foreach (StatType stat in group.OrderBy(s => s.ToString()))
                    {
                        string statName = ObjectNames.NicifyVariableName(stat.ToString());
                        bool isCurrentlySelected = statTypeProperty.enumValueIndex == (int)stat;

                        menu.AddItem(new GUIContent($"{group.Key}/{statName}"), isCurrentlySelected, () =>
                        {
                            statTypeProperty.enumValueIndex = (int)stat;
                            statTypeProperty.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }
            }

            menu.ShowAsContext();
        }
    }
#endif
}

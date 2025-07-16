#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using ModularCharacterController.Core.Abilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ModularCharacterController.Editor
{
    /// <summary>
    ///     A helper class that creates a reorderable list for StatModifier collections,
    ///     providing a cleaner UI without "Element X" foldouts
    /// </summary>
    public class ReorderableModifierList
    {
        private readonly float _elementHeight = EditorGUIUtility.singleLineHeight + 4;
        private readonly ReorderableList _list;
        private readonly SerializedProperty _listProperty;
        private readonly SerializedObject _serializedObject;
        private readonly bool _showAddButton;
        private readonly bool _showHeader;
        private GUIStyle _headerStyle; // Removed readonly to allow lazy initialization

        public ReorderableModifierList(SerializedObject serializedObject, SerializedProperty property,
            bool showHeader = true, bool showAddButton = true)
        {
            _serializedObject = serializedObject;
            _listProperty = property;
            _showHeader = showHeader;
            _showAddButton = showAddButton;

            // Initialize list without the header style
            _list = new ReorderableList(serializedObject, property, true, showHeader, showAddButton, true)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
                elementHeightCallback = ElementHeightCallback,
                onAddCallback = OnAddCallback,
                drawNoneElementCallback = DrawEmptyListCallback,
                footerHeight = 20f,
                headerHeight = 20f,
                elementHeight = _elementHeight,
                drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
                {
                    // Draw a subtle separator line between elements
                    if (Event.current.type == EventType.Repaint)
                    {
                        Color lineColor = new(0.5f, 0.5f, 0.5f, 0.2f);
                        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), lineColor);

                        // Selected element background
                        if (isActive || isFocused)
                        {
                            Color selectedColor = new(0.3f, 0.5f, 0.7f, 0.2f);
                            EditorGUI.DrawRect(rect, selectedColor);
                        }
                    }
                }
            };
        }

        private void DrawHeaderCallback(Rect rect)
        {
            if (!_showHeader) return;

            // Create header style lazily when needed (ensuring EditorStyles is initialized)
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                    fontSize = 11
                };
            }

            // Calculate column widths
            float statWidth = rect.width * 0.4f;
            float typeWidth = rect.width * 0.3f;
            float valueWidth = rect.width * 0.3f - 24; // Account for remove button

            // Draw column headers
            EditorGUI.LabelField(new Rect(rect.x + 8, rect.y, statWidth, rect.height), "Stat", _headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + statWidth + 4, rect.y, typeWidth, rect.height), "Types",
                _headerStyle);

            EditorGUI.LabelField(new Rect(rect.x + statWidth + typeWidth + 4, rect.y, valueWidth, rect.height), "Value",
                _headerStyle);

        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _listProperty.GetArrayElementAtIndex(index);

            // Adjust rect for element drawing with proper vertical alignment
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            // Get properties
            SerializedProperty statTypeProp = element.FindPropertyRelative("statType");
            SerializedProperty modTypeProp = element.FindPropertyRelative("modificationType");
            SerializedProperty valueProp = element.FindPropertyRelative("value");

            // Calculate column widths
            float statWidth = rect.width * 0.4f;
            float typeWidth = rect.width * 0.3f;
            float valueWidth = rect.width * 0.3f - 32; // Account for remove button and spacing

            // Create rects for each field with proper spacing
            Rect statRect = new(rect.x + 4, rect.y, statWidth - 8, rect.height);
            Rect typeRect = new(rect.x + statWidth, rect.y, typeWidth - 4, rect.height);
            Rect valueRect = new(rect.x + statWidth + typeWidth, rect.y, valueWidth, rect.height);

            // Draw stat types field with popup button
            string currentStatName = "Select Stat";
            if (statTypeProp.enumValueIndex >= 0 && statTypeProp.enumValueIndex < statTypeProp.enumDisplayNames.Length)
            {
                currentStatName =
                    ObjectNames.NicifyVariableName(statTypeProp.enumDisplayNames[statTypeProp.enumValueIndex]);
            }

            // Custom button style to match dropdown appearance
            GUIStyle popupStyle = new(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = EditorGUIUtility.singleLineHeight
            };

            if (GUI.Button(statRect, currentStatName, popupStyle))
            {
                // Get list of StatTypes already used in this list, excluding the current element
                var usedStatTypesInList = new List<StatType>();
                for (int i = 0; i < _listProperty.arraySize; i++)
                {
                    if (i == index) continue; // Skip the current element itself
                    usedStatTypesInList.Add((StatType)_listProperty.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("statType").enumValueIndex);
                }

                // Use the centralized utility method
                StatModifierEditorUtility.ShowStatTypeSelectionMenu(statTypeProp, usedStatTypesInList);
            }

            // Draw modification types dropdown
            EditorGUI.PropertyField(typeRect, modTypeProp, GUIContent.none);

            // Draw value field
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
        }

        private float ElementHeightCallback(int index) => _elementHeight;

        private void DrawEmptyListCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "No stat modifiers. Click + to add one.", EditorStyles.centeredGreyMiniLabel);
        }

        private void OnAddCallback(ReorderableList list)
        {
            // Get list of all StatTypes currently used in the list to filter them out
            var existingStatTypesInList = new List<StatType>();
            for (int i = 0; i < _listProperty.arraySize; i++)
            {
                existingStatTypesInList.Add((StatType)_listProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("statType").enumValueIndex);
            }

            // Show the stat selection menu, filtering out already used stats.
            var availableStats = StatModifierEditorUtility.GetAllStatTypes().Except(existingStatTypesInList).ToList();

            if (availableStats.Count == 0)
            {
                EditorUtility.DisplayDialog("No Stats Available",
                    "All available stat types are already in use in this list.", "OK");

                return;
            }

            GenericMenu menu = new();

            var statsByCategory = availableStats
                .GroupBy(MccStats.GetStatCategory)
                .OrderBy(g => g.Key);

            foreach (var group in statsByCategory)
            {
                foreach (StatType stat in group.OrderBy(s => s.ToString()))
                {
                    string statName = ObjectNames.NicifyVariableName(stat.ToString());
                    menu.AddItem(new GUIContent($"{group.Key}/{statName}"), false, () =>
                    {
                        // Add the new element to the list with the selected stat
                        int newIndex = _listProperty.arraySize;
                        _listProperty.InsertArrayElementAtIndex(newIndex);
                        SerializedProperty newElement = _listProperty.GetArrayElementAtIndex(newIndex);

                        newElement.FindPropertyRelative("statType").enumValueIndex = (int)stat;
                        newElement.FindPropertyRelative("modificationType").enumValueIndex =
                            (int)StatModifier.ModType.Multiplicative;

                        newElement.FindPropertyRelative("value").floatValue = 1f;

                        _serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            menu.ShowAsContext();
        }

        public bool DoLayoutList()
        {
            _serializedObject.Update();

            // Draw the list directly - using the cached properties where possible
            _list.DoLayoutList();

            // Apply changes only once at the end
            return _serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

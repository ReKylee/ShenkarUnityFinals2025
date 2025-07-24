#if UNITY_EDITOR
using ModularCharacterController.Core.Abilities;
using UnityEditor;
using UnityEngine;

namespace ModularCharacterController.Editor
{
    [CustomEditor(typeof(AbilityModuleBase), true)]
    public class AbilityModuleBaseEditor : UnityEditor.Editor
    {
        // Cache GUI styles as fields and initialize them lazily
        private SerializedProperty _abilityDefinedModifiersProperty;
        private GUIStyle _headerStyle;
        private ReorderableModifierList _modifierList;
        private GUIStyle _sectionBoxStyle;

        private void OnEnable()
        {
            _abilityDefinedModifiersProperty = serializedObject.FindProperty("abilityDefinedModifiers");
            _modifierList = new ReorderableModifierList(serializedObject, _abilityDefinedModifiersProperty);
        }

        public override void OnInspectorGUI()
        {
            // Lazy initialization of GUI styles - only create them once, not every frame
            if (_sectionBoxStyle == null)
            {
                _sectionBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    margin = new RectOffset(4, 4, 4, 4),
                    padding = new RectOffset(10, 10, 10, 10)
                };

                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 12
                };
            }

            // Update serialized object at the beginning
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // Set background color to ensure the helpBox is visible
            Color prevColor = GUI.backgroundColor;
            // Use a more contrasting color that will stand out in both light and dark themes
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.35f, 0.5f, 0.8f) // Blueish for dark theme
                : new Color(0.8f, 0.8f, 0.9f, 0.8f); // Light bluish for light theme

            EditorGUILayout.BeginVertical(_sectionBoxStyle);

            // Reset background color
            GUI.backgroundColor = prevColor;

            // Create a nice header for the base properties
            Rect basePropsHeaderRect = EditorGUILayout.GetControlRect(false, 28);
            EditorGUI.LabelField(basePropsHeaderRect, "Ability Properties", _headerStyle);

            EditorGUILayout.Space(8);

            // Use DrawPropertiesExcluding instead of the iterator loop
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script", "abilityDefinedModifiers");
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Use a rounded container for stat modifiers section
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.35f, 0.5f, 0.8f) // Blueish for dark theme
                : new Color(0.8f, 0.8f, 0.9f, 0.8f); // Light bluish for light theme

            EditorGUILayout.BeginVertical(_sectionBoxStyle);

            // Reset background color
            GUI.backgroundColor = prevColor;

            // Create a nice header for the stat modifiers
            Rect headerRect = EditorGUILayout.GetControlRect(false, 28);
            EditorGUI.LabelField(headerRect, "Ability Stat Modifiers", _headerStyle);

            EditorGUILayout.Space(8);

            if (_modifierList.DoLayoutList())
                EditorUtility.SetDirty(target);

            EditorGUILayout.EndVertical();
        }
    }
}
#endif

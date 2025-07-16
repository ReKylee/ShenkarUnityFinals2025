#if UNITY_EDITOR
using ModularCharacterController.Core.Abilities;
using UnityEditor;
using UnityEngine;

namespace ModularCharacterController.Editor
{
    [CustomPropertyDrawer(typeof(StatModifier))]
    public class StatModifierPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect contentPosition = position;
            if (label != null && label != GUIContent.none && !string.IsNullOrEmpty(label.text) &&
                !label.text.StartsWith("Element "))
            {
                contentPosition = EditorGUI.PrefixLabel(position, label);
            }

            float totalWidth = contentPosition.width;
            const float spacing = 5f;
            const int fieldCount = 3;
            float fieldWidth = (totalWidth - spacing * (fieldCount - 1)) / fieldCount;

            SerializedProperty statTypeProp = property.FindPropertyRelative("statType");
            SerializedProperty modTypeProp = property.FindPropertyRelative("modificationType");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            Rect statTypeButtonRect = new(contentPosition.x, contentPosition.y, fieldWidth, contentPosition.height);
            Rect modTypeRect = new(contentPosition.x + fieldWidth + spacing, contentPosition.y, fieldWidth,
                contentPosition.height);

            Rect valueRect = new(contentPosition.x + (fieldWidth + spacing) * 2, contentPosition.y, fieldWidth,
                contentPosition.height);

            string currentStatName = "Select Stat";
            if (statTypeProp.enumValueIndex >= 0 && statTypeProp.enumValueIndex < statTypeProp.enumDisplayNames.Length)
            {
                currentStatName =
                    ObjectNames.NicifyVariableName(statTypeProp.enumDisplayNames[statTypeProp.enumValueIndex]);
            }

            GUIStyle buttonStyle = new(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleLeft
            };

            if (GUI.Button(statTypeButtonRect, new GUIContent(currentStatName, "Click to select Stat Types"),
                    buttonStyle))
            {
                // Use the centralized utility method to show the stat selection menu
                StatModifierEditorUtility.ShowStatTypeSelectionMenu(statTypeProp);
            }

            EditorGUI.PropertyField(modTypeRect, modTypeProp, GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;
    }
}
#endif

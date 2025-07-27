using System;
using System.Collections.Generic;
using Health.Damage;
using UnityEditor;
using UnityEngine;

namespace Health.Editor
{
    [CustomPropertyDrawer(typeof(ConditionNode), true)]
    public class ConditionNodePropertyDrawer : PropertyDrawer
    {
        private static readonly Type[] NodeTypes =
            { typeof(ConditionLeaf), typeof(AndCondition), typeof(OrCondition), typeof(NotCondition) };

        private static readonly string[] NodeTypeNames = { "Leaf", "AND", "OR", "NOT" };
        private static readonly string[] NodeTypeSymbols = { "●", "∧", "∨", "¬" };

        // Modern color palette
        private static readonly Color CardColor = new(0.16f, 0.18f, 0.22f, 1f);
        private static readonly Color HeaderColor = new(0.22f, 0.26f, 0.34f, 1f);
        private static readonly Color AccentColor = new(0.36f, 0.78f, 0.93f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.96f, 1f, 1f);
        private static readonly Color BorderColor = new(0.36f, 0.78f, 0.93f, 0.2f);

        private static GUIStyle _cardStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _symbolStyle;

        private static GUIStyle CardStyle
        {
            get
            {
                if (_cardStyle == null)
                {
                    _cardStyle = new GUIStyle(GUI.skin.box)
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(14, 14, 14, 14),
                        border = new RectOffset(12, 12, 12, 12),
                        normal = { background = MakeTex(2, 2, CardColor) }
                    };
                }
                return _cardStyle;
            }
        }

        private static GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(GUI.skin.box)
                    {
                        padding = new RectOffset(10, 10, 6, 6),
                        normal = { background = MakeTex(2, 2, HeaderColor) },
                        alignment = TextAnchor.MiddleLeft,
                        fontStyle = FontStyle.Bold,
                        border = new RectOffset(8, 8, 8, 8)
                    };
                }
                return _headerStyle;
            }
        }

        private static GUIStyle SymbolStyle
        {
            get
            {
                if (_symbolStyle == null)
                {
                    _symbolStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 22,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = AccentColor }
                    };
                }
                return _symbolStyle;
            }
        }

        private static readonly GUIStyle AddButtonStyle = new(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = TextColor },
            alignment = TextAnchor.MiddleCenter
        };

        private static readonly GUIStyle DeleteButtonStyle = new(GUI.skin.button)
        {
            fixedWidth = 26,
            fixedHeight = 22,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            normal = { textColor = Color.red }
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.managedReferenceValue == null)
                return;

            EditorGUI.BeginProperty(position, label, property);
            float y = position.y;
            DrawNode(ref y, position.x, position.width, property, 0, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property == null || property.managedReferenceValue == null)
                return EditorGUIUtility.singleLineHeight + 8;

            // Use a stack to avoid recursion and reduce allocations
            float totalHeight = 0;
            var stack = new Stack<(SerializedProperty prop, int indent)>();
            stack.Push((property, 0));
            while (stack.Count > 0)
            {
                (SerializedProperty prop, int indent) = stack.Pop();
                string nodeType = prop.managedReferenceFullTypename;
                totalHeight += 32f; // headerHeight
                if (nodeType.Contains("ConditionLeaf"))
                {
                    SerializedProperty condProp = prop.FindPropertyRelative("conditionBehaviour");
                    totalHeight += EditorGUI.GetPropertyHeight(condProp, true) + 6;
                }
                else if (nodeType.Contains("AndCondition") || nodeType.Contains("OrCondition"))
                {
                    SerializedProperty childrenProp = prop.FindPropertyRelative("children");
                    for (int i = 0; i < childrenProp.arraySize; i++)
                    {
                        SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                        if (childProp.managedReferenceValue != null)
                            stack.Push((childProp, indent + 1));

                        totalHeight += 4;
                    }

                    totalHeight += EditorGUIUtility.singleLineHeight + 10;
                }
                else if (nodeType.Contains("NotCondition"))
                {
                    SerializedProperty childProp = prop.FindPropertyRelative("child");
                    if (childProp.managedReferenceValue == null)
                    {
                        totalHeight += EditorGUIUtility.singleLineHeight + 10;
                    }
                    else
                    {
                        stack.Push((childProp, indent + 1));
                        totalHeight += 4;
                    }
                }

                totalHeight += 6;
            }

            return totalHeight;
        }

        private void DrawNode(ref float y, float x, float width, SerializedProperty property, int indent,
            bool isRoot = false)
        {
            string nodeType = property.managedReferenceFullTypename;
            int typeIdx = Array.FindIndex(NodeTypes, t => nodeType != null && nodeType.Contains(t.Name));
            if (typeIdx < 0) typeIdx = 0;
            const float cardPad = 8f;
            const float headerHeight = 32f;
            const float symbolSize = 28f;
            const float spacing = 8f;
            float cardWidth = width - indent * 24 - cardPad * 2;
            float cardX = x + indent * 24 + cardPad;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            // Card background
            float cardHeight = GetNodeHeight(property, indent);
            GUI.Box(new Rect(cardX, y, cardWidth, cardHeight), GUIContent.none, CardStyle);
            // Header
            Rect headerRect = new(cardX, y, cardWidth, headerHeight);
            GUI.Box(headerRect, GUIContent.none, HeaderStyle);
            // Symbol
            Rect symbolRect = new(cardX + 6, y + 2, symbolSize, symbolSize);
            EditorGUI.LabelField(symbolRect, NodeTypeSymbols[typeIdx], SymbolStyle);
            // Type dropdown
            Rect dropdownRect = new(cardX + symbolSize + spacing, y + 6, 80, lineHeight);
            int newTypeIdx = EditorGUI.Popup(dropdownRect, typeIdx, NodeTypeNames);
            if (newTypeIdx != typeIdx)
            {
                object newNode = Activator.CreateInstance(NodeTypes[newTypeIdx]);
                // --- Preserve children/child when switching node types ---
                SerializedProperty oldChildren = property.FindPropertyRelative("children");
                SerializedProperty oldChild = property.FindPropertyRelative("child");
                // AND/OR <-> AND/OR: keep all children
                if ((NodeTypeNames[typeIdx] == "AND" || NodeTypeNames[typeIdx] == "OR") &&
                    (NodeTypeNames[newTypeIdx] == "AND" || NodeTypeNames[newTypeIdx] == "OR"))
                {
                    var list = new List<ConditionNode>();
                    if (oldChildren != null)
                    {
                        for (int i = 0; i < oldChildren.arraySize; i++)
                        {
                            var childProp = oldChildren.GetArrayElementAtIndex(i);
                            if (childProp.managedReferenceValue != null)
                                list.Add((ConditionNode)childProp.managedReferenceValue);
                        }
                    }
                    ((dynamic)newNode).children = list;
                }
                // AND/OR -> NOT: keep first child
                else if ((NodeTypeNames[newTypeIdx] == "NOT") && (NodeTypeNames[typeIdx] == "AND" || NodeTypeNames[typeIdx] == "OR"))
                {
                    ConditionNode first = null;
                    if (oldChildren != null)
                    {
                        for (int i = 0; i < oldChildren.arraySize; i++)
                        {
                            var childProp = oldChildren.GetArrayElementAtIndex(i);
                            if (childProp.managedReferenceValue != null)
                            {
                                first = (ConditionNode)childProp.managedReferenceValue;
                                break;
                            }
                        }
                    }
                    ((NotCondition)newNode).child = first ?? new ConditionLeaf();
                }
                // NOT -> AND/OR: wrap child in list
                else if ((NodeTypeNames[typeIdx] == "NOT") && (NodeTypeNames[newTypeIdx] == "AND" || NodeTypeNames[newTypeIdx] == "OR"))
                {
                    var list = new List<ConditionNode>();
                    if (oldChild != null && oldChild.managedReferenceValue != null)
                        list.Add((ConditionNode)oldChild.managedReferenceValue);
                    ((dynamic)newNode).children = list;
                }
                // NOT -> NOT: keep child
                else if (NodeTypeNames[newTypeIdx] == "NOT" && NodeTypeNames[typeIdx] == "NOT")
                {
                    ((NotCondition)newNode).child = (oldChild != null && oldChild.managedReferenceValue != null)
                        ? (ConditionNode)oldChild.managedReferenceValue
                        : new ConditionLeaf();
                }
                // LEAF: always new
                property.managedReferenceValue = newNode;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                GUI.FocusControl(null);
                return;
            }
            // Delete button (not for root)
            if (!isRoot)
            {
                Rect delRect = new(cardX + cardWidth - 32, y + 4, 26, 22);
                if (GUI.Button(delRect, "✕", DeleteButtonStyle))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                    GUI.FocusControl(null);
                    return;
                }
            }
            y += headerHeight;
            // Draw node content
            if (nodeType.Contains("ConditionLeaf"))
            {
                SerializedProperty condProp = property.FindPropertyRelative("conditionBehaviour");
                float condHeight = EditorGUI.GetPropertyHeight(condProp, true);
                EditorGUI.PropertyField(new Rect(cardX + 12, y, cardWidth - 24, condHeight), condProp,
                    new GUIContent("Condition Behaviour"), true);
                y += condHeight + 6;
            }
            else if (nodeType.Contains("AndCondition") || nodeType.Contains("OrCondition"))
            {
                SerializedProperty childrenProp = property.FindPropertyRelative("children");
                // Draw children
                for (int i = 0; i < childrenProp.arraySize; i++)
                {
                    SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                    if (childProp.managedReferenceValue == null) continue;
                    DrawNode(ref y, x, width, childProp, indent + 1);
                    y += 4;
                }
                // Add child button
                Rect addRect = new(cardX + 12, y, cardWidth - 24, lineHeight + 4);
                GUI.backgroundColor = AccentColor;
                if (GUI.Button(addRect, "+ Add Child Node", AddButtonStyle))
                {
                    childrenProp.InsertArrayElementAtIndex(childrenProp.arraySize);
                    SerializedProperty newChild = childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1);
                    newChild.managedReferenceValue = new ConditionLeaf();
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;
                y += lineHeight + 10;
            }
            else if (nodeType.Contains("NotCondition"))
            {
                SerializedProperty childProp = property.FindPropertyRelative("child");
                if (childProp.managedReferenceValue == null)
                {
                    Rect addRect = new(cardX + 12, y, cardWidth - 24, lineHeight + 4);
                    if (GUI.Button(addRect, "+ Add Child Node", AddButtonStyle))
                    {
                        childProp.managedReferenceValue = new ConditionLeaf();
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                        GUI.FocusControl(null);
                    }
                    y += lineHeight + 10;
                }
                else
                {
                    DrawNode(ref y, x, width, childProp, indent + 1);
                    y += 4;
                }
            }
            y += 6;
        }

        // Optimized, non-recursive node height calculation
        private float GetNodeHeight(SerializedProperty property, int indent)
        {
            float totalHeight = 0;
            var stack = new Stack<(SerializedProperty prop, int indent)>();
            stack.Push((property, indent));
            while (stack.Count > 0)
            {
                (SerializedProperty prop, _) = stack.Pop();
                string nodeType = prop.managedReferenceFullTypename;
                totalHeight += 32f; // headerHeight
                if (nodeType.Contains("ConditionLeaf"))
                {
                    SerializedProperty condProp = prop.FindPropertyRelative("conditionBehaviour");
                    totalHeight += EditorGUI.GetPropertyHeight(condProp, true) + 6;
                }
                else if (nodeType.Contains("AndCondition") || nodeType.Contains("OrCondition"))
                {
                    SerializedProperty childrenProp = prop.FindPropertyRelative("children");
                    for (int i = 0; i < childrenProp.arraySize; i++)
                    {
                        SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                        if (childProp.managedReferenceValue != null)
                            stack.Push((childProp, indent + 1));

                        totalHeight += 4;
                    }

                    totalHeight += EditorGUIUtility.singleLineHeight + 10;
                }
                else if (nodeType.Contains("NotCondition"))
                {
                    SerializedProperty childProp = prop.FindPropertyRelative("child");
                    if (childProp.managedReferenceValue == null)
                    {
                        totalHeight += EditorGUIUtility.singleLineHeight + 10;
                    }
                    else
                    {
                        stack.Push((childProp, indent + 1));
                        totalHeight += 4;
                    }
                }

                totalHeight += 6;
            }

            return totalHeight;
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;

            Texture2D result = new(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}

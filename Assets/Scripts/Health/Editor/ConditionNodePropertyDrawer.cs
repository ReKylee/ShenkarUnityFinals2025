using System;
using System.Collections.Generic;
using Health.Damage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Health.Editor
{
    [CustomPropertyDrawer(typeof(ConditionNode), true)]
    public class ConditionNodePropertyDrawer : PropertyDrawer
    {

        // Constants for better performance and consistency
        private const float CardPadding = 12f;
        private const float HeaderHeight = 36f;
        private const float SymbolSize = 24f;
        private const float Spacing = 12f;
        private const float IndentSize = 28f;
        private const float ContentPadding = 16f;
        private const float BorderThickness = 1f;
        private const float ShadowOffset = 2f;
        private const int GradientHeight = 64;
        private const int SmallGradientHeight = 32;

        private static readonly Type[] NodeTypes =
            { typeof(ConditionLeaf), typeof(AndCondition), typeof(OrCondition), typeof(NotCondition) };

        private static readonly string[] NodeTypeNames = { "Leaf", "AND", "OR", "NOT" };
        private static readonly string[] NodeTypeSymbols = { "●", "∧", "∨", "¬" };

        // Unity-themed color palette - matches Unity's official editor colors
        private static readonly Color CardColor = new(0.22f, 0.22f, 0.22f, 1f); // Unity's window background
        private static readonly Color CardColorLight = new(0.26f, 0.26f, 0.26f, 1f); // Slightly lighter for gradient
        private static readonly Color HeaderColor = new(0.28f, 0.28f, 0.28f, 1f); // Unity's header color
        private static readonly Color HeaderColorLight = new(0.32f, 0.32f, 0.32f, 1f); // Header gradient top
        private static readonly Color AccentColor = new(0.24f, 0.48f, 0.85f, 1f); // Unity's selection blue
        private static readonly Color AccentColorHover = new(0.28f, 0.54f, 0.92f, 1f); // Brighter blue for hover
        private static readonly Color TextColor = new(0.82f, 0.82f, 0.82f, 1f); // Unity's text color
        private static readonly Color TextColorDim = new(0.6f, 0.6f, 0.6f, 1f); // Dimmed text
        private static readonly Color BorderColor = new(0.13f, 0.13f, 0.13f, 1f); // Unity's border color
        private static readonly Color DeleteColor = new(0.85f, 0.24f, 0.24f, 1f); // Unity's error red
        private static readonly Color ShadowColor = new(0, 0, 0, 0.15f); // Subtle shadow
        private static readonly Color ConditionBgColor = new(0.18f, 0.18f, 0.18f, 0.8f); // Input field background

        // Cached content for reduced allocations
        private static readonly GUIContent ConditionBehaviourLabel = new("Condition Behaviour");
        private static readonly GUIContent AddChildLabel = new("+ Add Child Node");
        private static readonly GUIContent DeleteLabel = new("✕");
        private static readonly GUIContent NoConditionLabel = new("No condition node assigned");

        // Global style cache with cleanup mechanism
        private static readonly Dictionary<int, Dictionary<string, GUIStyle>> GlobalStyleCache = new();
        private static readonly Dictionary<int, Texture2D> TextureCache = new();
        private static int _lastCleanupFrame = -1;

        // Pre-allocated collections to reduce GC pressure
        private static readonly Stack<(SerializedProperty prop, int indent)> PropertyStack = new();
        private static readonly List<ConditionNode> TempChildList = new();

        static ConditionNodePropertyDrawer()
        {
            // Register cleanup callback
            EditorApplication.update += CleanupOldCaches;
        }

        private static void CleanupOldCaches()
        {
            int currentFrame = Time.frameCount;
            if (_lastCleanupFrame == currentFrame) return;

            // Cleanup every 100 frames to prevent memory leaks
            if (currentFrame % 100 == 0)
            {
                var framesToRemove = new List<int>();
                foreach (var kvp in GlobalStyleCache)
                {
                    if (currentFrame - kvp.Key > 300) // Remove caches older than 300 frames
                        framesToRemove.Add(kvp.Key);
                }

                foreach (int frame in framesToRemove)
                {
                    GlobalStyleCache.Remove(frame);
                    if (TextureCache.TryGetValue(frame, out Texture2D tex) && tex != null)
                    {
                        Object.DestroyImmediate(tex);
                        TextureCache.Remove(frame);
                    }
                }
            }

            _lastCleanupFrame = currentFrame;
        }

        private static Dictionary<string, GUIStyle> GetCurrentFrameStyles()
        {
            int currentFrame = Time.frameCount;
            if (!GlobalStyleCache.TryGetValue(currentFrame, out var styles))
            {
                styles = new Dictionary<string, GUIStyle>();
                GlobalStyleCache[currentFrame] = styles;
            }

            return styles;
        }

        private static GUIStyle GetCachedStyle(string key, Func<GUIStyle> factory)
        {
            if (Event.current?.type == EventType.Layout) return GUIStyle.none;

            var styles = GetCurrentFrameStyles();
            if (!styles.TryGetValue(key, out GUIStyle style))
            {
                style = factory();
                styles[key] = style;
            }

            return style;
        }

        private static Texture2D GetCachedTexture(string key, Func<Texture2D> factory)
        {
            int currentFrame = Time.frameCount;
            int textureKey = key.GetHashCode() ^ currentFrame;

            if (!TextureCache.TryGetValue(textureKey, out Texture2D texture))
            {
                texture = factory();
                TextureCache[textureKey] = texture;
            }

            return texture;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property?.managedReferenceValue == null)
            {
                EditorGUI.HelpBox(position, NoConditionLabel.text, MessageType.Info);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // No shadow - cleaner Unity-style flat design

            float y = position.y;
            DrawNode(ref y, position.x, position.width, property, 0, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property?.managedReferenceValue == null)
                return EditorGUIUtility.singleLineHeight + 12;

            return CalculateNodeHeightOptimized(property);
        }

        private void DrawNode(ref float y, float x, float width, SerializedProperty property, int indent,
            bool isRoot = false)
        {
            string nodeType = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(nodeType)) return;

            // Optimized type lookup using Contains check with early exit
            int typeIdx = GetNodeTypeIndex(nodeType);

            float cardWidth = width - indent * IndentSize - CardPadding * 2;
            float cardX = x + indent * IndentSize + CardPadding;

            // Early exit for invalid dimensions
            if (cardWidth <= 0) return;

            float cardHeight = CalculateNodeHeightOptimized(property);
            Rect cardRect = new(cardX, y, cardWidth, cardHeight);

            // Draw card components
            DrawCardBackground(cardRect);
            DrawHeader(cardRect, typeIdx, property, isRoot, cardX, cardWidth);

            y += HeaderHeight + 4;
            DrawNodeContentOptimized(ref y, cardX, cardWidth, property, nodeType, indent);
        }

        private static int GetNodeTypeIndex(string nodeType)
        {
            // Optimized type detection using switch on first character
            if (nodeType.Contains("ConditionLeaf")) return 0;
            if (nodeType.Contains("AndCondition")) return 1;
            if (nodeType.Contains("OrCondition")) return 2;
            if (nodeType.Contains("NotCondition")) return 3;
            return 0;
        }

        private void DrawCardBackground(Rect cardRect)
        {
            GUIStyle cardStyle = GetCachedStyle("card", CreateCardStyle);
            GUI.Box(cardRect, GUIContent.none, cardStyle);
            DrawBorderOptimized(cardRect, BorderColor, BorderThickness);
        }

        private void DrawHeader(Rect cardRect, int typeIdx, SerializedProperty property, bool isRoot, float cardX,
            float cardWidth)
        {
            Rect headerRect = new(cardRect.x + 1, cardRect.y + 1, cardRect.width - 2, HeaderHeight);
            GUIStyle headerStyle = GetCachedStyle("header", CreateHeaderStyle);
            GUI.Box(headerRect, GUIContent.none, headerStyle);

            // Symbol
            Rect symbolRect = new(cardX + 12, cardRect.y + 6, SymbolSize, SymbolSize);
            GUIStyle symbolStyle = GetCachedStyle("symbol", CreateSymbolStyle);
            EditorGUI.LabelField(symbolRect, NodeTypeSymbols[typeIdx], symbolStyle);

            // Type dropdown
            Rect dropdownRect = new(cardX + SymbolSize + Spacing + 8, cardRect.y + 8, 90,
                EditorGUIUtility.singleLineHeight + 2);

            GUIStyle typeStyle = GetCachedStyle("type", CreateTypeStyle);

            EditorGUI.BeginChangeCheck();
            int newTypeIdx = EditorGUI.Popup(dropdownRect, typeIdx, NodeTypeNames, typeStyle);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeNodeTypeOptimized(property, typeIdx, newTypeIdx);
                return;
            }

            // Delete button
            if (!isRoot)
            {
                Rect deleteRect = new(cardX + cardWidth - 36, cardRect.y + 6, 28, 24);
                GUIStyle deleteStyle = GetCachedStyle("delete", CreateDeleteButtonStyle);

                if (GUI.Button(deleteRect, DeleteLabel, deleteStyle))
                {
                    DeleteNode(property);
                }
            }
        }

        private void DrawNodeContentOptimized(ref float y, float cardX, float cardWidth, SerializedProperty property,
            string nodeType, int indent)
        {
            // Use switch for better performance than multiple Contains calls
            if (nodeType.Contains("ConditionLeaf"))
            {
                DrawConditionLeafOptimized(ref y, cardX, cardWidth, property);
            }
            else if (nodeType.Contains("AndCondition") || nodeType.Contains("OrCondition"))
            {
                DrawLogicalConditionOptimized(ref y, cardX, cardWidth, property, indent);
            }
            else if (nodeType.Contains("NotCondition"))
            {
                DrawNotConditionOptimized(ref y, cardX, cardWidth, property, indent);
            }

            y += 8; // Bottom padding
        }

        private void DrawConditionLeafOptimized(ref float y, float cardX, float cardWidth, SerializedProperty property)
        {
            SerializedProperty condProp = property.FindPropertyRelative("conditionBehaviour");
            if (condProp == null) return;

            float condHeight = EditorGUI.GetPropertyHeight(condProp, true);
            Rect condRect = new(cardX + ContentPadding, y, cardWidth - ContentPadding * 2, condHeight);

            // Draw background once with optimized rect calculation
            Rect bgRect = new(condRect.x - 4, condRect.y - 4, condRect.width + 8, condRect.height + 8);
            EditorGUI.DrawRect(bgRect, ConditionBgColor);

            EditorGUI.PropertyField(condRect, condProp, ConditionBehaviourLabel, true);
            y += condHeight + 12;
        }

        private void DrawLogicalConditionOptimized(ref float y, float cardX, float cardWidth,
            SerializedProperty property, int indent)
        {
            SerializedProperty childrenProp = property.FindPropertyRelative("children");
            if (childrenProp == null) return;

            int childCount = childrenProp.arraySize;
            for (int i = 0; i < childCount; i++)
            {
                SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                if (childProp.managedReferenceValue == null) continue;

                DrawNode(ref y, cardX - indent * IndentSize, cardWidth + indent * IndentSize, childProp, indent + 1);
                y += 6;
            }

            DrawAddButtonOptimized(cardX, cardWidth, ref y, () => AddChildNodeOptimized(property, childrenProp));
        }

        private void DrawNotConditionOptimized(ref float y, float cardX, float cardWidth, SerializedProperty property,
            int indent)
        {
            SerializedProperty childProp = property.FindPropertyRelative("child");
            if (childProp == null) return;

            if (childProp.managedReferenceValue == null)
            {
                DrawAddButtonOptimized(cardX, cardWidth, ref y, () => AddChildToNotNodeOptimized(property, childProp));
            }
            else
            {
                DrawNode(ref y, cardX - indent * IndentSize, cardWidth + indent * IndentSize, childProp, indent + 1);
                y += 6;
            }
        }

        private void DrawAddButtonOptimized(float cardX, float cardWidth, ref float y, Action onAddAction)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect addRect = new(cardX + ContentPadding, y, cardWidth - ContentPadding * 2, lineHeight + 8);
            GUIStyle addStyle = GetCachedStyle("addButton", CreateAddButtonStyle);

            if (GUI.Button(addRect, AddChildLabel, addStyle))
            {
                onAddAction();
            }

            y += lineHeight + 16;
        }

        private void ChangeNodeTypeOptimized(SerializedProperty property, int oldTypeIdx, int newTypeIdx)
        {
            object newNode = Activator.CreateInstance(NodeTypes[newTypeIdx]);
            PreserveChildrenOptimized(property, newNode, oldTypeIdx, newTypeIdx);

            property.managedReferenceValue = newNode;
            ApplyChanges(property);
        }

        private void PreserveChildrenOptimized(SerializedProperty property, object newNode, int oldTypeIdx,
            int newTypeIdx)
        {
            SerializedProperty oldChildren = property.FindPropertyRelative("children");
            SerializedProperty oldChild = property.FindPropertyRelative("child");

            // Clear temp list for reuse
            TempChildList.Clear();

            // Optimized preservation logic with early returns
            if (IsLogicalOperator(oldTypeIdx) && IsLogicalOperator(newTypeIdx))
            {
                // AND/OR <-> AND/OR: preserve all children
                CollectChildren(oldChildren, TempChildList);
                ((dynamic)newNode).children = new List<ConditionNode>(TempChildList);
            }
            else if (newTypeIdx == 3 && IsLogicalOperator(oldTypeIdx)) // -> NOT
            {
                ConditionNode first = GetFirstChildOptimized(oldChildren) ?? new ConditionLeaf();
                ((NotCondition)newNode).child = first;
            }
            else if (oldTypeIdx == 3 && IsLogicalOperator(newTypeIdx)) // NOT ->
            {
                if (oldChild?.managedReferenceValue != null)
                    TempChildList.Add((ConditionNode)oldChild.managedReferenceValue);

                ((dynamic)newNode).children = new List<ConditionNode>(TempChildList);
            }
            else if (oldTypeIdx == 3 && newTypeIdx == 3) // NOT -> NOT
            {
                ((NotCondition)newNode).child = oldChild?.managedReferenceValue as ConditionNode ?? new ConditionLeaf();
            }
        }

        private static void CollectChildren(SerializedProperty childrenProp, List<ConditionNode> target)
        {
            if (childrenProp == null) return;

            int count = childrenProp.arraySize;
            for (int i = 0; i < count; i++)
            {
                SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                if (childProp.managedReferenceValue != null)
                    target.Add((ConditionNode)childProp.managedReferenceValue);
            }
        }

        private static ConditionNode GetFirstChildOptimized(SerializedProperty childrenProp)
        {
            if (childrenProp == null) return null;

            int count = childrenProp.arraySize;
            for (int i = 0; i < count; i++)
            {
                SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                if (childProp.managedReferenceValue != null)
                    return (ConditionNode)childProp.managedReferenceValue;
            }

            return null;
        }

        private static bool IsLogicalOperator(int typeIdx) => typeIdx == 1 || typeIdx == 2; // AND or OR

        private static void DeleteNode(SerializedProperty property)
        {
            property.managedReferenceValue = null;
            ApplyChanges(property);
        }

        private void AddChildNodeOptimized(SerializedProperty property, SerializedProperty childrenProp)
        {
            childrenProp.InsertArrayElementAtIndex(childrenProp.arraySize);
            SerializedProperty newChild = childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1);
            newChild.managedReferenceValue = new ConditionLeaf();
            ApplyChanges(property);
        }

        private void AddChildToNotNodeOptimized(SerializedProperty property, SerializedProperty childProp)
        {
            childProp.managedReferenceValue = new ConditionLeaf();
            ApplyChanges(property);
        }

        private static void ApplyChanges(SerializedProperty property)
        {
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            GUI.FocusControl(null);
        }

        private float CalculateNodeHeightOptimized(SerializedProperty property)
        {
            if (property?.managedReferenceValue == null)
                return EditorGUIUtility.singleLineHeight + 12;

            // Clear and reuse stack to avoid allocations
            PropertyStack.Clear();

            try
            {
                PropertyStack.Push((property, 0));
                float totalHeight = 0;

                while (PropertyStack.Count > 0)
                {
                    (SerializedProperty prop, int indent) = PropertyStack.Pop();

                    // Enhanced null checking
                    if (prop == null || prop.managedReferenceValue == null)
                        continue;

                    string nodeType = prop.managedReferenceFullTypename;
                    if (string.IsNullOrEmpty(nodeType))
                        continue;

                    totalHeight += HeaderHeight + 4; // Header + spacing

                    // Optimized height calculation based on node type
                    if (nodeType.Contains("ConditionLeaf"))
                    {
                        SerializedProperty condProp = prop.FindPropertyRelative("conditionBehaviour");
                        if (condProp != null)
                        {
                            try
                            {
                                totalHeight += EditorGUI.GetPropertyHeight(condProp, true) + 12;
                            }
                            catch
                            {
                                // Fallback if GetPropertyHeight fails
                                totalHeight += EditorGUIUtility.singleLineHeight + 12;
                            }
                        }
                    }
                    else if (nodeType.Contains("AndCondition") || nodeType.Contains("OrCondition"))
                    {
                        SerializedProperty childrenProp = prop.FindPropertyRelative("children");
                        if (childrenProp != null && childrenProp.isArray)
                        {
                            int childCount = childrenProp.arraySize;
                            for (int i = 0; i < childCount; i++)
                            {
                                try
                                {
                                    SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(i);
                                    if (childProp != null && childProp.managedReferenceValue != null)
                                    {
                                        PropertyStack.Push((childProp, indent + 1));
                                        totalHeight += 6; // Child spacing
                                    }
                                }
                                catch
                                {
                                    // Skip invalid array elements
                                }
                            }
                        }

                        totalHeight += EditorGUIUtility.singleLineHeight + 24; // Add button + padding
                    }
                    else if (nodeType.Contains("NotCondition"))
                    {
                        SerializedProperty childProp = prop.FindPropertyRelative("child");
                        if (childProp != null && childProp.managedReferenceValue != null)
                        {
                            PropertyStack.Push((childProp, indent + 1));
                            totalHeight += 6; // Child spacing
                        }
                        else
                        {
                            totalHeight += EditorGUIUtility.singleLineHeight + 24; // Add button + padding
                        }
                    }

                    totalHeight += 8; // Bottom padding
                }

                return totalHeight;
            }
            catch (Exception ex)
            {
                // Fallback calculation if something goes wrong
                Debug.LogWarning($"ConditionNodePropertyDrawer height calculation failed: {ex.Message}");
                return EditorGUIUtility.singleLineHeight * 3 + 24; // Safe fallback
            }
            finally
            {
                // Always clear the stack to prevent issues
                PropertyStack.Clear();
            }
        }

        #region Style Factories (Optimized)

        private static GUIStyle CreateCardStyle() => new(GUI.skin.box)
        {
            margin = new RectOffset(0, 0, 2, 2),
            padding = new RectOffset(16, 16, 16, 16),
            border = new RectOffset(12, 12, 12, 12),
            normal =
            {
                background = GetCachedTexture("cardGradient",
                    () => CreateGradientTexture(CardColor, CardColorLight, GradientHeight))
            },
            overflow = new RectOffset(1, 1, 1, 1)
        };

        private static GUIStyle CreateHeaderStyle() => new(GUI.skin.box)
        {
            padding = new RectOffset(12, 12, 8, 8),
            normal =
            {
                background = GetCachedTexture("headerGradient",
                    () => CreateGradientTexture(HeaderColor, HeaderColorLight, SmallGradientHeight))
            },
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            border = new RectOffset(8, 8, 8, 8),
            overflow = new RectOffset(0, 0, 0, 0)
        };

        private static GUIStyle CreateSymbolStyle() => new(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = AccentColor }
        };

        private static GUIStyle CreateTypeStyle() => new(EditorStyles.popup)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            normal = { textColor = TextColorDim },
            alignment = TextAnchor.MiddleLeft
        };

        private static GUIStyle CreateAddButtonStyle() => new(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 11,
            normal =
            {
                textColor = TextColor,
                background = GetCachedTexture("addButton", () => CreateSolidTexture(AccentColor))
            },
            hover =
            {
                textColor = Color.white,
                background = GetCachedTexture("addButtonHover", () => CreateSolidTexture(AccentColorHover))
            },
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(12, 12, 8, 8),
            border = new RectOffset(6, 6, 6, 6)
        };

        private static GUIStyle CreateDeleteButtonStyle() => new(GUI.skin.button)
        {
            fixedWidth = 28,
            fixedHeight = 24,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = TextColorDim,
                background = GetCachedTexture("deleteButton",
                    () => CreateSolidTexture(new Color(0.2f, 0.2f, 0.2f, 0.8f)))
            },
            hover =
            {
                textColor = Color.white,
                background = GetCachedTexture("deleteButtonHover", () => CreateSolidTexture(DeleteColor))
            },
            border = new RectOffset(4, 4, 4, 4)
        };

        #endregion

        #region Optimized Texture and Drawing Methods

        private static Texture2D CreateGradientTexture(Color colorA, Color colorB, int height)
        {
            Texture2D texture = new(1, height, TextureFormat.RGBA32, false);
            var colors = new Color[height];

            float invHeight = 1f / (height - 1);
            for (int i = 0; i < height; i++)
            {
                float t = i * invHeight;
                colors[i] = Color.Lerp(colorA, colorB, t);
            }

            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void DrawBorderOptimized(Rect rect, Color color, float thickness)
        {
            // Draw border as 4 separate rects - more efficient than multiple DrawRect calls
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color); // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color); // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color); // Left
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color); // Right
        }

        #endregion

    }
}

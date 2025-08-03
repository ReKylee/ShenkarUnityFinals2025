﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerUps.Container;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    ///     Enhanced Prefab Auditor Window with optimized performance and improved functionality
    /// </summary>
    public class PrefabAuditorWindow : EditorWindow
    {

        #region Enums

        public enum FilterType
        {
            All,
            Enemies,
            Containers,
            Others
        }

        #endregion

        #region Utility Methods

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        #endregion

        private void HandlePlacementMode(Event currentEvent, SceneView sceneView)
        {
            if (!_placementMode || _selectedPrefabForPlacement == null) return;

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            Vector3 worldPos = GetMouseWorldPosition(currentEvent);

            if (currentEvent.shift && _settings.pixelsPerUnit > 0)
            {
                worldPos = SnapToGrid(worldPos);
            }

            if (currentEvent.type is EventType.MouseMove or EventType.Repaint)
            {
                DrawPlacementPreview(currentEvent.mousePosition, worldPos);
                sceneView.Repaint();
            }
            else if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                PlacePrefabAtPosition(_selectedPrefabForPlacement, worldPos);
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
            {
                ExitPlacementMode();
                currentEvent.Use();
            }
        }

        private void DrawPlacementPreview(Vector2 mousePos, Vector3 worldPos)
        {
            // Get prefab bounds for accurate sizing
            Bounds prefabBounds = GetPrefabBounds(_selectedPrefabForPlacement);
            Vector3 worldSize = prefabBounds.size;

            // If bounds are too small or invalid, use default size
            if (worldSize.magnitude < 0.1f)
            {
                worldSize = Vector3.one * 0.5f;
            }

            // Scale the world size to compensate for asset preview padding BEFORE any calculations
            // Unity's asset previews have internal padding, so we scale up the world size to compensate
            worldSize *= 1.4f;

            // Apply grid snapping to the preview position if enabled
            Vector3 previewWorldPos = worldPos;
            if (Event.current.shift && _settings.pixelsPerUnit > 0)
            {
                previewWorldPos = SnapToGrid(worldPos);
            }

            // Convert the snapped world position back to screen coordinates for the preview
            Vector2 previewScreenPos = mousePos;
            Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
            if (sceneCamera)
            {
                previewScreenPos = mousePos;
            }

            // Calculate accurate GUI preview size based on actual world size and camera
            float screenSize = 80f; // Base size in pixels

            if (sceneCamera)
            {
                if (sceneCamera.orthographic)
                {
                    // For orthographic camera, calculate screen size directly
                    float orthographicSize = sceneCamera.orthographicSize;
                    float screenHeight = SceneView.lastActiveSceneView.position.height;

                    // Calculate pixels per world unit
                    float pixelsPerWorldUnit = screenHeight / (orthographicSize * 2f);

                    // Use the larger dimension of the sprite for accurate representation
                    float largestWorldDimension = Mathf.Max(worldSize.x, worldSize.y, worldSize.z);
                    screenSize = largestWorldDimension * pixelsPerWorldUnit;
                }
                else
                {
                    // For perspective camera, factor in distance
                    float distance = Vector3.Distance(sceneCamera.transform.position, previewWorldPos);
                    float fieldOfViewRad = sceneCamera.fieldOfView * Mathf.Deg2Rad;
                    float screenHeight = SceneView.lastActiveSceneView.position.height;

                    // Calculate how many world units fit in screen height at this distance
                    float worldUnitsInScreenHeight = 2f * distance * Mathf.Tan(fieldOfViewRad * 0.5f);
                    float pixelsPerWorldUnit = screenHeight / worldUnitsInScreenHeight;

                    // Use the larger dimension of the sprite for accurate representation
                    float largestWorldDimension = Mathf.Max(worldSize.x, worldSize.y, worldSize.z);
                    screenSize = largestWorldDimension * pixelsPerWorldUnit;
                }

                // Clamp to reasonable bounds but allow larger sizes for bigger sprites
                screenSize = Mathf.Clamp(screenSize, 20f, 400f);
            }

            // Get the prefab preview texture
            Texture2D preview = GetPrefabPreview(_selectedPrefabForPlacement);

            if (preview != null)
            {
                Handles.BeginGUI();

                // Calculate preview rect centered on the snapped position with accurate size
                Rect previewRect = new(
                    previewScreenPos.x - screenSize * 0.5f,
                    previewScreenPos.y - screenSize * 0.5f,
                    screenSize,
                    screenSize
                );

                // Draw the preview image with transparency support
                Color originalColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 1f); // Ensure full alpha for transparency
                GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit, true); // Enable alpha blending
                GUI.color = originalColor;

                // Draw center crosshair for precise placement (use snapped position)
                Vector2 center = previewScreenPos;
                float crosshairSize = 6f;
                Color crosshairColor = Color.red;
                EditorGUI.DrawRect(new Rect(center.x - crosshairSize, center.y - 0.5f, crosshairSize * 2, 1),
                    crosshairColor);

                EditorGUI.DrawRect(new Rect(center.x - 0.5f, center.y - crosshairSize, 1, crosshairSize * 2),
                    crosshairColor);

                // Show prefab info below the preview
                string infoText = _selectedPrefabForPlacement.name;
                if (Event.current.shift && _settings.pixelsPerUnit > 0)
                {
                    infoText += " (Grid Snap)";
                }

                GUIContent infoContent = new(infoText);
                Vector2 infoSize = EditorStyles.miniLabel.CalcSize(infoContent);
                Rect infoRect = new(
                    previewScreenPos.x - infoSize.x * 0.5f,
                    previewRect.y + previewRect.height + 5,
                    infoSize.x,
                    infoSize.y
                );

                Color backgroundColor = new(0, 0, 0, 0.7f);
                // Draw info background
                EditorGUI.DrawRect(new Rect(infoRect.x - 2, infoRect.y - 1, infoRect.width + 4, infoRect.height + 2),
                    backgroundColor);

                // Draw info text
                GUI.Label(infoRect, infoContent, EditorStyles.miniLabel);

                Handles.EndGUI();
            }

            // Grid snap visualization (using the snapped world position)
            if (Event.current.shift && _settings.pixelsPerUnit > 0)
            {
                Handles.color = Color.yellow;
                float gridSize = 1.0f / _settings.pixelsPerUnit;

                // Draw snap grid around cursor
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        Vector3 gridPoint = previewWorldPos + new Vector3(i * gridSize, j * gridSize, 0);
                        Handles.DrawWireCube(gridPoint, Vector3.one * gridSize * 0.2f);
                    }
                }
            }

            Handles.color = Color.white;
        }

        private Bounds GetPrefabBounds(GameObject prefab)
        {
            if (prefab == null) return new Bounds();

            // Try to get bounds from renderers
            var renderers = prefab.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                return bounds;
            }

            // Try to get bounds from colliders
            var colliders = prefab.GetComponentsInChildren<Collider>();
            if (colliders != null && colliders.Length > 0)
            {
                Bounds bounds = colliders[0].bounds;
                foreach (Collider collider in colliders)
                {
                    bounds.Encapsulate(collider.bounds);
                }

                return bounds;
            }

            // Try to get bounds from colliders 2D
            var colliders2D = prefab.GetComponentsInChildren<Collider2D>();
            if (colliders2D is { Length: > 0 })
            {
                Bounds bounds = colliders2D[0].bounds;
                foreach (Collider2D collider in colliders2D)
                {
                    bounds.Encapsulate(collider.bounds);
                }

                return bounds;
            }

            // Fallback to transform bounds
            return new Bounds(prefab.transform.position, Vector3.one);
        }

        private void DrawRadialMenu()
        {
            Handles.BeginGUI();

            Vector2 center = _radialMenuPosition;
            float elapsedTime = (float)EditorApplication.timeSinceStartup - _radialMenuOpenTime;
            float growProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsedTime / RadialMenuAnimationTime));
            float radius = RadialMenuRadius * growProgress;

            var prefabsToShow = _frequentPrefabs.Count > 0
                ? _frequentPrefabs
                : _filteredPrefabs.Take(MaxFrequentPrefabs).Select(p => p.prefab).ToList();

            int prefabCount = Mathf.Min(prefabsToShow.Count, MaxFrequentPrefabs);
            if (prefabCount == 0)
            {
                Handles.EndGUI();
                return;
            }

            float segmentAngle = 360f / prefabCount;

            // Layer 1: Background and outer ring
            DrawRadialBackground(center, radius, growProgress);

            // Layer 2: Segment highlights (if any item is hovered)
            if (_hoveredRadialIndex >= 0 && _hoveredRadialIndex < prefabCount)
            {
                DrawSegmentHighlight(center, radius, _hoveredRadialIndex, segmentAngle);
            }

            // Layer 3: Segment dividers
            DrawSegmentDividers(center, radius, prefabCount, segmentAngle);

            // Layer 4: Prefab items
            DrawPrefabItems(center, radius, prefabsToShow, prefabCount, segmentAngle);

            // Layer 5: Center indicator
            DrawCenterIndicator(center, growProgress);

            // Layer 6: Hovered item tooltip
            if (_hoveredRadialIndex >= 0 && _hoveredRadialIndex < prefabCount)
            {
                DrawItemTooltip(center, radius, prefabsToShow[_hoveredRadialIndex]);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawRadialBackground(Vector2 center, float radius, float growProgress)
        {
            // Outer glow effect
            for (int i = 0; i < 3; i++)
            {
                float glowRadius = radius + (i + 1) * 8f;
                float glowAlpha = (0.1f - i * 0.03f) * growProgress;
                Handles.color = new Color(0.4f, 0.7f, 1f, glowAlpha);
                Handles.DrawSolidDisc(center, Vector3.forward, glowRadius);
            }

            // Main background with gradient effect
            Handles.color = new Color(0.15f, 0.15f, 0.2f, 0.9f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, radius);

            // Subtle inner shadow
            Handles.color = new Color(0.05f, 0.05f, 0.1f, 0.4f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, radius * 0.95f);

            // Main background again for clean appearance
            Handles.color = new Color(0.2f, 0.2f, 0.25f, 0.85f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, radius * 0.9f);

            // Outer border with animation
            Handles.color = new Color(0.6f, 0.6f, 0.7f, 0.8f * growProgress);
            Handles.DrawWireDisc(center, Vector3.forward, radius);

            // Inner ring for depth
            float innerRadius = radius * 0.3f;
            Handles.color = new Color(0.4f, 0.4f, 0.5f, 0.6f * growProgress);
            Handles.DrawWireDisc(center, Vector3.forward, innerRadius);

            // Very subtle inner fill
            Handles.color = new Color(0.25f, 0.25f, 0.3f, 0.3f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, innerRadius);
        }

        private void DrawSegmentHighlight(Vector2 center, float radius, int hoveredIndex, float segmentAngle)
        {
            float startAngle = hoveredIndex * segmentAngle - 90f - segmentAngle * 0.5f;
            float endAngle = startAngle + segmentAngle;

            // Create segment shape for highlight
            var highlightPoints = new List<Vector3>();

            // Add center point
            highlightPoints.Add(new Vector3(center.x, center.y, 0));

            // Add arc points from inner to outer radius
            int arcResolution = 20;
            for (int i = 0; i <= arcResolution; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)arcResolution) * Mathf.Deg2Rad;
                Vector2 outerPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.9f;
                highlightPoints.Add(new Vector3(outerPoint.x, outerPoint.y, 0));
            }

            // Multi-layer highlight for depth
            // Outer glow
            Handles.color = new Color(0.4f, 0.7f, 1f, 0.08f);
            Handles.DrawAAConvexPolygon(highlightPoints.ToArray());

            // Main highlight
            Handles.color = new Color(0.5f, 0.8f, 1f, 0.15f);
            var mainHighlightPoints = new List<Vector3> { new(center.x, center.y, 0) };
            for (int i = 0; i <= arcResolution; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)arcResolution) * Mathf.Deg2Rad;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.85f;
                mainHighlightPoints.Add(new Vector3(point.x, point.y, 0));
            }

            Handles.DrawAAConvexPolygon(mainHighlightPoints.ToArray());

            // Bright edge highlight
            Handles.color = new Color(0.7f, 0.9f, 1f, 0.6f);
            for (int i = 0; i <= arcResolution; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)arcResolution) * Mathf.Deg2Rad;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.88f;

                if (i < arcResolution)
                {
                    float nextAngle = Mathf.Lerp(startAngle, endAngle, (i + 1) / (float)arcResolution) * Mathf.Deg2Rad;
                    Vector2 nextPoint =
                        center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius * 0.88f;

                    Handles.DrawLine(point, nextPoint);
                }
            }
        }

        private void DrawSegmentDividers(Vector2 center, float radius, int prefabCount, float segmentAngle)
        {
            for (int i = 0; i < prefabCount; i++)
            {
                float angle = i * segmentAngle - 90f + segmentAngle * 0.5f;
                Vector2 direction = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                Vector2 lineStart = center + direction * (radius * 0.32f);
                Vector2 lineEnd = center + direction * (radius * 0.88f);

                // Subtle shadow line
                Handles.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
                Vector2 shadowOffset = Vector2.one * 0.5f;
                Handles.DrawLine(lineStart + shadowOffset, lineEnd + shadowOffset);

                // Main divider line - only highlight the dividers that border the hovered segment
                bool isAdjacentToHovered = _hoveredRadialIndex >= 0 &&
                                           i ==
                                           _hoveredRadialIndex; // Only highlight the current segment's starting divider

                Handles.color = isAdjacentToHovered
                    ? new Color(0.7f, 0.9f, 1f, 0.8f)
                    : new Color(0.4f, 0.4f, 0.5f, 0.5f);

                Handles.DrawLine(lineStart, lineEnd);
            }
        }

        private void DrawPrefabItems(Vector2 center, float radius, List<GameObject> prefabsToShow, int prefabCount,
            float segmentAngle)
        {
            for (int i = 0; i < prefabCount; i++)
            {
                GameObject prefab = prefabsToShow[i];
                if (prefab == null) continue;

                float angle = i * segmentAngle - 90f;
                Vector2 direction = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 itemPos = center + direction * (radius * 0.65f);

                bool isHovered = i == _hoveredRadialIndex;
                float baseIconSize = 48f;
                float iconSize = isHovered ? baseIconSize * 1.2f : baseIconSize;

                // Item background with subtle animation
                if (isHovered)
                {
                    // Pulsing background
                    float pulseScale = 1f + Mathf.Sin(Time.realtimeSinceStartup * 3f) * 0.1f;
                    float bgRadius = iconSize * 0.5f * pulseScale;

                    // Background glow
                    Handles.color = new Color(0.4f, 0.7f, 1f, 0.2f);
                    Handles.DrawSolidDisc(itemPos, Vector3.forward, bgRadius * 1.3f);

                    // Main background
                    Handles.color = new Color(0.6f, 0.8f, 1f, 0.3f);
                    Handles.DrawSolidDisc(itemPos, Vector3.forward, bgRadius);

                    // Border ring
                    Handles.color = new Color(0.7f, 0.9f, 1f, 0.9f);
                    Handles.DrawWireDisc(itemPos, Vector3.forward, bgRadius);
                }

                // Icon container
                Rect iconRect = new(
                    itemPos.x - iconSize * 0.5f,
                    itemPos.y - iconSize * 0.5f,
                    iconSize,
                    iconSize
                );

                // Draw prefab preview
                Texture2D preview = GetPrefabPreview(prefab);
                if (preview != null)
                {
                    Color originalColor = GUI.color;

                    if (isHovered)
                    {
                        // Enhanced brightness and slight scale animation
                        float brightness = 1.2f + Mathf.Sin(Time.realtimeSinceStartup * 4f) * 0.1f;
                        GUI.color = new Color(brightness, brightness, brightness, 1f);
                    }
                    else
                    {
                        GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
                    }

                    GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit, true);
                    GUI.color = originalColor;
                }
                else
                {
                    // Loading state with animated dots
                    Color bgColor = isHovered ? new Color(0.9f, 0.95f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.8f, 0.4f);
                    EditorGUI.DrawRect(iconRect, bgColor);

                    // Animated loading indicator
                    int dotCount = Mathf.FloorToInt(Time.realtimeSinceStartup * 2f) % 4;
                    string dots = new string('●', dotCount) + new string('○', 3 - dotCount);

                    GUIStyle loadingStyle = new(EditorStyles.centeredGreyMiniLabel)
                    {
                        fontSize = 10,
                        normal = { textColor = isHovered ? Color.white : Color.gray }
                    };

                    GUI.Label(iconRect, dots, loadingStyle);
                }

                // Usage indicator
                if (_prefabUsageCount.ContainsKey(prefab) && _prefabUsageCount[prefab] > 0)
                {
                    Vector2 badgePos = itemPos + new Vector2(iconSize * 0.3f, -iconSize * 0.3f);
                    float badgeSize = 16f;

                    Rect badgeRect = new(badgePos.x - badgeSize * 0.5f, badgePos.y - badgeSize * 0.5f, badgeSize,
                        badgeSize);

                    // Badge background
                    Handles.color = new Color(1f, 0.6f, 0.2f, 0.9f);
                    Handles.DrawSolidDisc(badgePos, Vector3.forward, badgeSize * 0.5f);

                    // Badge border
                    Handles.color = new Color(1f, 0.8f, 0.4f, 1f);
                    Handles.DrawWireDisc(badgePos, Vector3.forward, badgeSize * 0.5f);

                    // Usage count
                    GUIStyle badgeStyle = new(EditorStyles.miniLabel)
                    {
                        fontSize = 8,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    };

                    GUI.Label(badgeRect, _prefabUsageCount[prefab].ToString(), badgeStyle);
                }
            }
        }

        private void DrawCenterIndicator(Vector2 center, float growProgress)
        {
            float centerRadius = 6f * growProgress;

            // Center shadow
            Handles.color = new Color(0f, 0f, 0f, 0.5f * growProgress);
            Handles.DrawSolidDisc(center + Vector2.one, Vector3.forward, centerRadius);

            // Center background
            Handles.color = new Color(0.3f, 0.5f, 0.8f, 0.9f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, centerRadius);

            // Center highlight
            Handles.color = new Color(0.8f, 0.9f, 1f, 1f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, centerRadius * 0.6f);

            // Center dot
            Handles.color = new Color(1f, 1f, 1f, 0.8f * growProgress);
            Handles.DrawSolidDisc(center, Vector3.forward, centerRadius * 0.3f);
        }

        private void DrawItemTooltip(Vector2 center, float radius, GameObject prefab)
        {
            if (prefab == null) return;

            GUIContent nameContent = new(prefab.name);
            Vector2 nameSize = EditorStyles.boldLabel.CalcSize(nameContent);

            Rect nameRect = new(
                center.x - nameSize.x * 0.5f,
                center.y + radius + 15,
                nameSize.x,
                nameSize.y
            );

            // Tooltip background with rounded corners effect
            Rect tooltipBg = new(nameRect.x - 8, nameRect.y - 3, nameRect.width + 16, nameRect.height + 6);

            // Shadow
            Rect shadowRect = new(tooltipBg.x + 1, tooltipBg.y + 1, tooltipBg.width, tooltipBg.height);
            EditorGUI.DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.3f));

            // Background
            EditorGUI.DrawRect(tooltipBg, new Color(0.1f, 0.1f, 0.15f, 0.95f));

            // Border
            Rect borderRect = new(tooltipBg.x - 1, tooltipBg.y - 1, tooltipBg.width + 2, tooltipBg.height + 2);
            EditorGUI.DrawRect(borderRect, new Color(0.5f, 0.8f, 1f, 0.8f));
            EditorGUI.DrawRect(tooltipBg, new Color(0.1f, 0.1f, 0.15f, 0.95f));

            // Text with subtle glow effect
            GUI.color = new Color(0.95f, 0.95f, 1f, 1f);
            GUI.Label(nameRect, nameContent, EditorStyles.boldLabel);

            // Usage info
            if (_prefabUsageCount.ContainsKey(prefab) && _prefabUsageCount[prefab] > 0)
            {
                string usageText = $"Used {_prefabUsageCount[prefab]} times";
                GUIContent usageContent = new(usageText);
                Vector2 usageSize = EditorStyles.miniLabel.CalcSize(usageContent);

                Rect usageRect = new(
                    center.x - usageSize.x * 0.5f,
                    nameRect.y + nameRect.height + 2,
                    usageSize.x,
                    usageSize.y
                );

                GUI.color = new Color(0.7f, 0.8f, 0.9f, 0.9f);
                GUI.Label(usageRect, usageContent, EditorStyles.miniLabel);
            }

            GUI.color = Color.white;
        }

        #region Serialized Settings

        [Serializable]
        public class WindowSettings
        {
            public string searchFilter = "";
            public FilterType filterType = FilterType.All;
            public bool useGridView = true;
            public bool autoRefresh = true;
            public bool showAdvancedSettings;
            public float pixelsPerUnit = 16f;
            public int gridColumns = 4;
            public bool enablePreviewCache = true;
            public bool enableVirtualization = true;
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class PrefabInfo
        {
            public GameObject prefab;
            public string name;
            public string path;
            public string guid;
            public FilterType category;
            public bool isValid;
            public long fileSize;

            public PrefabInfo(GameObject prefab)
            {
                this.prefab = prefab;
                name = prefab.name;
                path = AssetDatabase.GetAssetPath(prefab);
                guid = AssetDatabase.AssetPathToGUID(path);
                category = DetermineCategory(prefab);
                isValid = prefab;

                if (File.Exists(path))
                {
                    FileInfo fileInfo = new(path);
                    fileSize = fileInfo.Length;
                }
            }

            private static FilterType DetermineCategory(GameObject prefab)
            {
                if ((1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0)
                    return FilterType.Enemies;

                if (prefab.layer == LayerMask.NameToLayer("Collectibles") &&
                    prefab.GetComponent<PowerUpContainer>())
                    return FilterType.Containers;

                return FilterType.Others;
            }
        }

        #endregion

        #region Constants

        private const int MaxFrequentPrefabs = 8;
        private const float RadialMenuRadius = 100f;
        private const float RadialMenuAnimationTime = 0.3f;
        private const float PreviewSize = 64f;
        private const string PrefabFolderPath = "Assets/Prefabs";
        private const string UsageDataKey = "PrefabAuditor_UsageData_v2";
        private const string SettingsKey = "PrefabAuditor_Settings_v2";

        #endregion

        #region Private Fields

        // Core Data
        private readonly List<PrefabInfo> _prefabInfos = new();
        private readonly List<PrefabInfo> _filteredPrefabs = new();
        private readonly List<GameObject> _frequentPrefabs = new();
        private readonly Dictionary<GameObject, int> _prefabUsageCount = new();

        // Preview System
        private readonly Dictionary<GameObject, Texture2D> _previewCache = new();
        private readonly Queue<GameObject> _previewLoadQueue = new();
        private bool _isLoadingPreviews;

        // UI State
        private WindowSettings _settings = new();
        private Vector2 _scrollPosition;
        private string _lastSearchFilter = "";
        private FilterType _lastFilterType = FilterType.All;

        // Placement System
        private bool _placementMode;
        private GameObject _selectedPrefabForPlacement;
        private GameObject _defaultDropPrefab;

        // Radial Menu System
        private bool _showRadialMenu;
        private Vector2 _radialMenuPosition;
        private float _radialMenuOpenTime;
        private int _hoveredRadialIndex = -1;

        // Performance Tracking
        private double _lastRepaintTime;
        private int _virtualizedStartIndex;
        private int _virtualizedEndIndex;

        // Styles (cached)
        private static GUIStyle _headerStyle;
        private static GUIStyle _cardStyle;
        private static GUIStyle _buttonStyle;
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        #endregion

        #region Unity Lifecycle

        [MenuItem("Tools/Enhanced Prefab Manager")]
        public static void ShowWindow()
        {
            PrefabAuditorWindow window = GetWindow<PrefabAuditorWindow>("Prefab Manager");
            window.minSize = new Vector2(600, 700);
            window.maxSize = new Vector2(2000, 2000);
        }

        private void OnEnable()
        {
            LoadSettings();
            LoadPrefabs();
            LoadUsageData();
            UpdateFrequentPrefabs();

            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.projectChanged += OnProjectChanged;

            // Start preview loading
            EditorApplication.delayCall += StartPreviewLoading;
        }

        private void OnDisable()
        {
            SaveSettings();
            SaveUsageData();

            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.projectChanged -= OnProjectChanged;

            ClearPreviews();
        }

        private void OnProjectChanged()
        {
            if (_settings.autoRefresh)
            {
                EditorApplication.delayCall += LoadPrefabs;
            }
        }

        private void OnEditorUpdate()
        {
            double currentTime = EditorApplication.timeSinceStartup;

            // Limit repaints for performance
            if (_showRadialMenu && currentTime - _lastRepaintTime > 0.016f) // ~60fps
            {
                SceneView.lastActiveSceneView?.Repaint();
                _lastRepaintTime = currentTime;
            }

            // Process preview loading queue
            ProcessPreviewQueue();

            // Auto-refresh check
            if (_settings.autoRefresh &&
                (_lastSearchFilter != _settings.searchFilter || _lastFilterType != _settings.filterType))
            {
                ApplyFilters();
                _lastSearchFilter = _settings.searchFilter;
                _lastFilterType = _settings.filterType;
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            using (new EditorGUILayout.VerticalScope())
            {
                DrawHeader();
                DrawFiltersAndSearch();
                DrawQuickSettings();
                DrawPrefabsList();
                DrawFooter();
            }

            HandleKeyboardShortcuts();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event currentEvent = Event.current;

            // Handle radial menu first (higher priority)
            if (HandleRadialMenu(currentEvent, sceneView))
                return;

            // Handle placement mode
            HandlePlacementMode(currentEvent, sceneView);
        }

        #endregion

        #region UI Drawing

        private void InitializeStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 8, 8),
                margin = new RectOffset(4, 4, 2, 2)
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(8, 8, 4, 4)
            };


        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField("🎮 Enhanced Prefab Manager", _headerStyle);
                EditorGUILayout.LabelField(
                    $"Found {_prefabInfos.Count} prefabs | Showing {_filteredPrefabs.Count} | Cache: {_previewCache.Count}",
                    EditorStyles.miniLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🔄 Refresh", _buttonStyle, GUILayout.Width(80)))
                    {
                        LoadPrefabs();
                    }

                    _settings.autoRefresh =
                        GUILayout.Toggle(_settings.autoRefresh, "Auto Refresh", GUILayout.Width(100));

                    GUILayout.FlexibleSpace();

                    if (_placementMode && GUILayout.Button("❌ Exit Placement", _buttonStyle, GUILayout.Width(120)))
                    {
                        ExitPlacementMode();
                    }
                }
            }
        }

        private void DrawFiltersAndSearch()
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField("🔍 Filters & Search", EditorStyles.boldLabel);

                // Search with clear button
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
                    _settings.searchFilter = EditorGUILayout.TextField(_settings.searchFilter);
                    if (GUILayout.Button("✕", GUILayout.Width(20)))
                    {
                        _settings.searchFilter = "";
                        ApplyFilters();
                    }
                }

                // Filter and view options
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
                    FilterType newFilter = (FilterType)EditorGUILayout.EnumPopup(_settings.filterType);
                    if (newFilter != _settings.filterType)
                    {
                        _settings.filterType = newFilter;
                        ApplyFilters();
                    }

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("View:", GUILayout.Width(40));
                    _settings.useGridView =
                        GUILayout.Toggle(_settings.useGridView, "Grid", "Button", GUILayout.Width(50));

                    _settings.useGridView =
                        !GUILayout.Toggle(!_settings.useGridView, "List", "Button", GUILayout.Width(50));
                }
            }
        }

        private void DrawQuickSettings()
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                _settings.showAdvancedSettings = EditorGUILayout.Foldout(_settings.showAdvancedSettings, "⚙️ Settings",
                    true, EditorStyles.foldout);

                if (_settings.showAdvancedSettings)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Grid Snap (PPU):", GUILayout.Width(100));
                        _settings.pixelsPerUnit =
                            EditorGUILayout.FloatField(_settings.pixelsPerUnit, GUILayout.Width(60));

                        GUILayout.Space(10);

                        EditorGUILayout.LabelField("Grid Columns:", GUILayout.Width(85));
                        _settings.gridColumns =
                            EditorGUILayout.IntSlider(_settings.gridColumns, 2, 8, GUILayout.Width(100));
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Default Drop:", GUILayout.Width(80));
                        _defaultDropPrefab =
                            (GameObject)EditorGUILayout.ObjectField(_defaultDropPrefab, typeof(GameObject), false);

                        GUILayout.Space(10);

                        _settings.enablePreviewCache = GUILayout.Toggle(_settings.enablePreviewCache, "Preview Cache");
                        _settings.enableVirtualization =
                            GUILayout.Toggle(_settings.enableVirtualization, "Virtualization");
                    }
                }
            }
        }

        private void DrawPrefabsList()
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField("📦 Prefabs", EditorStyles.boldLabel);

                if (_filteredPrefabs.Count == 0)
                {
                    EditorGUILayout.HelpBox("No prefabs match current filters.", MessageType.Info);
                    return;
                }

                CalculateVirtualization();

                using (EditorGUILayout.ScrollViewScope scrollScope = new(_scrollPosition, GUILayout.ExpandHeight(true)))
                {
                    _scrollPosition = scrollScope.scrollPosition;

                    if (_settings.useGridView)
                    {
                        DrawPrefabsGrid();
                    }
                    else
                    {
                        DrawPrefabsListItems();
                    }
                }
            }
        }

        private void DrawPrefabsListItems()
        {
            var visiblePrefabs = GetVisiblePrefabs();

            foreach (PrefabInfo prefabInfo in visiblePrefabs)
            {
                if (prefabInfo?.prefab == null) continue;
                DrawPrefabListItem(prefabInfo);
            }
        }

        private void DrawPrefabListItem(PrefabInfo prefabInfo)
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Preview
                    Texture2D preview = GetPrefabPreview(prefabInfo.prefab);
                    if (preview != null)
                    {
                        GUILayout.Label(preview, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                    }
                    else
                    {
                        GUILayout.Box("Loading...", GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                    }

                    // Info
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(prefabInfo.name, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"Category: {prefabInfo.category}", EditorStyles.miniLabel);

                        if (_settings.showAdvancedSettings)
                        {
                            EditorGUILayout.LabelField($"Size: {FormatFileSize(prefabInfo.fileSize)}",
                                EditorStyles.miniLabel);
                        }
                    }

                    GUILayout.FlexibleSpace();

                    // Actions
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(180)))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("🖱️ Place", _buttonStyle)) StartPlacementMode(prefabInfo.prefab);
                            if (GUILayout.Button("📍 Instant", _buttonStyle))
                                PlacePrefabAtPosition(prefabInfo.prefab, Vector3.zero);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("🔍 Select", _buttonStyle)) Selection.activeObject = prefabInfo.prefab;
                            if (GUILayout.Button("📂 Show", _buttonStyle))
                                EditorGUIUtility.PingObject(prefabInfo.prefab);
                        }
                    }
                }
            }
        }

        private void DrawPrefabsGrid()
        {
            var visiblePrefabs = GetVisiblePrefabs();
            int columns = _settings.gridColumns;
            int rows = Mathf.CeilToInt((float)visiblePrefabs.Count / columns);

            for (int row = 0; row < rows; row++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int col = 0; col < columns; col++)
                    {
                        int index = row * columns + col;
                        if (index >= visiblePrefabs.Count)
                        {
                            GUILayout.FlexibleSpace();
                            continue;
                        }

                        PrefabInfo prefabInfo = visiblePrefabs[index];
                        if (prefabInfo?.prefab != null)
                        {
                            DrawPrefabGridItem(prefabInfo);
                        }
                    }
                }
            }
        }

        private void DrawPrefabGridItem(PrefabInfo prefabInfo)
        {
            float cellWidth = (position.width - 60) / _settings.gridColumns;

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(cellWidth), GUILayout.Height(140)))
            {
                // Preview
                Texture2D preview = GetPrefabPreview(prefabInfo.prefab);
                Rect rect = GUILayoutUtility.GetRect(cellWidth - 20, 80, GUILayout.ExpandWidth(false));

                if (preview != null)
                {
                    GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(rect, Color.gray * 0.3f);
                    GUI.Label(rect, "Loading...", EditorStyles.centeredGreyMiniLabel);
                }

                // Name and actions
                EditorGUILayout.LabelField(prefabInfo.name, EditorStyles.miniLabel, GUILayout.Width(cellWidth - 10));

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Place", GUILayout.Height(25)))
                    {
                        StartPlacementMode(prefabInfo.prefab);
                    }

                    if (GUILayout.Button("📂", GUILayout.Width(25), GUILayout.Height(25)))
                    {
                        EditorGUIUtility.PingObject(prefabInfo.prefab);
                    }
                }
            }
        }

        private void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("↶ Undo", _buttonStyle, GUILayout.Width(80)))
                    Undo.PerformUndo();

                if (GUILayout.Button("↷ Redo", _buttonStyle, GUILayout.Width(80)))
                    Undo.PerformRedo();

                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField($"Preview Queue: {_previewLoadQueue.Count}", EditorStyles.miniLabel);

                if (GUILayout.Button("🗑️ Clear Cache", _buttonStyle, GUILayout.Width(100)))
                {
                    ClearPreviews();
                }
            }
        }

        #endregion

        #region Performance Optimizations

        private void CalculateVirtualization()
        {
            if (!_settings.enableVirtualization)
            {
                _virtualizedStartIndex = 0;
                _virtualizedEndIndex = _filteredPrefabs.Count;
                return;
            }

            float itemHeight = _settings.useGridView ? 140f : 80f;
            float visibleHeight = position.height - 200f; // Account for UI elements
            float scrollOffset = _scrollPosition.y;

            int visibleStart = Mathf.Max(0, Mathf.FloorToInt(scrollOffset / itemHeight) - 5);
            int visibleCount = Mathf.CeilToInt(visibleHeight / itemHeight) + 10;

            _virtualizedStartIndex = visibleStart;
            _virtualizedEndIndex = Mathf.Min(_filteredPrefabs.Count, visibleStart + visibleCount);
        }

        private List<PrefabInfo> GetVisiblePrefabs()
        {
            if (!_settings.enableVirtualization)
                return _filteredPrefabs;

            return _filteredPrefabs.GetRange(_virtualizedStartIndex, _virtualizedEndIndex - _virtualizedStartIndex);
        }

        private void ProcessPreviewQueue()
        {
            if (!_settings.enablePreviewCache || _isLoadingPreviews || _previewLoadQueue.Count == 0)
                return;

            _isLoadingPreviews = true;

            // Process a few previews per frame
            for (int i = 0; i < 3 && _previewLoadQueue.Count > 0; i++)
            {
                GameObject prefab = _previewLoadQueue.Dequeue();
                if (prefab != null && !_previewCache.ContainsKey(prefab))
                {
                    Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                    if (preview != null)
                    {
                        _previewCache[prefab] = preview;
                    }
                    else
                    {
                        // Re-queue if preview not ready
                        _previewLoadQueue.Enqueue(prefab);
                        break;
                    }
                }
            }

            _isLoadingPreviews = false;

            // Repaint if we processed any previews
            if (_previewLoadQueue.Count % 10 == 0)
            {
                Repaint();
            }
        }

        private void StartPreviewLoading()
        {
            if (!_settings.enablePreviewCache) return;

            _previewLoadQueue.Clear();
            foreach (PrefabInfo prefabInfo in _prefabInfos.Where(prefabInfo =>
                         prefabInfo.prefab && !_previewCache.ContainsKey(prefabInfo.prefab)))
            {
                _previewLoadQueue.Enqueue(prefabInfo.prefab);
            }
        }

        #endregion

        #region Scene GUI Handling

        private bool HandleRadialMenu(Event currentEvent, SceneView sceneView)
        {
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                _showRadialMenu = true;
                _radialMenuPosition = currentEvent.mousePosition;
                _radialMenuOpenTime = (float)EditorApplication.timeSinceStartup;
                _hoveredRadialIndex = -1;
                currentEvent.Use();
                return true;
            }

            if (!_showRadialMenu) return false;

            // Always update hover state when mouse is moving or during repaint
            if (currentEvent.type is EventType.MouseMove or EventType.Repaint)
            {
                int newHoveredIndex = GetRadialMenuIndex(currentEvent.mousePosition);
                if (newHoveredIndex != _hoveredRadialIndex)
                {
                    _hoveredRadialIndex = newHoveredIndex;
                    sceneView.Repaint();
                }
            }

            DrawRadialMenu();

            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 1)
            {
                GameObject selectedPrefab = GetPrefabFromRadialMenu(currentEvent.mousePosition);
                if (selectedPrefab != null)
                {
                    StartPlacementMode(selectedPrefab);
                }

                _showRadialMenu = false;
                _hoveredRadialIndex = -1;
                currentEvent.Use();
                return true;
            }

            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
            {
                _showRadialMenu = false;
                _hoveredRadialIndex = -1;
                currentEvent.Use();
            }

            return true;
        }

        private GameObject GetPrefabFromRadialMenu(Vector2 mousePosition)
        {
            int index = GetRadialMenuIndex(mousePosition);
            if (index < 0) return null;

            var prefabsToShow = _frequentPrefabs.Count > 0
                ? _frequentPrefabs
                : _filteredPrefabs.Take(MaxFrequentPrefabs).Select(p => p.prefab).ToList();

            return index < prefabsToShow.Count ? prefabsToShow[index] : null;
        }

        private int GetRadialMenuIndex(Vector2 mousePosition)
        {
            float distance = Vector2.Distance(mousePosition, _radialMenuPosition);
            if (distance < 20 || distance > RadialMenuRadius * 0.9f) return -1; // Improved hit detection

            var prefabsToShow = _frequentPrefabs.Count > 0
                ? _frequentPrefabs
                : _filteredPrefabs.Take(MaxFrequentPrefabs).Select(p => p.prefab).ToList();

            int prefabCount = Mathf.Min(prefabsToShow.Count, MaxFrequentPrefabs);
            if (prefabCount == 0) return -1;

            // Improved angle calculation for better selection accuracy
            Vector2 direction = (mousePosition - _radialMenuPosition).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Normalize angle to 0-360 range and adjust for starting at top
            angle = (angle + 90f + 360f) % 360f;

            float segmentAngle = 360f / prefabCount;
            int index = Mathf.FloorToInt((angle + segmentAngle * 0.5f) / segmentAngle) % prefabCount;

            return index;
        }

        #endregion

        #region Core Logic

        private void StartPlacementMode(GameObject prefab)
        {
            if (!prefab) return;

            _placementMode = true;
            _selectedPrefabForPlacement = prefab;
            IncrementPrefabUsage(prefab);
            GUI.FocusControl(null);

            Debug.Log($"Started placement mode for: {prefab.name}");
        }

        private void ExitPlacementMode()
        {
            _placementMode = false;
            _selectedPrefabForPlacement = null;
            Debug.Log("Exited placement mode");
        }

        private void PlacePrefabAtPosition(GameObject prefab, Vector3 placePosition)
        {
            if (!prefab) return;

            try
            {
                Undo.IncrementCurrentGroup();
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                // Get the currently selected GameObject in the hierarchy
                GameObject selectedParent = Selection.activeGameObject;

                // Check if the selected parent is a prefab instance - if so, don't parent to it
                if (selectedParent != null && PrefabUtility.IsPartOfPrefabInstance(selectedParent))
                {
                    // Don't parent to prefab instances to avoid nesting prefabs
                    instance.transform.position = placePosition;
                    Debug.Log(
                        $"Placed {prefab.name} at world position {placePosition} (avoided parenting to prefab instance)");
                }
                else if (selectedParent != null)
                {
                    // Safe to parent to non-prefab GameObject
                    Undo.SetTransformParent(instance.transform, selectedParent.transform,
                        $"Parent {prefab.name} to {selectedParent.name}");

                    // Convert world position to local position relative to the parent
                    Vector3 localPosition = selectedParent.transform.InverseTransformPoint(placePosition);
                    instance.transform.localPosition = localPosition;

                    Debug.Log(
                        $"Placed {prefab.name} as child of {selectedParent.name} at local position {localPosition}");
                }
                else
                {
                    // No parent selected, place at world position as before
                    instance.transform.position = placePosition;
                    Debug.Log($"Placed {prefab.name} at world position {placePosition}");
                }

                Undo.RegisterCreatedObjectUndo(instance, $"Place {prefab.name}");
                // Don't auto-select the new instance to avoid it becoming the new parent
                // Selection.activeGameObject = instance;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to place prefab {prefab.name}: {e.Message}");
            }
        }

        private Vector3 GetMouseWorldPosition(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return ray.origin;
        }

        private Vector3 SnapToGrid(Vector3 snapPosition)
        {
            if (_settings.pixelsPerUnit <= 0) return snapPosition;

            float ppu = _settings.pixelsPerUnit;
            snapPosition.x = Mathf.Round(snapPosition.x * ppu) / ppu;
            snapPosition.y = Mathf.Round(snapPosition.y * ppu) / ppu;
            snapPosition.z = 0;

            return snapPosition;
        }

        private void HandleKeyboardShortcuts()
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;

            if (e.control || e.command)
            {
                switch (e.keyCode)
                {
                    case KeyCode.F:
                        GUI.FocusControl("SearchField");
                        e.Use();
                        break;
                    case KeyCode.R:
                        LoadPrefabs();
                        e.Use();
                        break;
                    case KeyCode.Escape:
                        if (_placementMode)
                        {
                            ExitPlacementMode();
                            e.Use();
                        }

                        break;
                }
            }
        }

        #endregion

        #region Data Management

        private void LoadPrefabs()
        {
            try
            {
                _prefabInfos.Clear();

                if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
                {
                    Debug.LogWarning($"Prefab folder not found: {PrefabFolderPath}");
                    return;
                }

                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolderPath });

                for (int i = 0; i < guids.Length; i++)
                {
                    string guid = guids[i];
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (prefab != null)
                    {
                        _prefabInfos.Add(new PrefabInfo(prefab));
                    }

                    // Show progress for large collections
                    if (i % 50 == 0)
                    {
                        EditorUtility.DisplayProgressBar("Loading Prefabs", $"Processing {i}/{guids.Length}",
                            (float)i / guids.Length);
                    }
                }

                EditorUtility.ClearProgressBar();

                ApplyFilters();
                StartPreviewLoading();

                Debug.Log($"Loaded {_prefabInfos.Count} prefabs from {PrefabFolderPath}");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Failed to load prefabs: {e.Message}");
            }
        }

        private void ApplyFilters()
        {
            try
            {
                _filteredPrefabs.Clear();
                string searchLower = _settings.searchFilter.ToLowerInvariant();

                foreach (PrefabInfo prefabInfo in _prefabInfos)
                {
                    if (!prefabInfo.isValid || prefabInfo.prefab == null) continue;

                    // Search filter
                    if (!string.IsNullOrEmpty(_settings.searchFilter) &&
                        !prefabInfo.name.ToLowerInvariant().Contains(searchLower))
                    {
                        continue;
                    }

                    // Category filter
                    if (MatchesFilter(prefabInfo))
                    {
                        _filteredPrefabs.Add(prefabInfo);
                    }
                }

                // Sort by usage frequency and name
                _filteredPrefabs.Sort((a, b) =>
                {
                    int usageA = _prefabUsageCount.GetValueOrDefault(a.prefab, 0);
                    int usageB = _prefabUsageCount.GetValueOrDefault(b.prefab, 0);

                    if (usageA != usageB)
                        return usageB.CompareTo(usageA);

                    return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to apply filters: {e.Message}");
            }
        }

        private bool MatchesFilter(PrefabInfo prefabInfo)
        {
            return _settings.filterType switch
            {
                FilterType.All => true,
                FilterType.Enemies => prefabInfo.category == FilterType.Enemies,
                FilterType.Containers => prefabInfo.category == FilterType.Containers,
                FilterType.Others => prefabInfo.category == FilterType.Others,
                _ => true
            };
        }

        private void LoadUsageData()
        {
            try
            {
                _prefabUsageCount.Clear();
                string data = EditorPrefs.GetString(UsageDataKey, "");
                if (string.IsNullOrEmpty(data)) return;

                foreach (string entry in data.Split(';'))
                {
                    if (string.IsNullOrEmpty(entry)) continue;

                    string[] parts = entry.Split(':');
                    if (parts.Length != 2) continue;

                    string path = AssetDatabase.GUIDToAssetPath(parts[0]);
                    if (string.IsNullOrEmpty(path)) continue;

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && int.TryParse(parts[1], out int count))
                    {
                        _prefabUsageCount[prefab] = count;
                    }
                }

                UpdateFrequentPrefabs();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load usage data: {e.Message}");
            }
        }

        private void SaveUsageData()
        {
            try
            {
                var validEntries = _prefabUsageCount
                    .Where(kvp => kvp.Key != null && AssetDatabase.Contains(kvp.Key))
                    .Select(kvp =>
                    {
                        string path = AssetDatabase.GetAssetPath(kvp.Key);
                        string guid = AssetDatabase.AssetPathToGUID(path);
                        return $"{guid}:{kvp.Value}";
                    });

                string data = string.Join(";", validEntries);
                EditorPrefs.SetString(UsageDataKey, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save usage data: {e.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                string json = EditorPrefs.GetString(SettingsKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    _settings = JsonUtility.FromJson<WindowSettings>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load settings: {e.Message}");
                _settings = new WindowSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(_settings);
                EditorPrefs.SetString(SettingsKey, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save settings: {e.Message}");
            }
        }

        private void IncrementPrefabUsage(GameObject prefab)
        {
            if (prefab == null) return;

            _prefabUsageCount.TryGetValue(prefab, out int count);
            _prefabUsageCount[prefab] = count + 1;

            UpdateFrequentPrefabs();
            EditorApplication.delayCall += SaveUsageData;
        }

        private void UpdateFrequentPrefabs()
        {
            _frequentPrefabs.Clear();
            var sortedPrefabs = _prefabUsageCount
                .Where(kvp => kvp.Key != null)
                .OrderByDescending(kvp => kvp.Value)
                .Take(MaxFrequentPrefabs)
                .Select(kvp => kvp.Key);

            _frequentPrefabs.AddRange(sortedPrefabs);
        }

        #endregion

        #region Preview System

        private Texture2D GetPrefabPreview(GameObject prefab)
        {
            if (prefab == null) return null;

            // Check cache first
            if (_settings.enablePreviewCache && _previewCache.TryGetValue(prefab, out Texture2D cachedPreview))
            {
                // Validate cached preview is still valid
                if (cachedPreview != null && !AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
                {
                    return cachedPreview;
                }

                // Remove invalid cache entry
                _previewCache.Remove(prefab);
            }

            // Try to get preview from Unity's AssetPreview system
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);

            if (preview != null)
            {
                // Process the preview to make background transparent
                Texture2D processedPreview = MakePreviewTransparent(preview);

                // Cache the processed preview
                if (_settings.enablePreviewCache)
                {
                    _previewCache[prefab] = processedPreview;
                }

                return processedPreview;
            }

            // If no preview available, check if it's still loading
            if (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
            {
                // Force refresh to potentially generate preview
                AssetPreview.GetAssetPreview(prefab);

                // Queue for later retry if not already queued
                if (_settings.enablePreviewCache && !_previewLoadQueue.Contains(prefab))
                {
                    _previewLoadQueue.Enqueue(prefab);
                }

                return null;
            }

            // Try to generate a mini preview from the prefab's renderer
            return GenerateCustomPreview(prefab);
        }

        private Texture2D MakePreviewTransparent(Texture2D originalTexture)
        {
            if (originalTexture == null) return null;

            try
            {
                // Create a new readable texture
                Texture2D readableTexture =
                    new(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

                // Create a RenderTexture to copy the original texture
                RenderTexture renderTexture = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height,
                    0, RenderTextureFormat.ARGB32);

                Graphics.Blit(originalTexture, renderTexture);

                // Read the pixels from the RenderTexture
                RenderTexture.active = renderTexture;
                readableTexture.ReadPixels(new Rect(0, 0, originalTexture.width, originalTexture.height), 0, 0);
                readableTexture.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexture);

                // Process pixels to make background transparent
                var pixels = readableTexture.GetPixels();
                Color backgroundColor = pixels[0]; // Assume top-left corner is background

                for (int i = 0; i < pixels.Length; i++)
                {
                    Color pixel = pixels[i];

                    // If pixel is very similar to background color, make it transparent
                    float colorDistance = Vector3.Distance(
                        new Vector3(pixel.r, pixel.g, pixel.b),
                        new Vector3(backgroundColor.r, backgroundColor.g, backgroundColor.b)
                    );

                    if (colorDistance < 0.1f) // Threshold for background detection
                    {
                        pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 0f); // Make transparent
                    }
                    else
                    {
                        // Keep original alpha or make slightly transparent for blending
                        pixels[i] = new Color(pixel.r, pixel.g, pixel.b, pixel.a * 0.9f);
                    }
                }

                readableTexture.SetPixels(pixels);
                readableTexture.Apply();

                return readableTexture;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to make preview transparent: {e.Message}");
                return originalTexture; // Return original if processing fails
            }
        }

        private Texture2D GenerateCustomPreview(GameObject prefab)
        {
            try
            {
                // Get all renderers in the prefab
                var renderers = prefab.GetComponentsInChildren<Renderer>();
                if (renderers == null || renderers.Length == 0)
                    return null;

                // Create a simple colored texture based on the prefab's main material
                Renderer mainRenderer = renderers[0];
                if (mainRenderer.sharedMaterial != null)
                {
                    Color materialColor = Color.white;

                    // Try to get color from various shader properties
                    Material material = mainRenderer.sharedMaterial;
                    if (material.HasProperty(Color1))
                    {
                        materialColor = material.GetColor(Color1);
                    }
                    else if (material.HasProperty(BaseColor))
                    {
                        materialColor = material.GetColor(BaseColor);
                    }
                    else if (material.HasProperty(MainTex) && material.mainTexture)
                    {
                        // Use a sample from the main texture
                        Texture2D texture = material.mainTexture as Texture2D;
                        if (texture && texture.isReadable)
                        {
                            materialColor = texture.GetPixel(texture.width / 2, texture.height / 2);
                        }
                    }

                    // Create a simple 64x64 preview texture
                    Texture2D customPreview = new(64, 64, TextureFormat.RGBA32, false);
                    var pixels = new Color[64 * 64];

                    // Create a simple gradient/pattern
                    for (int y = 0; y < 64; y++)
                    {
                        for (int x = 0; x < 64; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                            float alpha = Mathf.Clamp01(1.0f - distance / 32.0f);
                            pixels[y * 64 + x] = new Color(materialColor.r, materialColor.g, materialColor.b,
                                alpha * materialColor.a);
                        }
                    }

                    customPreview.SetPixels(pixels);
                    customPreview.Apply();

                    // Cache the custom preview
                    if (_settings.enablePreviewCache)
                    {
                        _previewCache[prefab] = customPreview;
                    }

                    return customPreview;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to generate custom preview for {prefab.name}: {e.Message}");
            }

            return null;
        }

        private void ClearPreviews()
        {
            foreach (Texture2D preview in _previewCache.Values)
            {
                if (preview != null)
                {
                    DestroyImmediate(preview);
                }
            }

            _previewCache.Clear();
            _previewLoadQueue.Clear();
        }

        #endregion

    }
}

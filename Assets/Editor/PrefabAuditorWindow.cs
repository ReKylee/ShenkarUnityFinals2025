using System;
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
            public DateTime lastModified;

            public PrefabInfo(GameObject prefab)
            {
                this.prefab = prefab;
                name = prefab.name;
                path = AssetDatabase.GetAssetPath(prefab);
                guid = AssetDatabase.AssetPathToGUID(path);
                category = DetermineCategory(prefab);
                isValid = prefab != null;

                if (File.Exists(path))
                {
                    FileInfo fileInfo = new(path);
                    fileSize = fileInfo.Length;
                    lastModified = fileInfo.LastWriteTime;
                }
            }

            private static FilterType DetermineCategory(GameObject prefab)
            {
                if ((1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0)
                    return FilterType.Enemies;

                if (prefab.layer == LayerMask.NameToLayer("Collectibles") &&
                    prefab.GetComponent<PowerUpContainer>() != null)
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
            foreach (PrefabInfo prefabInfo in _prefabInfos)
            {
                if (prefabInfo.prefab != null && !_previewCache.ContainsKey(prefabInfo.prefab))
                {
                    _previewLoadQueue.Enqueue(prefabInfo.prefab);
                }
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

            if (currentEvent.type == EventType.MouseMove || currentEvent.type == EventType.Repaint)
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
            Vector3 pivotOffset = GetPrefabPivotOffset(_selectedPrefabForPlacement, prefabBounds);
            
            // If bounds are too small or invalid, use default size
            if (worldSize.magnitude < 0.1f)
            {
                worldSize = Vector3.one * 0.5f;
                pivotOffset = Vector3.zero;
            }

            // Scale the world size to compensate for asset preview padding BEFORE any calculations
            // Unity's asset previews have internal padding, so we scale up the world size to compensate
            worldSize *= 1.2f; // Scale up by 20% to account for internal padding

            // Apply grid snapping to the preview position if enabled
            Vector3 previewWorldPos = worldPos;
            if (Event.current.shift && _settings.pixelsPerUnit > 0)
            {
                previewWorldPos = SnapToGrid(worldPos);
            }

            // Convert the snapped world position back to screen coordinates for the preview
            Vector2 previewScreenPos = mousePos;
            Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
            if (sceneCamera != null)
            {
                // Account for pivot offset when converting to screen coordinates
                Vector3 pivotWorldPos = previewWorldPos + pivotOffset;
                Vector3 screenPoint = sceneCamera.WorldToScreenPoint(pivotWorldPos);
                // Convert Unity screen coordinates to GUI coordinates
                previewScreenPos = new Vector2(screenPoint.x, SceneView.lastActiveSceneView.position.height - screenPoint.y);
            }

            // Calculate accurate GUI preview size based on actual world size and camera
            float screenSize = 80f; // Base size in pixels
            
            if (sceneCamera != null)
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
                Rect previewRect = new Rect(
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
                EditorGUI.DrawRect(new Rect(center.x - crosshairSize, center.y - 0.5f, crosshairSize * 2, 1), crosshairColor);
                EditorGUI.DrawRect(new Rect(center.x - 0.5f, center.y - crosshairSize, 1, crosshairSize * 2), crosshairColor);
                
                // Show prefab info below the preview
                string infoText = _selectedPrefabForPlacement.name;
                if (Event.current.shift && _settings.pixelsPerUnit > 0)
                {
                    infoText += " (Grid Snap)";
                }
                
                // Add size info for debugging
                infoText += $" | Size: {worldSize.x:F1}x{worldSize.y:F1} | Screen: {screenSize:F0}px";
                
                GUIContent infoContent = new GUIContent(infoText);
                Vector2 infoSize = EditorStyles.miniLabel.CalcSize(infoContent);
                Rect infoRect = new Rect(
                    previewScreenPos.x - infoSize.x * 0.5f, 
                    previewRect.y + previewRect.height + 5, 
                    infoSize.x, 
                    infoSize.y
                );
                Color backgroundColor = new Color(0, 0, 0, 0.7f);
                // Draw info background
                EditorGUI.DrawRect(new Rect(infoRect.x - 2, infoRect.y - 1, infoRect.width + 4, infoRect.height + 2), backgroundColor);
                
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
                foreach (var renderer in renderers)
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
                foreach (var collider in colliders)
                {
                    bounds.Encapsulate(collider.bounds);
                }
                return bounds;
            }
            
            // Try to get bounds from colliders 2D
            var colliders2D = prefab.GetComponentsInChildren<Collider2D>();
            if (colliders2D != null && colliders2D.Length > 0)
            {
                Bounds bounds = colliders2D[0].bounds;
                foreach (var collider in colliders2D)
                {
                    bounds.Encapsulate(collider.bounds);
                }
                return bounds;
            }
            
            // Fallback to transform bounds
            return new Bounds(prefab.transform.position, Vector3.one);
        }

        private Vector3 GetPrefabPivotOffset(GameObject prefab, Bounds bounds)
        {
            if (prefab == null) return Vector3.zero;
            
            // Get the prefab's transform position (which represents its pivot)
            Vector3 pivotPosition = prefab.transform.position;
            
            // Calculate the offset from the bounds center to the pivot position
            Vector3 pivotOffset = pivotPosition - bounds.center;
            
            // For 2D sprites, we might need to consider the sprite's pivot settings
            var spriteRenderer = prefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                
                // Get the sprite's pivot in world space
                Vector2 spritePivot = sprite.pivot;
                Vector2 spriteSize = sprite.rect.size;
                Vector2 pixelsPerUnit = Vector2.one * sprite.pixelsPerUnit;
                
                // Convert pivot from pixel coordinates to normalized coordinates (0-1)
                Vector2 normalizedPivot = new Vector2(spritePivot.x / spriteSize.x, spritePivot.y / spriteSize.y);
                
                // Calculate the offset based on sprite pivot
                Vector3 spriteOffset = new Vector3(
                    (0.5f - normalizedPivot.x) * bounds.size.x,
                    (0.5f - normalizedPivot.y) * bounds.size.y,
                    0f
                );
                
                return spriteOffset;
            }
            
            // For non-sprite objects, return the calculated pivot offset
            return pivotOffset;
        }

        private void DrawRadialMenu()
        {
            Handles.BeginGUI();

            Vector2 center = _radialMenuPosition;
            float elapsedTime = (float)EditorApplication.timeSinceStartup - _radialMenuOpenTime;
            float growProgress = Mathf.Clamp01(elapsedTime / RadialMenuAnimationTime);
            float radius = RadialMenuRadius * Mathf.SmoothStep(0, 1, growProgress);

            // Background with gradient
            Handles.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            Handles.color = Color.white;

            var prefabsToShow = _frequentPrefabs.Count > 0
                ? _frequentPrefabs
                : _filteredPrefabs.Take(MaxFrequentPrefabs).Select(p => p.prefab).ToList();

            int prefabCount = Mathf.Min(prefabsToShow.Count, MaxFrequentPrefabs);

            for (int i = 0; i < prefabCount; i++)
            {
                GameObject prefab = prefabsToShow[i];
                if (prefab == null) continue;

                float angle = i / (float)prefabCount * 360f - 90f;
                Vector2 direction = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 itemPos = center + direction * (radius * 0.75f);
                
                bool isHovered = i == _hoveredRadialIndex;
                float iconSize = isHovered ? 55f : 45f;
                
                // Enhanced highlighting for hovered item
                if (isHovered)
                {
                    // Draw pulsing highlight background
                    float pulseScale = 1f + Mathf.Sin((float)EditorApplication.timeSinceStartup * 8f) * 0.1f;
                    Handles.color = new Color(0, 1, 1, 0.6f); // Cyan with transparency
                    Handles.DrawSolidDisc(itemPos, Vector3.forward, iconSize * 0.7f * pulseScale);
                    
                    // Draw bright border
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(itemPos, Vector3.forward, iconSize * 0.7f);
                    Handles.color = Color.white;

                    // Show enhanced name display
                    GUIContent nameContent = new GUIContent(prefab.name);
                    Vector2 nameSize = EditorStyles.boldLabel.CalcSize(nameContent);
                    Rect nameRect = new Rect(
                        center.x - nameSize.x * 0.5f, 
                        center.y + radius + 10, 
                        nameSize.x, 
                        nameSize.y
                    );
                    
                    // Draw name background
                    EditorGUI.DrawRect(new Rect(nameRect.x - 4, nameRect.y - 2, nameRect.width + 8, nameRect.height + 4), 
                        new Color(0, 0, 0, 0.8f));
                    
                    // Draw name with highlight color
                    GUI.color = Color.cyan;
                    GUI.Label(nameRect, nameContent, EditorStyles.boldLabel);
                    GUI.color = Color.white;
                }
                else
                {
                    // Subtle highlight for non-hovered items
                    Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
                    Handles.DrawSolidDisc(itemPos, Vector3.forward, iconSize * 0.6f);
                    Handles.color = Color.white;
                }

                // Draw icon rect
                Rect iconRect = new Rect(
                    itemPos.x - iconSize * 0.5f, 
                    itemPos.y - iconSize * 0.5f, 
                    iconSize, 
                    iconSize
                );

                // Draw preview with enhanced border for hovered items
                Texture2D preview = GetPrefabPreview(prefab);
                if (preview != null)
                {
                    if (isHovered)
                    {
                        // Draw glowing border for hovered item
                        Rect borderRect = new Rect(iconRect.x - 2, iconRect.y - 2, iconRect.width + 4, iconRect.height + 4);
                        EditorGUI.DrawRect(borderRect, Color.cyan);
                    }
                    GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit);
                }
                else
                {
                    // Draw placeholder with proper highlighting
                    Color placeholderColor = isHovered ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.3f, 0.3f, 0.3f);
                    EditorGUI.DrawRect(iconRect, placeholderColor);
                    
                    // Draw loading text
                    GUI.color = isHovered ? Color.white : Color.gray;
                    GUI.Label(iconRect, "...", EditorStyles.centeredGreyMiniLabel);
                    GUI.color = Color.white;
                }
            }

            Handles.EndGUI();
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
                Undo.RegisterCreatedObjectUndo(instance, $"Place {prefab.name}");
                instance.transform.position = placePosition;
                Selection.activeGameObject = instance;

                Debug.Log($"Placed {prefab.name} at {placePosition}");
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
                else
                {
                    // Remove invalid cache entry
                    _previewCache.Remove(prefab);
                }
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
                Texture2D readableTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
                
                // Create a RenderTexture to copy the original texture
                RenderTexture renderTexture = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(originalTexture, renderTexture);
                
                // Read the pixels from the RenderTexture
                RenderTexture.active = renderTexture;
                readableTexture.ReadPixels(new Rect(0, 0, originalTexture.width, originalTexture.height), 0, 0);
                readableTexture.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexture);

                // Process pixels to make background transparent
                Color[] pixels = readableTexture.GetPixels();
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
            catch (System.Exception e)
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
                var mainRenderer = renderers[0];
                if (mainRenderer.sharedMaterial != null)
                {
                    Color materialColor = Color.white;
                    
                    // Try to get color from various shader properties
                    var material = mainRenderer.sharedMaterial;
                    if (material.HasProperty("_Color"))
                        materialColor = material.GetColor("_Color");
                    else if (material.HasProperty("_BaseColor"))
                        materialColor = material.GetColor("_BaseColor");
                    else if (material.HasProperty("_MainTex") && material.mainTexture != null)
                    {
                        // Use a sample from the main texture
                        var texture = material.mainTexture as Texture2D;
                        if (texture != null && texture.isReadable)
                        {
                            materialColor = texture.GetPixel(texture.width / 2, texture.height / 2);
                        }
                    }

                    // Create a simple 64x64 preview texture
                    Texture2D customPreview = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                    Color[] pixels = new Color[64 * 64];
                    
                    // Create a simple gradient/pattern
                    for (int y = 0; y < 64; y++)
                    {
                        for (int x = 0; x < 64; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                            float alpha = Mathf.Clamp01(1.0f - distance / 32.0f);
                            pixels[y * 64 + x] = new Color(materialColor.r, materialColor.g, materialColor.b, alpha * materialColor.a);
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
            foreach (var preview in _previewCache.Values)
            {
                if (preview != null)
                {
                    DestroyImmediate(preview);
                }
            }
            _previewCache.Clear();
            _previewLoadQueue.Clear();
        }

        private void RefreshPreviewCache()
        {
            // Clear existing cache
            ClearPreviews();
            
            // Restart preview loading
            StartPreviewLoading();
            
            // Force repaint
        }

        #endregion

    }
}

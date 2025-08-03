using System.Collections.Generic;
using System.Linq;
using PowerUps.Container;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PrefabAuditorWindow : EditorWindow
    {
        #region Constants
        private const int MaxFrequentPrefabs = 8;
        private const float RadialMenuRadius = 100f;
        private const float RadialMenuItemRadius = 30f;
        private const float RadialMenuAnimationTime = 0.5f;
        #endregion

        #region Private Fields
        private readonly List<GameObject> _prefabs = new();
        private readonly List<GameObject> _filteredPrefabs = new();
        private readonly List<GameObject> _frequentPrefabs = new();
        private readonly Dictionary<GameObject, int> _prefabUsageCount = new();
        private readonly Dictionary<GameObject, Texture2D> _previewCache = new();

        private string _searchFilter = "";
        private FilterType _filterType = FilterType.All;
        private bool _useGridView = true;
        private Vector2 _scrollPosition;

        private bool _placementMode;
        private GameObject _selectedPrefabForPlacement;
        private float _ppu = 16f;

        private bool _showRadialMenu;
        private Vector2 _radialMenuPosition;
        private float _radialMenuOpenTime;

        private bool _autoRefresh = true;
        private bool _showAdvancedSettings;
        private GameObject _defaultDropPrefab;

        private static GUIStyle _headerStyle;
        private static GUIStyle _cardStyle;
        #endregion

        #region Unity Methods
        [MenuItem("Tools/Level Auditor & Prefab Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabAuditorWindow>("Level Auditor");
            window.minSize = new Vector2(500, 600);
        }

        private void OnEnable()
        {
            LoadPrefabs();
            LoadUsageData();
            UpdateFrequentPrefabs();
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            SaveUsageData();
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= OnEditorUpdate;
            ClearPreviews();
        }

        private void OnEditorUpdate()
        {
            if (_showRadialMenu)
            {
                SceneView.lastActiveSceneView?.Repaint();
            }
            Repaint();
        }

        private void OnGUI()
        {
            InitializeStyles();

            DrawHeader();
            DrawFiltersAndSearch();
            DrawQuickSettings();
            DrawPrefabsList();
            DrawFooter();

            if (_autoRefresh && GUI.changed)
            {
                ApplyFilters();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            var currentEvent = Event.current;

            // Prioritize radial menu input to ensure it's not consumed by other controls.
            HandleRadialMenu(currentEvent, sceneView);

            // If the radial menu used the event, don't process placement mode.
            if (currentEvent.type == EventType.Used) return;

            HandlePlacementMode(currentEvent, sceneView);
        }
        #endregion

        #region UI Drawing
        private void InitializeStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };

            _cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 2, 2)
            };
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField("🎮 Level Auditor & Prefab Manager", _headerStyle);
            EditorGUILayout.LabelField($"Found {_prefabs.Count} prefabs | Showing {_filteredPrefabs.Count}", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80)))
            {
                LoadPrefabs();
            }
            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            if (_placementMode && GUILayout.Button("❌ Exit Placement", GUILayout.Width(120)))
            {
                ExitPlacementMode();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFiltersAndSearch()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField("🔍 Filters & Search", EditorStyles.boldLabel);

            // Search
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            var newSearch = EditorGUILayout.TextField(_searchFilter);
            if (newSearch != _searchFilter)
            {
                _searchFilter = newSearch;
                ApplyFilters();
            }
            EditorGUILayout.EndHorizontal();

            // Filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
            var newFilter = (FilterType)EditorGUILayout.EnumPopup(_filterType);
            if (newFilter != _filterType)
            {
                _filterType = newFilter;
                ApplyFilters();
            }
            EditorGUILayout.EndHorizontal();

            // View
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View:", GUILayout.Width(50));
            if (GUILayout.Toggle(_useGridView, "Grid", "Button", GUILayout.Width(60))) _useGridView = true;
            if (GUILayout.Toggle(!_useGridView, "List", "Button", GUILayout.Width(60))) _useGridView = false;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickSettings()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "⚙️ Quick Settings", true, EditorStyles.foldout);
            if (_showAdvancedSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Grid Snap (PPU):", GUILayout.Width(100));
                _ppu = EditorGUILayout.FloatField(_ppu, GUILayout.Width(60));
                EditorGUILayout.LabelField("Default Drop:", GUILayout.Width(80));
                _defaultDropPrefab = (GameObject)EditorGUILayout.ObjectField(_defaultDropPrefab, typeof(GameObject), false);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabsList()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField("📦 Prefabs", EditorStyles.boldLabel);

            if (_filteredPrefabs.Count == 0)
            {
                EditorGUILayout.HelpBox("No prefabs match current filters.", MessageType.Info);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
                if (_useGridView)
                {
                    DrawPrefabsGrid();
                }
                else
                {
                    DrawPrefabsListItems();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabsListItems()
        {
            foreach (var prefab in _filteredPrefabs)
            {
                DrawPrefabListItem(prefab);
            }
        }

        private void DrawPrefabListItem(GameObject prefab)
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.BeginHorizontal();

            var preview = GetPrefabPreview(prefab);
            if (preview != null)
            {
                GUILayout.Label(preview, GUILayout.Width(48), GUILayout.Height(48));
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(prefab.name, EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("🖱️ Place", GUILayout.Width(60))) StartPlacementMode(prefab);
            if (GUILayout.Button("📍 Instant", GUILayout.Width(60))) PlacePrefabAtPosition(prefab, Vector3.zero);
            if (GUILayout.Button("🔍 Select", GUILayout.Width(60))) Selection.activeObject = prefab;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabsGrid()
        {
            var columns = Mathf.Max(1, (int)(position.width / 150));
            var rows = Mathf.CeilToInt((float)_filteredPrefabs.Count / columns);

            for (var row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (var col = 0; col < columns; col++)
                {
                    var index = row * columns + col;
                    if (index >= _filteredPrefabs.Count) break;

                    var prefab = _filteredPrefabs[index];
                    DrawPrefabGridItem(prefab);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPrefabGridItem(GameObject prefab)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            var preview = GetPrefabPreview(prefab);
            var rect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
            if (preview != null)
            {
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
            }

            GUILayout.Label(prefab.name, EditorStyles.miniLabel, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Place", GUILayout.Height(30)))
            {
                StartPlacementMode(prefab);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Undo", GUILayout.Width(80))) Undo.PerformUndo();
            if (GUILayout.Button("Redo", GUILayout.Width(80))) Undo.PerformRedo();
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Scene GUI Handling
        private void HandlePlacementMode(Event currentEvent, SceneView sceneView)
        {
            if (!_placementMode || _selectedPrefabForPlacement == null) return;

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            var worldPos = GetMouseWorldPosition(currentEvent);
            if (currentEvent.shift && _ppu > 0)
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
            var preview = GetPrefabPreview(_selectedPrefabForPlacement);
            if (preview != null)
            {
                Handles.BeginGUI();
                var previewRect = new Rect(mousePos.x + 15, mousePos.y + 15, 100, 100);
                GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit);
                Handles.EndGUI();
            }
            Handles.color = Color.green;
            Handles.DrawWireCube(worldPos, Vector3.one);
        }

        private void HandleRadialMenu(Event currentEvent, SceneView sceneView)
        {
            // Use the raw event type for more reliable capture
            var eventType = currentEvent.GetTypeForControl(GUIUtility.GetControlID(FocusType.Passive));

            if (eventType == EventType.MouseDown && currentEvent.button == 1)
            {
                _showRadialMenu = true;
                _radialMenuPosition = currentEvent.mousePosition;
                _radialMenuOpenTime = Time.realtimeSinceStartup;
                currentEvent.Use();
                return;
            }

            if (!_showRadialMenu) return;

            DrawRadialMenu();

            if (eventType == EventType.MouseUp && currentEvent.button == 1)
            {
                var selectedPrefab = GetPrefabFromRadialMenu(currentEvent.mousePosition);
                if (selectedPrefab != null)
                {
                    StartPlacementMode(selectedPrefab);
                }
                _showRadialMenu = false;
                currentEvent.Use();
            }
            else if (eventType == EventType.MouseDrag && currentEvent.button == 1)
            {
                sceneView.Repaint();
            }
            else if (eventType == EventType.ScrollWheel || (eventType == EventType.MouseDown && currentEvent.button != 1))
            {
                // Dismiss on other mouse actions
                _showRadialMenu = false;
                // Don't use the event, let other controls use it
            }
        }

        private void DrawRadialMenu()
        {
            Handles.BeginGUI();

            var center = _radialMenuPosition;
            var elapsedTime = Time.realtimeSinceStartup - _radialMenuOpenTime;
            var growProgress = Mathf.Clamp01(elapsedTime / RadialMenuAnimationTime);
            var radius = RadialMenuRadius * growProgress;

            // Background
            Handles.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            Handles.color = Color.white;
            Handles.DrawWireDisc(center, Vector3.forward, radius);

            var prefabsToShow = _frequentPrefabs.Count > 0 ? _frequentPrefabs : _filteredPrefabs;
            var prefabCount = Mathf.Min(prefabsToShow.Count, MaxFrequentPrefabs);
            var mousePosition = Event.current.mousePosition;

            for (var i = 0; i < prefabCount; i++)
            {
                var prefab = prefabsToShow[i];
                if (prefab == null) continue;

                var angle = i / (float)prefabCount * 360f - 90f;
                var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                var itemPos = center + direction * (radius * 0.7f);
                var iconRect = new Rect(itemPos.x - 20, itemPos.y - 20, 40, 40);

                var isHovered = iconRect.Contains(mousePosition);
                if (isHovered)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawSolidDisc(itemPos, Vector3.forward, 25f);
                    Handles.color = Color.white;
                    GUI.Label(new Rect(center.x - 50, center.y + radius + 10, 100, 20), prefab.name, EditorStyles.boldLabel);
                }

                var preview = GetPrefabPreview(prefab);
                if (preview != null)
                {
                    GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit);
                }
            }

            Handles.EndGUI();
        }
        #endregion

        #region Core Logic
        private void StartPlacementMode(GameObject prefab)
        {
            _placementMode = true;
            _selectedPrefabForPlacement = prefab;
            IncrementPrefabUsage(prefab);
            GUI.FocusControl(null); // Remove focus from window UI
        }

        private void ExitPlacementMode()
        {
            _placementMode = false;
            _selectedPrefabForPlacement = null;
        }

        private void PlacePrefabAtPosition(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return;
            Undo.IncrementCurrentGroup();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, $"Place {prefab.name}");
            instance.transform.position = position;
            Selection.activeGameObject = instance;
        }

        private GameObject GetPrefabFromRadialMenu(Vector2 mousePosition)
        {
            if (Vector2.Distance(mousePosition, _radialMenuPosition) < 20) return null; // Deadzone in center

            var prefabsToShow = _frequentPrefabs.Count > 0 ? _frequentPrefabs : _filteredPrefabs;
            var prefabCount = Mathf.Min(prefabsToShow.Count, MaxFrequentPrefabs);
            if (prefabCount == 0) return null;

            var angle = Vector2.SignedAngle(Vector2.up, mousePosition - _radialMenuPosition);
            if (angle < 0) angle += 360;

            var segmentAngle = 360f / prefabCount;
            var index = Mathf.FloorToInt((angle + segmentAngle / 2f) % 360 / segmentAngle);

            return index < prefabsToShow.Count ? prefabsToShow[index] : null;
        }

        private Vector3 GetMouseWorldPosition(Event e)
        {
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            // Assuming a 2D setup, raycast against XY plane at Z=0
            var plane = new Plane(Vector3.forward, Vector3.zero);
            if (plane.Raycast(ray, out var distance))
            {
                return ray.GetPoint(distance);
            }
            return ray.origin; // Fallback
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            if (_ppu <= 0) return position;
            position.x = Mathf.Round(position.x * _ppu) / _ppu;
            position.y = Mathf.Round(position.y * _ppu) / _ppu;
            position.z = 0;
            return position;
        }
        #endregion

        #region Data Handling
        private void LoadPrefabs()
        {
            _prefabs.Clear();
            ClearPreviews();
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    _prefabs.Add(prefab);
                }
            }
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredPrefabs.Clear();
            var searchLower = _searchFilter.ToLower();

            foreach (var prefab in _prefabs)
            {
                if (!string.IsNullOrEmpty(_searchFilter) && !prefab.name.ToLower().Contains(searchLower))
                {
                    continue;
                }

                if (MatchesFilter(prefab))
                {
                    _filteredPrefabs.Add(prefab);
                }
            }
        }

        private bool MatchesFilter(GameObject prefab)
        {
            return _filterType switch
            {
                FilterType.All => true,
                FilterType.Enemies => IsOnLayer(prefab, "Enemy"),
                FilterType.Containers => IsOnLayer(prefab, "Collectibles") && prefab.GetComponent<PowerUpContainer>() != null,
                FilterType.Others => !IsOnLayer(prefab, "Enemy") && (!IsOnLayer(prefab, "Collectibles") || prefab.GetComponent<PowerUpContainer>() == null),
                _ => true,
            };
        }

        private bool IsOnLayer(GameObject obj, string layerName)
        {
            return obj.layer == LayerMask.NameToLayer(layerName);
        }

        private void LoadUsageData()
        {
            _prefabUsageCount.Clear();
            var data = EditorPrefs.GetString(nameof(PrefabAuditorWindow) + "_UsageData", "");
            if (string.IsNullOrEmpty(data)) return;

            foreach (var entry in data.Split(';'))
            {
                var parts = entry.Split(':');
                if (parts.Length != 2) continue;

                var path = AssetDatabase.GUIDToAssetPath(parts[0]);
                if (string.IsNullOrEmpty(path)) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && int.TryParse(parts[1], out var count))
                {
                    _prefabUsageCount[prefab] = count;
                }
            }
        }

        private void SaveUsageData()
        {
            var validEntries = _prefabUsageCount
                .Where(kvp => kvp.Key != null && AssetDatabase.Contains(kvp.Key))
                .Select(kvp =>
                {
                    var path = AssetDatabase.GetAssetPath(kvp.Key);
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    return $"{guid}:{kvp.Value}";
                });

            var data = string.Join(";", validEntries);
            EditorPrefs.SetString(nameof(PrefabAuditorWindow) + "_UsageData", data);
        }

        private void IncrementPrefabUsage(GameObject prefab)
        {
            if (prefab == null) return;
            _prefabUsageCount.TryGetValue(prefab, out var count);
            _prefabUsageCount[prefab] = count + 1;
            UpdateFrequentPrefabs();
            SaveUsageData();
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

        #region Preview Handling
        private Texture2D GetPrefabPreview(GameObject prefab)
        {
            if (prefab == null) return null;
            if (_previewCache.TryGetValue(prefab, out var preview) && preview != null)
            {
                return preview;
            }

            preview = AssetPreview.GetAssetPreview(prefab);
            if (preview == null)
            {
                preview = AssetPreview.GetMiniThumbnail(prefab);
            }
            if (preview == null)
            {
                preview = CreatePlaceholderTexture();
            }

            _previewCache[prefab] = preview;
            return preview;
        }

        private Texture2D CreatePlaceholderTexture()
        {
            var tex = new Texture2D(64, 64);
            // Simple placeholder graphic
            return tex;
        }

        private void ClearPreviews()
        {
            foreach (var tex in _previewCache.Values)
            {
                if (tex != null) DestroyImmediate(tex);
            }
            _previewCache.Clear();
        }
        #endregion

        private enum FilterType
        {
            All,
            Enemies,
            Containers,
            Others
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using PowerUps.Container;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PrefabAuditorWindow : EditorWindow
    {
        private const int MaxFrequentPrefabs = 8;

        private static GUIStyle _headerStyle;
        private static GUIStyle _cardStyle;
        private readonly List<GameObject> _filteredPrefabs = new();
        private readonly List<GameObject> _frequentPrefabs = new();
        private readonly List<GameObject> _prefabs = new();

        private readonly Dictionary<GameObject, int> _prefabUsageCount = new();

        private readonly Dictionary<GameObject, Texture2D> _previewCache = new();
        private bool _autoRefresh = true;
        private GameObject _defaultDropPrefab;
        private FilterType _filterType = FilterType.All;
        private bool _placementMode;

        private float _ppu = 16f;
        private float _radialMenuCloseTime;

        private float _radialMenuOpenTime;

        private Vector2 _radialMenuPosition;
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private GameObject _selectedPrefabForPlacement;
        private bool _showAdvancedSettings;
        private bool _showRadialMenu;
        private bool _useGridView = true;

        private void OnEnable()
        {
            LoadPrefabs();
            LoadUsageData();
            UpdateFrequentPrefabs();
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.update += UpdateRadialMenuAnimation;
        }

        private void OnDisable()
        {
            SaveUsageData();
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update -= UpdateRadialMenuAnimation;
        }

        private void UpdateRadialMenuAnimation()
        {
            if (_showRadialMenu)
            {
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        private void OnEditorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            InitializeStyles();

            DrawHeader();
            DrawFiltersAndSearch();
            DrawQuickSettings();
            DrawPrefabsList();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Undo", GUILayout.Width(80))) Undo.PerformUndo();
            if (GUILayout.Button("Redo", GUILayout.Width(80))) Undo.PerformRedo();
            EditorGUILayout.EndHorizontal();

            if (_autoRefresh && GUI.changed)
                ApplyFilters();

            HandlePlacementMode();
        }

        [MenuItem("Tools/Level Auditor & Prefab Manager")]
        public static void ShowWindow()
        {
            PrefabAuditorWindow window = GetWindow<PrefabAuditorWindow>("Level Auditor");
            window.minSize = new Vector2(500, 600);
        }

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
            EditorGUILayout.LabelField($"Found {_prefabs.Count} prefabs | Showing {_filteredPrefabs.Count}",
                EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80))) LoadPrefabs();
            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            if (_placementMode && GUILayout.Button("❌ Exit Placement", GUILayout.Width(120)))
            {
                _placementMode = false;
                _selectedPrefabForPlacement = null;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFiltersAndSearch()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField("🔍 Filters & Search", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearch = EditorGUILayout.TextField(_searchFilter);
            if (newSearch != _searchFilter)
            {
                _searchFilter = newSearch;
                ApplyFilters();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
            FilterType newFilter = (FilterType)EditorGUILayout.EnumPopup(_filterType);
            if (newFilter != _filterType)
            {
                _filterType = newFilter;
                ApplyFilters();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View:", GUILayout.Width(50));
            _useGridView = GUILayout.Toggle(_useGridView, "Grid", GUILayout.Width(60));
            _useGridView = !GUILayout.Toggle(!_useGridView, "List", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickSettings()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("⚙️ Quick Settings", EditorStyles.boldLabel);
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Snap (PPU):", GUILayout.Width(100));
            _ppu = EditorGUILayout.FloatField(_ppu, GUILayout.Width(60));
            EditorGUILayout.LabelField("Default Drop:", GUILayout.Width(80));
            _defaultDropPrefab = (GameObject)EditorGUILayout.ObjectField(_defaultDropPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabsList()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField("📦 Prefabs", EditorStyles.boldLabel);
            if (_filteredPrefabs.Count == 0)
            {
                EditorGUILayout.HelpBox("No prefabs match current filters.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
            if (_useGridView) DrawPrefabsGrid();
            else
                foreach (GameObject prefab in _filteredPrefabs)
                    DrawPrefabListItem(prefab);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabListItem(GameObject prefab)
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.BeginHorizontal();
            Texture2D preview = GetPrefabPreview(prefab);
            if (preview != null)
                GUILayout.Label(preview, GUILayout.Width(48), GUILayout.Height(48));

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
            int columns = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth / 150));
            int rows = Mathf.CeilToInt(_filteredPrefabs.Count / (float)columns);
            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index >= _filteredPrefabs.Count) break;
                    GameObject prefab = _filteredPrefabs[index];
                    EditorGUILayout.BeginVertical(GUILayout.Width(120), GUILayout.Height(140));
                    Texture2D preview = GetPrefabPreview(prefab);
                    if (preview != null)
                    {
                        Rect rect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
                        GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUILayout.Space(100);
                    }

                    GUILayout.Label(prefab.name, EditorStyles.miniLabel, GUILayout.Width(120));
                    if (GUILayout.Button("Place", GUILayout.Height(30))) StartPlacementMode(prefab);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private Texture2D GetPrefabPreview(GameObject prefab)
        {
            if (_previewCache.TryGetValue(prefab, out Texture2D preview) && preview != null) return preview;

            // Attempt to fetch the preview texture
            preview = AssetPreview.GetAssetPreview(prefab);

            // Fallback to mini thumbnail if preview is unavailable
            if (!preview)
            {
                preview = AssetPreview.GetMiniThumbnail(prefab);
            }

            // If still unavailable, create a placeholder texture
            if (preview == null)
            {
                preview = new Texture2D(100, 100);
                Color fillColor = Color.yellow;
                var fillPixels = Enumerable.Repeat(fillColor, preview.width * preview.height).ToArray();
                preview.SetPixels(fillPixels);
                preview.Apply();
            }

            _previewCache[prefab] = preview;
            return preview;
        }

        private void StartPlacementMode(GameObject prefab)
        {
            Undo.RecordObject(this, "Select Prefab for Placement");
            _placementMode = true;
            _selectedPrefabForPlacement = prefab;
            IncrementPrefabUsage(prefab);
        }

        private void HandlePlacementMode()
        {
            if (_placementMode && _selectedPrefabForPlacement == null) _placementMode = false;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (_placementMode && _selectedPrefabForPlacement != null)
            {
                if (e.type == EventType.MouseMove || e.type == EventType.Repaint)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    Vector3 worldPos = ray.origin;
                    worldPos.z = 0;

                    if (e.shift && _ppu > 0)
                    {
                        worldPos.x = Mathf.Round(worldPos.x * _ppu) / _ppu;
                        worldPos.y = Mathf.Round(worldPos.y * _ppu) / _ppu;
                    }

                    Texture2D preview = GetPrefabPreview(_selectedPrefabForPlacement);
                    if (preview != null)
                    {
                        Rect previewRect = new(e.mousePosition.x + 15, e.mousePosition.y + 15, 100, 100);
                        GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit);
                    }

                    sceneView.Repaint();
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    Vector3 worldPos = ray.origin;
                    worldPos.z = 0;

                    if (e.shift && _ppu > 0)
                    {
                        worldPos.x = Mathf.Round(worldPos.x * _ppu) / _ppu;
                        worldPos.y = Mathf.Round(worldPos.y * _ppu) / _ppu;
                    }

                    PlacePrefabAtPosition(_selectedPrefabForPlacement, worldPos);
                    e.Use();
                }

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                _showRadialMenu = true;
                _radialMenuPosition = e.mousePosition;
                _radialMenuOpenTime = Time.realtimeSinceStartup;
                sceneView.Repaint();
                e.Use();
                return;
            }

            float baseRadius = 100f;

            if (_showRadialMenu)
            {
                Handles.BeginGUI();

                Vector2 center = _radialMenuPosition;

                // Smooth grow-in animation
                float elapsedTime = Time.realtimeSinceStartup - _radialMenuOpenTime;
                float growProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsedTime / 0.5f));
                float radius = baseRadius * growProgress;

                Handles.color = new Color(0, 0, 0, 0.5f);
                Handles.DrawSolidDisc(center, Vector3.forward, radius);

                Handles.color = Color.white;
                Handles.DrawWireDisc(center, Vector3.forward, radius);

                int prefabCount = Mathf.Min(_filteredPrefabs.Count, 8);
                for (int i = 0; i < prefabCount; i++)
                {
                    if (_filteredPrefabs[i] == null) continue;

                    float angle = i / (float)prefabCount * 360f - 90f;
                    Vector2 direction = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    Vector2 buttonPos = center + direction * (radius * 0.7f);

                    Texture2D preview = GetPrefabPreview(_filteredPrefabs[i]);
                    if (preview != null)
                    {
                        Rect imageRect = new(buttonPos.x - 20, buttonPos.y - 20, 40, 40);

                        // Highlight hovered prefab
                        if (Vector2.Distance(Event.current.mousePosition, buttonPos) < 30f)
                        {
                            GUI.color = Color.yellow; // Change color to highlight
                            GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
                            GUI.color = Color.white; // Reset color
                        }
                        else
                        {
                            GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
                        }
                    }

                    if (Event.current.type == EventType.MouseDown && Vector2.Distance(Event.current.mousePosition, buttonPos) < 30f)
                    {
                        _selectedPrefabForPlacement = _filteredPrefabs[i];
                        _placementMode = false; // Ensure placement mode is disabled
                        _showRadialMenu = false;
                        Event.current.Use();
                    }
                }

                Handles.EndGUI();
            }

            // Shrink-out animation
            if (!_showRadialMenu)
            {
                float shrinkProgress = Mathf.Clamp01((Time.realtimeSinceStartup - _radialMenuCloseTime) / 0.5f);
                float radius = baseRadius * (1 - shrinkProgress);

                Handles.color = new Color(0, 0, 0, 0.5f);
                Handles.DrawSolidDisc(_radialMenuPosition, Vector3.forward, radius);
            }
        }

        private void PlacePrefabAtPosition(GameObject prefab, Vector3 placePosition)
        {
            Undo.IncrementCurrentGroup();
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");
            instance.transform.position = placePosition;
            Selection.activeGameObject = instance;
        }

        private void IncrementPrefabUsage(GameObject prefab)
        {
            if (_prefabUsageCount.ContainsKey(prefab)) _prefabUsageCount[prefab]++;
            else _prefabUsageCount[prefab] = 1;

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

        private void SaveUsageData()
        {
            string data = string.Join(";", _prefabUsageCount.Select(kvp =>
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(kvp.Key));
                return $"{guid}:{kvp.Value}";
            }));

            EditorPrefs.SetString("PrefabAuditor_UsageData", data);
        }

        private void LoadUsageData()
        {
            _prefabUsageCount.Clear();
            string data = EditorPrefs.GetString("PrefabAuditor_UsageData", "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (string entry in data.Split(';'))
            {
                if (string.IsNullOrEmpty(entry)) continue;
                string[] parts = entry.Split(':');
                if (parts.Length != 2) continue;

                string guid = parts[0];
                if (!AssetDatabase.GUIDToAssetPath(guid).EndsWith(".prefab")) continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab != null)
                    _prefabUsageCount[prefab] = int.Parse(parts[1]);
            }
        }

        private void LoadPrefabs()
        {
            _prefabs.Clear();
            _previewCache.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    _prefabs.Add(prefab);
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredPrefabs.Clear();
            foreach (GameObject prefab in _prefabs)
            {
                if (!MatchesSearch(prefab)) continue;
                if (!MatchesFilter(prefab)) continue;
                _filteredPrefabs.Add(prefab);
            }
        }

        private bool MatchesSearch(GameObject prefab)
        {
            if (string.IsNullOrEmpty(_searchFilter)) return true;
            return prefab.name.ToLower().Contains(_searchFilter.ToLower());
        }

        private bool MatchesFilter(GameObject prefab)
        {
            switch (_filterType)
            {
                case FilterType.Enemies:
                    return (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
                case FilterType.Containers:
                    return prefab.layer == LayerMask.NameToLayer("Collectibles") &&
                           prefab.GetComponent<PowerUpContainer>();
                case FilterType.Others:
                    bool isEnemy = (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
                    bool isContainer = prefab.layer == LayerMask.NameToLayer("Collectibles") &&
                                       prefab.GetComponent<PowerUpContainer>();

                    return !isEnemy && !isContainer;
                default:
                    return true;
            }
        }

        private enum FilterType
        {
            All,
            Enemies,
            Containers,
            Others
        }
    }
}

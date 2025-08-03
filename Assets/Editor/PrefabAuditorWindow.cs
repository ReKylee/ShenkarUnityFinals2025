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
        }

        private void OnDisable()
        {
            SaveUsageData();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            InitializeStyles();

            DrawHeader();
            DrawFrequentPrefabs();
            DrawFiltersAndSearch();
            DrawQuickSettings();
            DrawPrefabsList();

            if (_autoRefresh && GUI.changed)
                ApplyFilters();

            HandlePlacementMode();
            HandleRadialMenu(SceneView.lastActiveSceneView);
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

        private void DrawFrequentPrefabs()
        {
            if (_frequentPrefabs.Count == 0) return;
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField("⭐ Most Used Prefabs", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            foreach (GameObject prefab in _frequentPrefabs)
            {
                if (prefab == null) continue;
                Texture2D preview = GetPrefabPreview(prefab);
                EditorGUILayout.BeginVertical(GUILayout.Width(90), GUILayout.Height(80));
                if (preview != null)
                {
                    Rect imageRect = GUILayoutUtility.GetRect(64, 40, GUILayout.ExpandWidth(false));
                    imageRect.x += 13f;
                    GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUILayoutUtility.GetRect(64, 40);
                }

                if (GUILayout.Button(prefab.name, GUILayout.Height(35))) StartPlacementMode(prefab);
                EditorGUILayout.EndVertical();
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
            int columns = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth / 120));
            int rows = Mathf.CeilToInt(_filteredPrefabs.Count / (float)columns);
            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index >= _filteredPrefabs.Count) break;
                    GameObject prefab = _filteredPrefabs[index];
                    EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(110));
                    Texture2D preview = GetPrefabPreview(prefab);
                    if (preview != null)
                    {
                        Rect rect = GUILayoutUtility.GetRect(80, 60, GUILayout.ExpandWidth(false));
                        GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUILayout.Space(60);
                    }

                    if (GUILayout.Button(prefab.name, GUILayout.Height(35))) StartPlacementMode(prefab);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private Texture2D GetPrefabPreview(GameObject prefab)
        {
            if (_previewCache.TryGetValue(prefab, out Texture2D preview) && preview != null) return preview;
            preview = AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab);
            _previewCache[prefab] = preview;
            return preview;
        }

        private void StartPlacementMode(GameObject prefab)
        {
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

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                _showRadialMenu = true;
                _radialMenuPosition = e.mousePosition;
                sceneView.Repaint();
                e.Use();
                return;
            }

            if (_showRadialMenu)
            {
                HandleRadialMenu(sceneView);
            }

            if (_placementMode && _selectedPrefabForPlacement != null)
            {
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

                if (e.type == EventType.Repaint)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    Vector3 worldPos = ray.origin;
                    worldPos.z = 0;

                    if (e.shift && _ppu > 0)
                    {
                        worldPos.x = Mathf.Round(worldPos.x * _ppu) / _ppu;
                        worldPos.y = Mathf.Round(worldPos.y * _ppu) / _ppu;
                    }

                    Handles.color = Color.yellow;
                    Handles.DrawWireCube(worldPos, Vector3.one * 0.5f);
                }

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }

        private void HandleRadialMenu(SceneView sceneView)
        {
            Handles.BeginGUI();

            Vector2 center = _radialMenuPosition;
            float radius = 80f;

            Handles.color = new Color(0, 0, 0, 0.5f);
            Handles.DrawSolidDisc(center, Vector3.forward, radius);

            Handles.color = Color.white;
            Handles.DrawWireDisc(center, Vector3.forward, radius);

            int prefabCount = Mathf.Min(_frequentPrefabs.Count, 8);
            for (int i = 0; i < prefabCount; i++)
            {
                if (_frequentPrefabs[i] == null) continue;

                float angle = i / (float)prefabCount * 360f - 90f;
                Vector2 direction = new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 buttonPos = center + direction * (radius * 0.7f);

                Texture2D preview = AssetPreview.GetAssetPreview(_frequentPrefabs[i]);
                if (preview != null)
                {
                    Rect imageRect = new(buttonPos.x - 20, buttonPos.y - 20, 40, 40);
                    GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
                }

                if (Event.current.type == EventType.MouseDown &&
                    Vector2.Distance(Event.current.mousePosition, buttonPos) < 30f)
                {
                    StartPlacementMode(_frequentPrefabs[i]);
                    _showRadialMenu = false;
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.MouseDown &&
                Vector2.Distance(Event.current.mousePosition, center) > radius)
            {
                _showRadialMenu = false;
                Event.current.Use();
            }

            Handles.EndGUI();
            sceneView.Repaint();
        }

        private void PlacePrefabAtPosition(GameObject prefab, Vector3 placePosition)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
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

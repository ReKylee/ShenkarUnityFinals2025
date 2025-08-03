using System.Collections.Generic;
using System.Linq;
using Enemies.Behaviors;
using PowerUps.Container;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace Editor
{
    public class PrefabAuditorWindow : EditorWindow
    {
        private readonly List<GameObject> _prefabs = new();
        private readonly List<GameObject> _filteredPrefabs = new();
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private FilterType _filterType = FilterType.All;
        private bool _autoRefresh = true;
        
        // Quick settings
        private float _ppu = 16f;
        private GameObject _defaultDropPrefab;
        private bool _showAdvancedSettings = false;
        private bool _placementMode = false;
        private GameObject _selectedPrefabForPlacement;
        
        // Radial Menu System
        private bool _showRadialMenu = false;
        private Vector2 _radialMenuPosition;
        private readonly Dictionary<GameObject, int> _prefabUsageCount = new();
        private List<GameObject> _frequentPrefabs = new();
        private const int MAX_FREQUENT_PREFABS = 8;
        
        // Batch operations
        private readonly List<int> _selectedPrefabs = new();
        private bool _batchMode = false;
        
        // UI Colors and Styles
        private static GUIStyle _headerStyle;
        private static GUIStyle _cardStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _selectedButtonStyle;
        
        // Preview icon cache
        private readonly Dictionary<GameObject, Texture2D> _previewCache = new();
        
        private enum FilterType
        {
            All,
            Enemies,
            Containers,
            Others
        }

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

        private void InitializeStyles()
        {
            if (_headerStyle != null) return; // Already initialized
            
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
            
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 5, 5)
            };
            
            _selectedButtonStyle = new GUIStyle(_buttonStyle)
            {
                normal = { background = EditorGUIUtility.isProSkin ? 
                    MakeTex(1, 1, new Color(0.3f, 0.5f, 0.8f, 1f)) : 
                    MakeTex(1, 1, new Color(0.6f, 0.8f, 1f, 1f)) }
            };
        }

        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            InitializeStyles();
            
            DrawHeader();
            DrawFrequentPrefabs();
            DrawFiltersAndSearch();
            DrawQuickSettings();
            DrawPrefabsList();
            DrawBatchOperations();
            
            if (_autoRefresh && GUI.changed)
                ApplyFilters();
            
            HandlePlacementMode();
            
            if (_showRadialMenu)
                Repaint();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(_cardStyle ?? GUI.skin.box);
            EditorGUILayout.LabelField("🎮 Level Auditor & Prefab Manager", _headerStyle ?? EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Found {_prefabs.Count} prefabs | Showing {_filteredPrefabs.Count}", EditorStyles.miniLabel);
            
            if (_placementMode)
            {
                string prefabName = _selectedPrefabForPlacement ? _selectedPrefabForPlacement.name : "None";
                EditorGUILayout.HelpBox($"🎯 PLACEMENT MODE: {prefabName} - Click in Scene View to place", MessageType.Info);
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80)))
                LoadPrefabs();
            
            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", GUILayout.Width(100));
            
            GUILayout.FlexibleSpace();
            
            if (_placementMode && GUILayout.Button("❌ Exit Placement", GUILayout.Width(120)))
            {
                _placementMode = false;
                _selectedPrefabForPlacement = null;
            }
            
            _batchMode = GUILayout.Toggle(_batchMode, "Batch Mode", _buttonStyle ?? GUI.skin.button, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFrequentPrefabs()
        {
            if (_frequentPrefabs.Count == 0) return;
            
            EditorGUILayout.BeginVertical(_cardStyle ?? GUI.skin.box);
            EditorGUILayout.LabelField("⭐ Most Used Prefabs (Right-click in Scene for quick menu)", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < Mathf.Min(_frequentPrefabs.Count, 6); i++)
            {
                GameObject prefab = _frequentPrefabs[i];
                if (prefab == null) continue;
                
                Texture2D preview = GetPrefabPreview(prefab);
                int usageCount = _prefabUsageCount.ContainsKey(prefab) ? _prefabUsageCount[prefab] : 0;
                
                // Create a vertical layout for the button content
                EditorGUILayout.BeginVertical(GUILayout.Width(90), GUILayout.Height(80));
                
                // Draw preview image
                if (preview != null)
                {
                    Rect imageRect = GUILayoutUtility.GetRect(64, 40, GUILayout.ExpandWidth(false));
                    imageRect.x += (90 - 64) / 2; // Center the image
                    GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUILayoutUtility.GetRect(64, 40); // Reserve space even if no preview
                }
                
                // Draw button with name and usage count
                string buttonText = $"{prefab.name}\n({usageCount})";
                if (GUILayout.Button(buttonText, GUILayout.Height(35)))
                {
                    StartPlacementMode(prefab);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("🗑️ Reset Usage", GUILayout.Width(100), GUILayout.Height(80)))
            {
                if (EditorUtility.DisplayDialog("Reset Usage Data", "This will reset all prefab usage statistics.", "Reset", "Cancel"))
                {
                    _prefabUsageCount.Clear();
                    UpdateFrequentPrefabs();
                    SaveUsageData();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFiltersAndSearch()
        {
            EditorGUILayout.BeginVertical(_cardStyle ?? GUI.skin.box);
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
            EditorGUILayout.EndVertical();
        }

        private void DrawQuickSettings()
        {
            EditorGUILayout.BeginVertical(_cardStyle ?? GUI.skin.box);
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
            
            if (_showAdvancedSettings)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🎯 Batch Set Drops (Instances Only)"))
                    BatchSetDropsInstances();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabsList()
        {
            EditorGUILayout.BeginVertical(_cardStyle ?? GUI.skin.box);
            EditorGUILayout.LabelField("📦 Prefabs", EditorStyles.boldLabel);
            
            if (_filteredPrefabs.Count == 0)
            {
                EditorGUILayout.HelpBox("No prefabs match current filters.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
            
            for (int i = 0; i < _filteredPrefabs.Count; i++)
            {
                DrawPrefabCard(_filteredPrefabs[i], i);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawPrefabCard(GameObject prefab, int index)
        {
            bool isSelected = _selectedPrefabs.Contains(index);
            GUIStyle cardStyle = isSelected && _selectedButtonStyle != null ? _selectedButtonStyle : 
                                (_cardStyle ?? GUI.skin.box);
            
            EditorGUILayout.BeginVertical(cardStyle);
            
            // Header with prefab info and preview
            EditorGUILayout.BeginHorizontal();
            
            if (_batchMode)
            {
                bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                if (newSelected != isSelected)
                {
                    if (newSelected)
                        _selectedPrefabs.Add(index);
                    else
                        _selectedPrefabs.Remove(index);
                }
            }
            
            // Preview image
            Texture2D preview = GetPrefabPreview(prefab);
            if (preview != null)
            {
                GUILayout.Label(preview, GUILayout.Width(48), GUILayout.Height(48));
            }
            else
            {
                // Fallback - draw a colored box with emoji icon
                GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    alignment = TextAnchor.MiddleCenter
                };
                
                string icon = GetPrefabIcon(prefab);
                Rect iconRect = GUILayoutUtility.GetRect(48, 48);
                
                // Draw background
                EditorGUI.DrawRect(iconRect, new Color(0.3f, 0.3f, 0.3f, 0.5f));
                GUI.Label(iconRect, icon, iconStyle);
            }
            
            // Vertical layout for name and info
            EditorGUILayout.BeginVertical();
            
            // Prefab name
            EditorGUILayout.LabelField(prefab.name, EditorStyles.boldLabel);
            
            // Quick info badges in a horizontal layout
            EditorGUILayout.BeginHorizontal();
            DrawInfoBadges(prefab);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
            
            // Quick actions
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🖱️ Place", GUILayout.Width(60)))
                StartPlacementMode(prefab);
            
            if (GUILayout.Button("📍 Instant", GUILayout.Width(60)))
                PlacePrefabInstant(prefab);
            
            if (GUILayout.Button("🔍 Select", GUILayout.Width(60)))
                Selection.activeObject = prefab;
            
            // Context-specific buttons
            DrawContextButtons(prefab);
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(2);
        }

        private Texture2D GetPrefabPreview(GameObject prefab)
        {
            if (_previewCache.ContainsKey(prefab) && _previewCache[prefab] != null)
                return _previewCache[prefab];
            
            // Try to get asset preview first
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            
            // If no preview available, try mini thumbnail
            if (preview == null)
                preview = AssetPreview.GetMiniThumbnail(prefab);
            
            // Cache the result (even if null to avoid repeated calls)
            _previewCache[prefab] = preview;
            
            return preview;
        }

        private void DrawInfoBadges(GameObject prefab)
        {
            bool isEnemy = (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
            bool isContainer = prefab.layer == LayerMask.NameToLayer("Collectibles") && 
                              prefab.GetComponent<PowerUpContainer>();
            
            if (isEnemy)
            {
                bool hasDrop = prefab.GetComponent<EnemyDropOnDeath>();
                EditorGUILayout.LabelField(hasDrop ? "💎 Drops" : "❌ No Drop", 
                    EditorStyles.miniLabel, GUILayout.Width(60));
            }
            
            if (isContainer)
            {
                bool hasLauncher = prefab.GetComponent<ProximityLauncher>();
                EditorGUILayout.LabelField(hasLauncher ? "🚀 Launcher" : "📦 Static", 
                    EditorStyles.miniLabel, GUILayout.Width(70));
            }
            
            // Layer info
            EditorGUILayout.LabelField($"Layer: {LayerMask.LayerToName(prefab.layer)}", 
                EditorStyles.miniLabel, GUILayout.Width(80));
        }

        private void DrawContextButtons(GameObject prefab)
        {
            bool isEnemy = (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
            bool isContainer = prefab.layer == LayerMask.NameToLayer("Collectibles") && 
                              prefab.GetComponent<PowerUpContainer>();
            
            if (isEnemy)
            {
                bool hasDrop = prefab.GetComponent<EnemyDropOnDeath>();
                if (GUILayout.Button(hasDrop ? "🔄 Update Drop" : "➕ Add Drop", GUILayout.Width(100)))
                {
                    if (_defaultDropPrefab != null)
                        ApplyEnemyDropSettingsToInstances(prefab, _defaultDropPrefab);
                    else
                        EditorUtility.DisplayDialog("No Drop Prefab", "Please set a default drop prefab first!", "OK");
                }
            }
            
            if (isContainer)
            {
                bool hasLauncher = prefab.GetComponent<ProximityLauncher>();
                if (GUILayout.Button(hasLauncher ? "🚀 Remove Launcher" : "➕ Add Launcher", GUILayout.Width(120)))
                    ApplyContainerSettingsToInstances(prefab, !hasLauncher);
            }
        }

        private void DrawBatchOperations()
        {
            if (!_batchMode || _selectedPrefabs.Count == 0) return;
            
            EditorGUILayout.BeginVertical(_cardStyle ?? GUI.skin.box);
            EditorGUILayout.LabelField($"🔧 Batch Operations ({_selectedPrefabs.Count} selected)", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🖱️ Place All"))
                BatchPlaceSelectedWithMouse();
            if (GUILayout.Button("📍 Instant Place"))
                BatchPlaceSelected();
            if (GUILayout.Button("💎 Set Drops"))
                BatchSetDropsSelected();
            if (GUILayout.Button("🚀 Toggle Launchers"))
                BatchToggleLaunchers();
            if (GUILayout.Button("❌ Clear Selection"))
                _selectedPrefabs.Clear();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            
            // Handle right-click for radial menu
            if (e.type == EventType.MouseDown && e.button == 1 && !_placementMode)
            {
                _showRadialMenu = true;
                _radialMenuPosition = e.mousePosition;
                sceneView.Repaint();
                e.Use();
                return;
            }
            
            // Handle radial menu
            if (_showRadialMenu)
            {
                HandleRadialMenu(sceneView);
                return;
            }
            
            // Handle placement mode
            if (_placementMode && _selectedPrefabForPlacement != null)
            {
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                {
                    _placementMode = false;
                    _selectedPrefabForPlacement = null;
                    e.Use();
                    return;
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    Vector3 worldPos = ray.origin;
                    worldPos.z = 0; // Assuming 2D game

                    // Grid snap if Shift is held
                    if (e.shift && _ppu > 0)
                    {
                        worldPos.x = Mathf.Round(worldPos.x * _ppu) / _ppu;
                        worldPos.y = Mathf.Round(worldPos.y * _ppu) / _ppu;
                    }

                    PlacePrefabAtPosition(_selectedPrefabForPlacement, worldPos);
                    e.Use();
                }

                // Draw preview at mouse position
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
                    Handles.Label(worldPos + Vector3.up * 0.5f, _selectedPrefabForPlacement.name);
                }

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }

        private void HandleRadialMenu(SceneView sceneView)
        {
            Event e = Event.current;
            
            // Draw radial menu
            Handles.BeginGUI();
            
            Vector2 center = _radialMenuPosition;
            float radius = 80f;
            
            // Background circle
            Handles.color = new Color(0, 0, 0, 0.5f);
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            
            Handles.color = Color.white;
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            
            // Draw prefab options in circle
            int prefabCount = Mathf.Min(_frequentPrefabs.Count, MAX_FREQUENT_PREFABS);
            for (int i = 0; i < prefabCount; i++)
            {
                if (_frequentPrefabs[i] == null) continue;
                
                float angle = (i / (float)prefabCount) * 360f - 90f; // Start from top
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 buttonPos = center + direction * (radius * 0.7f);
                
                // Highlight if mouse is over
                Vector2 mousePos = e.mousePosition;
                bool isHovered = Vector2.Distance(mousePos, buttonPos) < 30f;
                
                if (isHovered)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawSolidDisc(buttonPos, Vector3.forward, 30f);
                    Handles.color = Color.white;
                }
                
                // Draw preview image or fallback icon
                Texture2D preview = GetPrefabPreview(_frequentPrefabs[i]);
                if (preview != null)
                {
                    // Draw the preview image
                    Rect imageRect = new Rect(buttonPos.x - 20, buttonPos.y - 20, 40, 40);
                    GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit);
                }
                else
                {
                    // Draw fallback colored circle with emoji
                    Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                    Handles.DrawSolidDisc(buttonPos, Vector3.forward, 20f);
                    
                    string icon = GetPrefabIcon(_frequentPrefabs[i]);
                    GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 16,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    };
                    
                    GUI.Label(new Rect(buttonPos.x - 15, buttonPos.y - 10, 30, 20), icon, iconStyle);
                    Handles.color = Color.white;
                }
                
                // Draw name below
                string displayName = _frequentPrefabs[i].name;
                if (displayName.Length > 8) displayName = displayName.Substring(0, 8) + "...";
                
                GUI.Label(new Rect(buttonPos.x - 30, buttonPos.y + 25, 60, 20), displayName, EditorStyles.centeredGreyMiniLabel);
                
                // Handle click
                if (isHovered && e.type == EventType.MouseDown && e.button == 0)
                {
                    StartPlacementMode(_frequentPrefabs[i]);
                    _showRadialMenu = false;
                    e.Use();
                }
            }
            
            // Close menu if clicked outside or right-clicked
            if ((e.type == EventType.MouseDown && Vector2.Distance(e.mousePosition, center) > radius) ||
                (e.type == EventType.MouseDown && e.button == 1))
            {
                _showRadialMenu = false;
                e.Use();
            }
            
            // Instructions
            GUI.Label(new Rect(center.x - 100, center.y + radius + 10, 200, 20), 
                "Click prefab to place • Right-click to cancel", EditorStyles.centeredGreyMiniLabel);
            
            Handles.EndGUI();
            
            sceneView.Repaint();
        }

        private void StartPlacementMode(GameObject prefab)
        {
            _placementMode = true;
            _selectedPrefabForPlacement = prefab;
            
            // Track usage
            IncrementPrefabUsage(prefab);
            
            EditorUtility.DisplayDialog("Placement Mode", 
                $"Click in Scene View to place {prefab.name}.\nHold Shift for grid snap.\nRight-click for quick menu.\nPress Escape to stop.", "OK");
        }

        private void HandlePlacementMode()
        {
            if (_placementMode && _selectedPrefabForPlacement == null)
            {
                _placementMode = false;
            }
        }

        // Usage Tracking System
        private void IncrementPrefabUsage(GameObject prefab)
        {
            if (_prefabUsageCount.ContainsKey(prefab))
                _prefabUsageCount[prefab]++;
            else
                _prefabUsageCount[prefab] = 1;
            
            UpdateFrequentPrefabs();
            SaveUsageData();
        }

        private void UpdateFrequentPrefabs()
        {
            _frequentPrefabs.Clear();
            
            var sortedPrefabs = _prefabUsageCount
                .Where(kvp => kvp.Key != null)
                .OrderByDescending(kvp => kvp.Value)
                .Take(MAX_FREQUENT_PREFABS)
                .Select(kvp => kvp.Key);
            
            _frequentPrefabs.AddRange(sortedPrefabs);
        }

        private void SaveUsageData()
        {
            string data = "";
            foreach (var kvp in _prefabUsageCount)
            {
                if (kvp.Key != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(kvp.Key));
                    data += $"{guid}:{kvp.Value};";
                }
            }
            EditorPrefs.SetString("PrefabAuditor_UsageData", data);
        }

        private void LoadUsageData()
        {
            _prefabUsageCount.Clear();
            string data = EditorPrefs.GetString("PrefabAuditor_UsageData", "");
            
            if (string.IsNullOrEmpty(data)) return;
            
            string[] entries = data.Split(';');
            foreach (string entry in entries)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                
                string[] parts = entry.Split(':');
                if (parts.Length == 2)
                {
                    string guid = parts[0];
                    if (int.TryParse(parts[1], out int count))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab != null)
                        {
                            _prefabUsageCount[prefab] = count;
                        }
                    }
                }
            }
        }

        // Batch Operations
        private void BatchPlaceSelectedWithMouse()
        {
            if (_selectedPrefabs.Count == 0) return;

            // Start placement mode with first selected prefab
            if (_selectedPrefabs.Count > 0 && _selectedPrefabs[0] < _filteredPrefabs.Count)
            {
                StartPlacementMode(_filteredPrefabs[_selectedPrefabs[0]]);
            }
        }

        private void BatchPlaceSelected()
        {
            Vector3 basePos = Vector3.zero;
            foreach (int index in _selectedPrefabs)
            {
                if (index < _filteredPrefabs.Count)
                {
                    PlacePrefabAtPosition(_filteredPrefabs[index], basePos);
                    basePos.x += 2f; // Offset each placement
                }
            }
        }

        private void BatchSetDropsSelected()
        {
            if (_defaultDropPrefab == null)
            {
                EditorUtility.DisplayDialog("No Drop Prefab", "Please set a default drop prefab first!", "OK");
                return;
            }

            foreach (int index in _selectedPrefabs)
            {
                if (index < _filteredPrefabs.Count)
                {
                    GameObject prefab = _filteredPrefabs[index];
                    bool isEnemy = (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
                    if (isEnemy)
                        ApplyEnemyDropSettingsToInstances(prefab, _defaultDropPrefab);
                }
            }
        }

        private void BatchToggleLaunchers()
        {
            foreach (int index in _selectedPrefabs)
            {
                if (index < _filteredPrefabs.Count)
                {
                    GameObject prefab = _filteredPrefabs[index];
                    bool isContainer = prefab.layer == LayerMask.NameToLayer("Collectibles") &&
                                      prefab.GetComponent<PowerUpContainer>();
                    if (isContainer)
                    {
                        bool hasLauncher = prefab.GetComponent<ProximityLauncher>();
                        ApplyContainerSettingsToInstances(prefab, !hasLauncher);
                    }
                }
            }
        }

        private void BatchSetDropsInstances()
        {
            if (_defaultDropPrefab == null)
            {
                EditorUtility.DisplayDialog("No Drop Prefab", "Please set a default drop prefab first!", "OK");
                return;
            }

            int updatedCount = 0;
            foreach (GameObject prefab in _filteredPrefabs)
            {
                bool isEnemy = (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
                if (isEnemy)
                {
                    ApplyEnemyDropSettingsToInstances(prefab, _defaultDropPrefab);
                    updatedCount++;
                }
            }
            EditorUtility.DisplayDialog("Batch Set Drops", $"Updated drop settings for {updatedCount} enemy instances in scene.", "OK");
        }

        // Placement Methods (Instance-only modifications)
        private void PlacePrefabInstant(GameObject prefab)
        {
            PlacePrefabAtPosition(prefab, Vector3.zero);
        }

        private void PlacePrefabAtPosition(GameObject prefab, Vector3 position)
        {
            try
            {
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = position;
                Selection.activeGameObject = go;

                // Track usage
                IncrementPrefabUsage(prefab);

                // Apply instance modifications if this is an enemy with no drop settings
                bool isEnemy = (1 << prefab.layer & LayerMask.GetMask("Enemy")) != 0;
                if (isEnemy && _defaultDropPrefab != null && !prefab.GetComponent<EnemyDropOnDeath>())
                {
                    EnemyDropOnDeath enemyDrop = go.AddComponent<EnemyDropOnDeath>();
                    enemyDrop.dropPrefab = _defaultDropPrefab;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to place prefab {prefab.name}: {e.Message}");
            }
        }

        // Instance-only modification methods
        private void ApplyEnemyDropSettingsToInstances(GameObject prefab, GameObject dropPrefab)
        {
            // Find all instances of this prefab in the scene and update them
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int updatedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == prefab)
                {
                    try
                    {
                        EnemyDropOnDeath enemyDrop = obj.GetComponent<EnemyDropOnDeath>() ??
                                                    obj.AddComponent<EnemyDropOnDeath>();
                        enemyDrop.dropPrefab = dropPrefab;
                        EditorUtility.SetDirty(obj);
                        updatedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to apply enemy drop settings to instance {obj.name}: {e.Message}");
                    }
                }
            }

            if (updatedCount > 0)
                EditorUtility.DisplayDialog("Updated Instances", $"Updated {updatedCount} instances of {prefab.name} in the scene.", "OK");
            else
                EditorUtility.DisplayDialog("No Instances", $"No instances of {prefab.name} found in the current scene.", "OK");
        }

        private void ApplyContainerSettingsToInstances(GameObject prefab, bool hasProximityLauncher)
        {
            // Find all instances of this prefab in the scene and update them
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int updatedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == prefab)
                {
                    try
                    {
                        ProximityLauncher launcher = obj.GetComponent<ProximityLauncher>();
                        if (hasProximityLauncher && launcher == null)
                            obj.AddComponent<ProximityLauncher>();
                        else if (!hasProximityLauncher && launcher)
                            DestroyImmediate(launcher);

                        EditorUtility.SetDirty(obj);
                        updatedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to apply container settings to instance {obj.name}: {e.Message}");
                    }
                }
            }

            if (updatedCount > 0)
                EditorUtility.DisplayDialog("Updated Instances", $"Updated {updatedCount} instances of {prefab.name} in the scene.", "OK");
            else
                EditorUtility.DisplayDialog("No Instances", $"No instances of {prefab.name} found in the current scene.", "OK");
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

        // Utility Methods
        private void LoadPrefabs()
        {
            _prefabs.Clear();
            _previewCache.Clear(); // Clear preview cache when reloading

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab)
                    _prefabs.Add(prefab);
            }
            ApplyFilters();
        }

        [MenuItem("Tools/Level Auditor & Prefab Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabAuditorWindow>("Level Auditor");
            window.minSize = new Vector2(500, 600);
        }
    }
}


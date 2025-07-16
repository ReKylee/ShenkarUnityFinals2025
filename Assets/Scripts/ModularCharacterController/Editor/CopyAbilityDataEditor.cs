#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModularCharacterController.Core.Abilities;
using ModularCharacterController.Core.Abilities.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModularCharacterController.Editor
{
    [CustomEditor(typeof(CopyAbilityData))]
    public class CopyAbilityDataEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, bool> _abilityFoldouts = new();
        // --- END OPTIMIZATION ---

        private readonly List<int> _cachedDisplayedIndices = new(50);
        private readonly Dictionary<Object, UnityEditor.Editor> _cachedEditors = new(); // Cache for embedded editors
        private readonly Dictionary<int, Texture> _cachedIcons = new();

        // --- OPTIMIZATION: Cache GUI styles as fields but initialize them lazily ---
        private GUIStyle _abilitiesContainerStyle;
        private SerializedProperty _abilitiesListProperty;
        private string _abilitySearchString = string.Empty;
        private GUIStyle _addAbilityContainerStyle;
        private string[] _availableAbilityTypeNames;
        private List<Type> _availableAbilityTypes;
        private GUIStyle _cardButtonStyle;
        private GUIStyle _foldoutContainerStyle;
        private GUIStyle _foldoutStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _listContainerStyle;
        private ReorderableModifierList _modifierList;
        private GUIStyle _objectFieldStyle;
        private GUIStyle _roundedBoxStyle;
        private Vector2 _scrollPosition;
        private bool _searchResultsNeedRefresh = true;
        private int _selectedAbilityTypeIndex;

        private Type _selectedInterfaceFilter;

        private SerializedProperty _statModifiersProperty;

        private void OnEnable()
        {
            _abilitiesListProperty = serializedObject.FindProperty("abilities");
            _statModifiersProperty = serializedObject.FindProperty("statModifiers");
            _modifierList = new ReorderableModifierList(serializedObject, _statModifiersProperty);

            RefreshAvailableAbilityTypes();
        }

        private void OnDisable() // Clean up cached editors
        {
            foreach (UnityEditor.Editor editorEntry in _cachedEditors.Values)
            {
                if (editorEntry)
                {
                    DestroyImmediate(editorEntry);
                }
            }

            _cachedEditors.Clear();
        }

        private void RefreshAvailableAbilityTypes()
        {
            _availableAbilityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(AbilityModuleBase)) && !type.IsAbstract && type.IsPublic)
                .ToList();

            _availableAbilityTypeNames = _availableAbilityTypes.Select(type => type.Name).ToArray();
        }

        public override void OnInspectorGUI()
        {
            // --- OPTIMIZATION: Lazy Initialization of GUIStyles ---
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                    { alignment = TextAnchor.MiddleLeft, fontSize = 12 };

                _listContainerStyle = new GUIStyle
                    { margin = new RectOffset(8, 8, 0, 8), padding = new RectOffset(2, 2, 2, 2) };

                _abilitiesContainerStyle = new GUIStyle
                    { margin = new RectOffset(8, 8, 0, 8), padding = new RectOffset(5, 5, 5, 5) };

                _addAbilityContainerStyle = new GUIStyle(EditorStyles.helpBox)
                    { margin = new RectOffset(8, 8, 0, 8), padding = new RectOffset(10, 10, 10, 10) };

                _roundedBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(8, 8, 8, 8) };

                _foldoutContainerStyle = new GUIStyle
                    { margin = new RectOffset(0, 0, 1, 0), padding = new RectOffset(0, 0, 0, 0) };

                _foldoutStyle = new GUIStyle(EditorStyles.foldout)
                    { alignment = TextAnchor.MiddleLeft, margin = new RectOffset(0, 0, 1, 0) };

                _objectFieldStyle = new GUIStyle
                    { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0) };

                _cardButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 32f,
                    padding = new RectOffset(12, 8, 8, 8),
                    margin = new RectOffset(4, 4, 4, 4),
                    richText = true
                };
            }
            // --- END OPTIMIZATION ---

            // Update serialized object at the beginning
            serializedObject.Update();

            CopyAbilityData copyAbilityData = (CopyAbilityData)target;

            // Ensure DrawPropertiesExcluding has its own change check
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script", "statModifiers", "abilities");
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space(10);

            // Create a header for Stat Modifiers
            Rect statModHeaderRect = EditorGUILayout.GetControlRect(false, 28);
            statModHeaderRect.x += 10;
            statModHeaderRect.width -= 20;
            EditorGUI.LabelField(statModHeaderRect, "Stat Modifiers", _headerStyle);

            EditorGUILayout.Space(5);

            // Draw the modifiers list
            EditorGUILayout.BeginVertical(_listContainerStyle);

            if (_modifierList.DoLayoutList()) // DoLayoutList handles its own ApplyModifiedProperties
                EditorUtility.SetDirty(target);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Create a header for Contained Abilities
            Rect abilitiesHeaderRect = EditorGUILayout.GetControlRect(false, 28);
            abilitiesHeaderRect.x += 10;
            abilitiesHeaderRect.width -= 20;

            Rect labelRect = new(abilitiesHeaderRect.x, abilitiesHeaderRect.y,
                abilitiesHeaderRect.width * 0.6f, abilitiesHeaderRect.height);

            Rect buttonRect = new(
                labelRect.x + labelRect.width + 10,
                abilitiesHeaderRect.y + 2,
                abilitiesHeaderRect.width * 0.4f - 10,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, "Contained Abilities", _headerStyle);

            Color prevCleanupBtnColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.6f, 0.3f, 0.3f, 0.8f)
                : new Color(1f, 0.85f, 0.85f, 1f);

            GUIContent cleanupButtonContent = new("Clean Up Unused",
                "Find and delete ability module assets that are no longer referenced by this or any other Copy Ability");

            if (GUI.Button(buttonRect, cleanupButtonContent))
            {
                CleanupUnusedAbilityModules((CopyAbilityData)target);
            }

            GUI.backgroundColor = prevCleanupBtnColor;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(_abilitiesContainerStyle);
            var currentAbilityObjects = new List<Object>();
            for (int i = 0; i < _abilitiesListProperty.arraySize; i++)
            {
                SerializedProperty abilityRefProperty = _abilitiesListProperty.GetArrayElementAtIndex(i);
                currentAbilityObjects.Add(abilityRefProperty.objectReferenceValue);
                AbilityModuleBase abilityModuleInstance = abilityRefProperty.objectReferenceValue as AbilityModuleBase;

                if (abilityModuleInstance)
                {
                    string foldoutKey = $"Ability_{abilityModuleInstance.GetInstanceID()}";
                    _abilityFoldouts.TryGetValue(foldoutKey, out bool isFoldout);

                    EditorGUILayout.BeginVertical(_roundedBoxStyle);
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 2));
                    GUILayout.Space(8);
                    EditorGUILayout.BeginVertical(_foldoutContainerStyle,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    GUIContent foldoutContent = new($"{abilityModuleInstance.GetType().Name}");
                    bool newFoldoutState =
                        EditorGUILayout.Foldout(isFoldout, foldoutContent, true, _foldoutStyle);

                    if (newFoldoutState != isFoldout)
                    {
                        _abilityFoldouts[foldoutKey] = newFoldoutState;
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();

                    float objectFieldWidth = EditorGUIUtility.currentViewWidth * 0.4f;

                    EditorGUILayout.BeginVertical(_objectFieldStyle);
                    EditorGUI.BeginChangeCheck();
                    Object newReference = EditorGUILayout.ObjectField(
                        abilityModuleInstance,
                        typeof(AbilityModuleBase),
                        false,
                        GUILayout.Width(objectFieldWidth));

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(4);

                    if (EditorGUI.EndChangeCheck())
                    {
                        abilityRefProperty.objectReferenceValue = newReference;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(copyAbilityData);
                    }

                    if (GUILayout.Button(new GUIContent("X", "Remove Ability"), GUILayout.Width(30),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        Object objToRemove = abilityRefProperty.objectReferenceValue;
                        if (objToRemove != null &&
                            _cachedEditors.TryGetValue(objToRemove, out UnityEditor.Editor cachedEditor))
                        {
                            DestroyImmediate(cachedEditor);
                            _cachedEditors.Remove(objToRemove);
                        }

                        abilityRefProperty.objectReferenceValue = null;
                        _abilitiesListProperty.DeleteArrayElementAtIndex(i);
                        EditorUtility.SetDirty(copyAbilityData);
                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        GUIUtility.ExitGUI();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (newFoldoutState)
                    {
                        EditorGUILayout.Space(8);
                        DrawAbilityModuleInspector(abilityModuleInstance, _headerStyle);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(abilityRefProperty, GUIContent.none, GUILayout.ExpandWidth(true));

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(copyAbilityData);
                    }

                    if (GUILayout.Button(new GUIContent("X", "Remove Slot"), GUILayout.Width(30),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        Object objToRemove = abilityRefProperty.objectReferenceValue;
                        if (objToRemove &&
                            _cachedEditors.TryGetValue(objToRemove, out UnityEditor.Editor cachedEditor))
                        {
                            DestroyImmediate(cachedEditor);
                            _cachedEditors.Remove(objToRemove);
                        }

                        abilityRefProperty.objectReferenceValue = null;
                        _abilitiesListProperty.DeleteArrayElementAtIndex(i);
                        EditorUtility.SetDirty(copyAbilityData);
                        serializedObject.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        GUIUtility.ExitGUI();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (i < _abilitiesListProperty.arraySize - 1) EditorGUILayout.Separator();
                EditorGUILayout.Space(2);
            }

            var keysToRemove = _cachedEditors.Keys.Where(k => !currentAbilityObjects.Contains(k)).ToList();
            foreach (Object key in keysToRemove)
            {
                if (_cachedEditors.TryGetValue(key, out UnityEditor.Editor cachedEditor))
                {
                    DestroyImmediate(cachedEditor);
                }

                _cachedEditors.Remove(key);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Create a header for Add New Ability
            Rect addAbilityHeaderRect = EditorGUILayout.GetControlRect(false, 28);
            addAbilityHeaderRect.x += 10;
            addAbilityHeaderRect.width -= 20;
            EditorGUI.LabelField(addAbilityHeaderRect, "Add New Ability", _headerStyle);

            EditorGUILayout.Space(5);

            DrawOptimizedAddAbilitySection(copyAbilityData);
        }

        private void DrawOptimizedAddAbilitySection(CopyAbilityData copyAbilityData)
        {
            EditorGUILayout.BeginVertical(_addAbilityContainerStyle);

            Color prevBgColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.25f, 0.28f, 1f)
                : new Color(0.93f, 0.95f, 1f, 1f);

            if (_availableAbilityTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("No AbilityBase subclasses found in the project.", MessageType.Info);
                GUI.backgroundColor = prevBgColor;
                EditorGUILayout.EndVertical();
                return;
            }

            // Search field
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Find:", GUILayout.Width(40));

            string newSearch = EditorGUILayout.TextField(_abilitySearchString, EditorStyles.toolbarSearchField);
            if (newSearch != _abilitySearchString)
            {
                _abilitySearchString = newSearch.Trim();
                _searchResultsNeedRefresh = true;
            }

            if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)) &&
                !string.IsNullOrEmpty(_abilitySearchString))
            {
                _abilitySearchString = "";
                _searchResultsNeedRefresh = true;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            // Interface filter dropdown
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Interface:", GUILayout.Width(60));

            int selectedIndex = _selectedInterfaceFilter != null
                ? _availableAbilityTypes.SelectMany(t => t.GetInterfaces()).Distinct().ToList()
                    .IndexOf(_selectedInterfaceFilter)
                : -1;

            string[] interfaceNames = _availableAbilityTypes
                .SelectMany(t => t.GetInterfaces())
                .Distinct()
                .Select(i => i.Name)
                .ToArray();

            int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, interfaceNames, GUILayout.Width(120));

            if (newSelectedIndex >= 0 && newSelectedIndex < interfaceNames.Length)
            {
                _selectedInterfaceFilter = _availableAbilityTypes
                    .SelectMany(t => t.GetInterfaces())
                    .Distinct()
                    .FirstOrDefault(i => i.Name == interfaceNames[newSelectedIndex]);

                _searchResultsNeedRefresh = true;
            }
            else if (newSelectedIndex == -1)
            {
                _selectedInterfaceFilter = typeof(IAbilityModule);
                _searchResultsNeedRefresh = true;
            }

            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                _selectedInterfaceFilter = typeof(IAbilityModule);
                _searchResultsNeedRefresh = true;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            var displayedIndices = new List<int>(50);

            if (_searchResultsNeedRefresh)
            {
                for (int i = 0; i < _availableAbilityTypeNames.Length; i++)
                {
                    string displayName = ObjectNames.NicifyVariableName(_availableAbilityTypeNames[i]);
                    bool matchesSearch = string.IsNullOrEmpty(_abilitySearchString) ||
                                         displayName.IndexOf(_abilitySearchString,
                                             StringComparison.OrdinalIgnoreCase) >= 0;

                    bool matchesInterface = _selectedInterfaceFilter == null ||
                                            _availableAbilityTypes[i].GetInterfaces()
                                                .Any(iface => iface == _selectedInterfaceFilter);

                    if (matchesSearch && matchesInterface)
                    {
                        displayedIndices.Add(i);
                    }
                }

                _searchResultsNeedRefresh = false;
            }
            else
            {
                displayedIndices.AddRange(_cachedDisplayedIndices);
            }

            _cachedDisplayedIndices.Clear();
            _cachedDisplayedIndices.AddRange(displayedIndices);

            float windowWidth = EditorGUIUtility.currentViewWidth - 40;
            int maxButtonsPerRow = Mathf.Max(1, Mathf.FloorToInt(windowWidth / 180f));
            float minScrollHeight = Mathf.Min(displayedIndices.Count * 32f, 180f);

            if (displayedIndices.Count == 0)
            {
                EditorGUILayout.HelpBox($"No abilities found matching '{_abilitySearchString}'", MessageType.Info);
                GUI.backgroundColor = prevBgColor;
                EditorGUILayout.EndVertical();
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(minScrollHeight));

            GUI.backgroundColor = prevBgColor;

            DrawAbilitiesFlat(displayedIndices, maxButtonsPerRow, copyAbilityData);

            EditorGUILayout.EndScrollView();

            GUI.backgroundColor = prevBgColor;
            EditorGUILayout.EndVertical();
        }

        private void DrawAbilitiesByCategory(List<int> displayedIndices, int maxButtonsPerRow,
            CopyAbilityData copyAbilityData)
        {
            // Check for empty list to avoid layout issues
            if (displayedIndices == null || displayedIndices.Count == 0)
                return;

            var abilityIndicesByCategory = new Dictionary<string, List<int>>(8);

            foreach (int index in displayedIndices)
            {
                string category = GetAbilityCategory(_availableAbilityTypes[index]);
                if (!abilityIndicesByCategory.TryGetValue(category, out var categoryList))
                {
                    categoryList = new List<int>();
                    abilityIndicesByCategory[category] = categoryList;
                }

                categoryList.Add(index);
            }

            foreach (var categoryPair in abilityIndicesByCategory.OrderBy(x => x.Key))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField(categoryPair.Key, EditorStyles.boldLabel);

                int[] sortedIndices = categoryPair.Value.OrderBy(i => _availableAbilityTypeNames[i]).ToArray();

                // Only start horizontal group if we have items
                if (sortedIndices.Length == 0)
                    continue;

                int buttonsInCurrentRow = 0;
                EditorGUILayout.BeginHorizontal();

                try
                {
                    foreach (int index in sortedIndices)
                    {
                        if (buttonsInCurrentRow >= maxButtonsPerRow)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            buttonsInCurrentRow = 0;
                        }

                        DrawAbilityButton(index, copyAbilityData);
                        buttonsInCurrentRow++;
                    }

                    while (buttonsInCurrentRow < maxButtonsPerRow)
                    {
                        GUILayout.FlexibleSpace();
                        buttonsInCurrentRow++;
                    }
                }
                finally
                {
                    // Always close the horizontal group, even if an exception occurs
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawAbilitiesFlat(List<int> displayedIndices, int maxButtonsPerRow,
            CopyAbilityData copyAbilityData)
        {
            // Check for empty list to avoid layout issues
            if (displayedIndices == null || displayedIndices.Count == 0)
                return;

            int[] sortedIndices = displayedIndices.OrderBy(i => _availableAbilityTypeNames[i]).ToArray();

            // Only start horizontal group if we have items
            if (sortedIndices.Length == 0)
                return;

            int buttonsInCurrentRow = 0;
            EditorGUILayout.BeginHorizontal();

            try
            {
                foreach (int index in sortedIndices)
                {
                    if (buttonsInCurrentRow >= maxButtonsPerRow)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        buttonsInCurrentRow = 0;
                    }

                    DrawAbilityButton(index, copyAbilityData, !string.IsNullOrEmpty(_abilitySearchString));
                    buttonsInCurrentRow++;
                }

                while (buttonsInCurrentRow < maxButtonsPerRow)
                {
                    GUILayout.FlexibleSpace();
                    buttonsInCurrentRow++;
                }
            }
            finally
            {
                // Always close the horizontal group, even if an exception occurs
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAbilityButton(int index, CopyAbilityData copyAbilityData, bool highlightSearch = false)
        {
            string displayName = ObjectNames.NicifyVariableName(_availableAbilityTypeNames[index]);

            if (!_cachedIcons.TryGetValue(index, out Texture iconTexture))
            {
                iconTexture = GetAbilityIcon(_availableAbilityTypes[index]);
                _cachedIcons[index] = iconTexture;
            }

            GUIContent buttonContent;
            if (iconTexture)
            {
                buttonContent = new GUIContent($"  {displayName}", iconTexture);
            }
            else
            {
                buttonContent = new GUIContent($"  {displayName}");
            }

            if (highlightSearch && !string.IsNullOrEmpty(_abilitySearchString))
            {
                int matchIndex = displayName.IndexOf(_abilitySearchString, StringComparison.OrdinalIgnoreCase);
                if (matchIndex >= 0)
                {
                    string beforeMatch = displayName.Substring(0, matchIndex);
                    string match = displayName.Substring(matchIndex, _abilitySearchString.Length);
                    string afterMatch = displayName.Substring(matchIndex + _abilitySearchString.Length);
                    buttonContent.text = $"  {beforeMatch}<color=#FFA500FF>{match}</color>{afterMatch}";
                }
            }

            if (GUILayout.Button(buttonContent, _cardButtonStyle, GUILayout.MinWidth(160)))
            {
                CreateAndAddAbility(copyAbilityData, _availableAbilityTypes[index]);
            }
        }

        private void CreateAndAddAbility(CopyAbilityData owner, Type abilityType)
        {
            CopyAbilityData.AbilityAddResult canAddResult = owner.CanAddAbilityModule(abilityType);

            if (canAddResult == CopyAbilityData.AbilityAddResult.DuplicateNotAllowed)
            {
                AbilityModuleBase conflictingModule = owner.GetConflictingModule(abilityType);
                string moduleName = conflictingModule ? conflictingModule.name : "Unknown";

                EditorUtility.DisplayDialog(
                    "Duplicate Module Not Allowed",
                    $"Cannot add multiple instances of {ObjectNames.NicifyVariableName(abilityType.Name)}.\n\n" +
                    $"An instance already exists: {moduleName}.\n\n" +
                    "This module types is configured to not allow multiple instances.",
                    "OK");

                return;
            }

            if (canAddResult == CopyAbilityData.AbilityAddResult.InvalidAbility)
            {
                EditorUtility.DisplayDialog(
                    "Invalid Ability Types",
                    $"The selected types '{abilityType?.Name ?? "null"}' is not a valid ability module types.",
                    "OK");

                return;
            }

            AbilityModuleBase newAbilityModuleInstance = (AbilityModuleBase)CreateInstance(abilityType);
            newAbilityModuleInstance.name = string.Format("{0}_For_{1}", abilityType.Name, owner.name.Replace(" ", ""));
            newAbilityModuleInstance.InitializeNameAndIDWithDefaultValue();

            string ownerPath = AssetDatabase.GetAssetPath(owner);
            string directory;
            if (string.IsNullOrEmpty(ownerPath))
            {
                directory = "Assets/Abilities_Generated";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            else
            {
                string abilitiesFolderName = string.Format("{0}_Abilities", owner.name.Replace(" ", ""));
                directory = Path.Combine(Path.GetDirectoryName(ownerPath) ?? string.Empty, abilitiesFolderName);
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string abilityAssetName = string.Format("{0}.asset", newAbilityModuleInstance.name);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, abilityAssetName));

            AssetDatabase.CreateAsset(newAbilityModuleInstance, assetPath);
            AssetDatabase.SaveAssets();

            CopyAbilityData.AbilityAddResult addResult = owner.AddAbilityModule(newAbilityModuleInstance);

            if (addResult != CopyAbilityData.AbilityAddResult.Success)
            {
                Debug.LogError($"Failed to add ability module: {addResult}");
                AssetDatabase.DeleteAsset(assetPath);
                return;
            }

            _abilitiesListProperty.arraySize = owner.abilities.Count;
            _abilitiesListProperty.GetArrayElementAtIndex(_abilitiesListProperty.arraySize - 1).objectReferenceValue =
                newAbilityModuleInstance;

            EditorUtility.SetDirty(owner);
            serializedObject.ApplyModifiedProperties();
        }

        private void CleanupUnusedAbilityModules(CopyAbilityData owner)
        {
            int totalAbilitiesFound = 0;
            int abilitiesDeleted = 0;

            string ownerPath = AssetDatabase.GetAssetPath(owner);
            if (string.IsNullOrEmpty(ownerPath))
            {
                EditorUtility.DisplayDialog("Cannot Clean Up",
                    "Save the CopyAbilityData asset first before cleaning up.", "OK");

                return;
            }

            string abilitiesFolderName = $"{owner.name.Replace(" ", "")}_Abilities";
            string directory = Path.Combine(Path.GetDirectoryName(ownerPath) ?? string.Empty, abilitiesFolderName);

            if (!Directory.Exists(directory))
            {
                EditorUtility.DisplayDialog("No Abilities Folder",
                    $"No abilities folder found at: {directory}", "OK");

                return;
            }

            string[] abilityAssetPaths = Directory.GetFiles(directory, "*.asset");
            totalAbilitiesFound = abilityAssetPaths.Length;

            if (totalAbilitiesFound == 0)
            {
                EditorUtility.DisplayDialog("No Abilities Found",
                    $"No ability assets found in: {directory}", "OK");

                return;
            }

            string[] allCopyAbilityGUIDs = AssetDatabase.FindAssets("t:CopyAbilityData");
            var allCopyAbilityData =
                allCopyAbilityGUIDs
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<CopyAbilityData>)
                    .ToList();

            var allReferencedAbilities = new HashSet<Object>();
            var copyAbilityDatas =
                allCopyAbilityData
                    .Select(copyAbility => new SerializedObject(copyAbility))
                    .Select(serializedCopyAbility => serializedCopyAbility.FindProperty("abilities"))
                    .Where(abilitiesProperty => abilitiesProperty is { isArray: true });

            foreach (SerializedProperty abilitiesProperty in copyAbilityDatas)
            {
                for (int i = 0; i < abilitiesProperty.arraySize; i++)
                {
                    SerializedProperty abilityRefProperty = abilitiesProperty.GetArrayElementAtIndex(i);
                    if (abilityRefProperty.objectReferenceValue)
                        allReferencedAbilities.Add(abilityRefProperty.objectReferenceValue);
                }
            }

            var pathsToDelete = abilityAssetPaths
                .Select(assetPath => (assetPath, abilityAsset: AssetDatabase.LoadAssetAtPath<Object>(assetPath)))
                .Where(t => t.abilityAsset && !allReferencedAbilities.Contains(t.abilityAsset))
                .Select(t => t.assetPath).ToList();

            if (pathsToDelete.Count == 0)
            {
                EditorUtility.DisplayDialog("No Unused Abilities",
                    "All ability modules in this folder are currently in use.", "OK");

                return;
            }

            string message =
                $"Found {pathsToDelete.Count} unused ability modules out of {totalAbilitiesFound} total.\n\n";

            int displayCount = Mathf.Min(pathsToDelete.Count, 5);
            for (int i = 0; i < displayCount; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathsToDelete[i]);
                message += $"• {fileNameWithoutExtension}\n";
            }

            if (pathsToDelete.Count > 5)
            {
                message += $"• ...and {pathsToDelete.Count - 5} more\n";
            }

            message += "\nDo you want to delete these unused ability modules?";

            bool userConfirmed = EditorUtility.DisplayDialog(
                "Confirm Cleanup",
                message,
                "Delete Unused Abilities",
                "Cancel");

            if (userConfirmed)
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (string path in pathsToDelete)
                    {
                        AssetDatabase.DeleteAsset(path);
                        abilitiesDeleted++;
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();

                    AssetDatabase.Refresh();
                }

                EditorUtility.DisplayDialog(
                    "Cleanup Complete",
                    $"Successfully deleted {abilitiesDeleted} unused ability modules.",
                    "OK");
            }
        }

        private UnityEditor.Editor CreateCachedEditorWithoutCustomEditor(Object obj, Type type,
            ref Dictionary<Object, UnityEditor.Editor> cache)
        {
            if (!obj) return null;
            if (cache.TryGetValue(obj, out UnityEditor.Editor editor) && editor) return editor;
            editor = CreateEditor(obj);
            cache[obj] = editor;
            return editor;
        }

        private void DrawAbilityModuleInspector(AbilityModuleBase abilityModuleInstance, GUIStyle headerStyle)
        {
            if (!abilityModuleInstance) return;

            if (!_cachedEditors.TryGetValue(abilityModuleInstance, out UnityEditor.Editor abilityEditor) ||
                !abilityEditor)
            {
                abilityEditor = CreateEditor(abilityModuleInstance);
                _cachedEditors[abilityModuleInstance] = abilityEditor;
            }

            EditorGUI.BeginChangeCheck();

            abilityEditor.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(abilityModuleInstance);

                SerializedObject serializedAbility = new(abilityModuleInstance);
                serializedAbility.ApplyModifiedProperties();

                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(abilityModuleInstance)))
                {
                    AssetDatabase.SaveAssetIfDirty(abilityModuleInstance);
                }
            }
        }

        private string GetAbilityCategory(Type abilityType)
        {
            string category = "General";
            if (abilityType.Namespace is not null)
            {
                string[] namespaceParts = abilityType.Namespace.Split('.');
                if (namespaceParts.Length > 2)
                {
                    category = namespaceParts[^1];
                }
            }

            string typeName = abilityType.Name;
            if (typeName.EndsWith("AbilityModule"))
            {
                string baseTypeName = typeName[..^"AbilityModule".Length];
                if (baseTypeName.Contains("Attack") || baseTypeName.Contains("Weapon"))
                {
                    category = "Combat";
                }
                else if (baseTypeName.Contains("Move") || baseTypeName.Contains("Jump") ||
                         baseTypeName.Contains("Dash"))
                {
                    category = "Movement";
                }
                else if (baseTypeName.Contains("Shield") || baseTypeName.Contains("Defense"))
                {
                    category = "Defense";
                }
            }

            return category;
        }

        private Texture GetAbilityIcon(Type abilityType)
        {
            string iconName = abilityType.Name;
            Texture icon = null;

            string[] guids = AssetDatabase.FindAssets(iconName + " t:texture");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                icon = AssetDatabase.LoadAssetAtPath<Texture>(path);
            }

            if (!icon)
            {
                string category = GetAbilityCategory(abilityType);
                icon = category switch
                {
                    "Combat" => EditorGUIUtility.IconContent("d_Animation.Play").image,
                    "Movement" => EditorGUIUtility.IconContent("d_MoveTool").image,
                    "Defense" => EditorGUIUtility.IconContent("d_PreMatCube").image,
                    _ => EditorGUIUtility.IconContent("d_ScriptableObject Icon").image
                };
            }

            return icon;
        }
    }
}
#endif

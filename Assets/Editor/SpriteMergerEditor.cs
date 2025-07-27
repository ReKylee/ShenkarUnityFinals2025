using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// REQUIRED: The "2D Sprite" package must be installed via the Package Manager for this to compile.

namespace Editor
{
    public class SpriteMergerEditor : EditorWindow
    {

        // Undo tracking
        private static readonly string UndoGroupName = "Sprite Atlas Merge";
        private static readonly Dictionary<Object, Object> OriginalReferences = new();
        [SerializeField] private List<string> createdAssetPaths = new();
        [SerializeField] private string outputFileName = "MergedSpriteAtlas";
        private readonly List<Sprite> _spritesToMerge = new();
        private FilterMode _filterMode = FilterMode.Point;
        private bool _makeBackup = true;
        private int _maxAtlasSize = 4096;
        private int _padding = 2;
        private Vector2 _scrollPosition;
        private TextureFormat _textureFormat = TextureFormat.RGBA32;
        private bool _updatePrefabs = true;
        private bool _updateScenes = true;
        private bool _updateScriptableObjects = true;

        private void OnGUI()
        {
            GUILayout.Label("Sprite Atlas Merger", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Merge multiple sprite sheets into a single atlas and update ALL project references automatically.",
                MessageType.Info);

            EditorGUILayout.HelpBox("IMPORTANT: This tool requires the '2D Sprite' package. Make backups before use!",
                MessageType.Warning);

            DrawSpriteList();
            DrawOutputSettings();
            DrawUpdateOptions();
            DrawMergeButton();
        }

        [MenuItem("Tools/Sprite Atlas Merger")]
        public static void ShowWindow()
        {
            GetWindow<SpriteMergerEditor>("Atlas Merger");
        }

        private void DrawSpriteList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Sprites to Merge ({_spritesToMerge.Count})", EditorStyles.centeredGreyMiniLabel);

            // Drag and drop area
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Sprites or Textures Here\n(Individual sprites or entire sprite sheets)",
                EditorStyles.helpBox);

            ProcessDragAndDrop(dropArea);

            // Sprite list with scroll view
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
            for (int i = _spritesToMerge.Count - 1; i >= 0; i--)
            {
                if (_spritesToMerge[i] == null)
                {
                    _spritesToMerge.RemoveAt(i);
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                _spritesToMerge[i] = (Sprite)EditorGUILayout.ObjectField(_spritesToMerge[i], typeof(Sprite), false);

                // Show source texture info
                if (_spritesToMerge[i] != null)
                {
                    GUILayout.Label($"[{_spritesToMerge[i].texture.name}]", EditorStyles.miniLabel,
                        GUILayout.Width(100));
                }

                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _spritesToMerge.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Control buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected from Project"))
            {
                AddSelectedSprites();
            }

            if (GUILayout.Button("Add All from Folders"))
            {
                AddSpritesFromSelectedFolders();
            }

            if (GUILayout.Button("Clear All"))
            {
                _spritesToMerge.Clear();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawOutputSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Atlas Settings", EditorStyles.centeredGreyMiniLabel);

            outputFileName = EditorGUILayout.TextField("Output File Name", outputFileName);
            _padding = EditorGUILayout.IntSlider("Padding", _padding, 0, 10);
            _maxAtlasSize = EditorGUILayout.IntPopup("Max Atlas Size", _maxAtlasSize,
                new[] { "1024", "2048", "4096", "8192" },
                new[] { 1024, 2048, 4096, 8192 });

            _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _filterMode);
            _textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", _textureFormat);

            EditorGUILayout.EndVertical();
        }

        private void DrawUpdateOptions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Reference Update Options", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginHorizontal();
            _updateScenes = EditorGUILayout.Toggle(_updateScenes, GUILayout.Width(20));
            GUILayout.Label("Update Scene References", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _updatePrefabs = EditorGUILayout.Toggle(_updatePrefabs, GUILayout.Width(20));
            GUILayout.Label("Update Prefab References", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _updateScriptableObjects = EditorGUILayout.Toggle(_updateScriptableObjects, GUILayout.Width(20));
            GUILayout.Label("Update ScriptableObject References", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _makeBackup = EditorGUILayout.Toggle(_makeBackup, GUILayout.Width(20));
            GUILayout.Label("Create Backup Before Merge", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawMergeButton()
        {
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("Create Atlas and Update All References", GUILayout.Height(50)))
            {
                if (ValidateInput())
                {
                    if (_makeBackup && !CreateBackup())
                    {
                        EditorUtility.DisplayDialog("Backup Failed", "Could not create backup. Operation cancelled.",
                            "OK");

                        return;
                    }

                    MergeSpritesIntoAtlas();
                }
            }

            GUI.backgroundColor = Color.white;
        }

        private bool ValidateInput()
        {
            if (_spritesToMerge.Count < 2)
            {
                EditorUtility.DisplayDialog("Error", "Please select at least two sprites to merge.", "OK");
                return false;
            }

            if (string.IsNullOrEmpty(outputFileName))
            {
                EditorUtility.DisplayDialog("Error", "Please provide an output file name.", "OK");
                return false;
            }

            // Check for duplicate sprite names
            var duplicates = _spritesToMerge.Where(s => s != null)
                .GroupBy(s => s.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                string duplicateNames = string.Join(", ", duplicates);
                if (!EditorUtility.DisplayDialog("Duplicate Names Found",
                        $"Found duplicate sprite names: {duplicateNames}\n\nThis may cause reference update issues. Continue anyway?",
                        "Yes", "Cancel"))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CreateBackup()
        {
            try
            {
                string backupFolder = "Assets/SpriteAtlasBackup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                Directory.CreateDirectory(backupFolder);

                // Backup original textures
                var originalTextures = _spritesToMerge.Where(s => s != null)
                    .Select(s => s.texture)
                    .Distinct();

                foreach (Texture2D texture in originalTextures)
                {
                    string originalPath = AssetDatabase.GetAssetPath(texture);
                    if (!string.IsNullOrEmpty(originalPath))
                    {
                        string backupPath = Path.Combine(backupFolder, Path.GetFileName(originalPath));
                        AssetDatabase.CopyAsset(originalPath, backupPath);
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log($"Backup created at: {backupFolder}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create backup: {e.Message}");
                return false;
            }
        }

        private void ProcessDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (!dropArea.Contains(evt.mousePosition)) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        AddObjectToSpriteList(draggedObject);
                    }
                }
            }
        }

        private void AddObjectToSpriteList(Object obj)
        {
            if (obj == null) return;

            if (obj is Sprite sprite)
            {
                if (!_spritesToMerge.Contains(sprite))
                    _spritesToMerge.Add(sprite);
            }
            else if (obj is Texture2D texture)
            {
                // Add all sprites from this texture
                string path = AssetDatabase.GetAssetPath(texture);
                if (!string.IsNullOrEmpty(path))
                {
                    var spritesInTexture = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
                    foreach (Sprite s in spritesInTexture)
                    {
                        if (s != null && !_spritesToMerge.Contains(s))
                            _spritesToMerge.Add(s);
                    }
                }
            }
            else if (obj is DefaultAsset)
            {
                // Handle folder selection
                string path = AssetDatabase.GetAssetPath(obj);
                if (Directory.Exists(path))
                {
                    AddSpritesFromFolder(path);
                }
            }
        }

        private void AddSelectedSprites()
        {
            foreach (Object obj in Selection.objects)
            {
                AddObjectToSpriteList(obj);
            }
        }

        private void AddSpritesFromSelectedFolders()
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is DefaultAsset)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (Directory.Exists(path))
                    {
                        AddSpritesFromFolder(path);
                    }
                }
            }
        }

        private void AddSpritesFromFolder(string folderPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null && !_spritesToMerge.Contains(sprite))
                {
                    _spritesToMerge.Add(sprite);
                }
            }
        }

        private void MergeSpritesIntoAtlas()
        {
            string outputPath = EditorUtility.SaveFilePanelInProject("Save Atlas Texture", outputFileName, "png",
                "Save the merged atlas texture");

            if (string.IsNullOrEmpty(outputPath)) return;

            // Begin Undo group
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoGroupName);
            createdAssetPaths.Clear();
            OriginalReferences.Clear();

            EditorUtility.DisplayProgressBar("Creating Atlas", "Preparing sprites...", 0f);

            try
            {
                var validSprites = _spritesToMerge.Where(s => s != null && s.texture != null).ToList();
                if (validSprites.Count == 0)
                {
                    EditorUtility.DisplayDialog("Error", "No valid sprites found to merge.", "OK");
                    return;
                }

                // Create atlas texture
                Texture2D atlas = CreateAtlasTexture(validSprites);
                if (atlas == null)
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create atlas texture.", "OK");
                    return;
                }

                // Save atlas to disk
                byte[] bytes = atlas.EncodeToPNG();
                File.WriteAllBytes(outputPath, bytes);
                AssetDatabase.Refresh();

                // Track created asset for undo
                createdAssetPaths.Add(outputPath);
                RegisterCreatedAssetForUndo(outputPath);

                EditorUtility.DisplayProgressBar("Creating Atlas", "Configuring texture importer...", 0.5f);

                // Configure texture importer
                if (!ConfigureAtlasImporter(outputPath, validSprites, atlas))
                {
                    EditorUtility.DisplayDialog("Error", "Failed to configure atlas importer.", "OK");
                    return;
                }

                EditorUtility.DisplayProgressBar("Creating Atlas", "Updating references...", 0.7f);

                // Update all references
                UpdateAllProjectReferences(validSprites, outputPath);

                // Collapse undo group
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                Debug.Log("Atlas merge completed. Use Ctrl+Z to undo all changes made by this operation.");

                EditorUtility.DisplayDialog("Success",
                    "Atlas created successfully!\n" +
                    $"• Merged {validSprites.Count} sprites\n" +
                    $"• Atlas size: {atlas.width}x{atlas.height}\n" +
                    "• All project references updated" +
                    "• Use Ctrl+Z to undo if needed", "OK");
            }
            catch (Exception e)
            {
                // Revert undo group on error
                Undo.RevertAllInCurrentGroup();

                EditorUtility.DisplayDialog("Error", $"Failed to create atlas: {e.Message}", "OK");
                Debug.LogError($"Atlas creation failed: {e}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private Texture2D CreateAtlasTexture(List<Sprite> sprites)
        {
            // Group sprites by their source texture to handle sprite sheets properly
            var textureGroups = sprites.GroupBy(s => s.texture).ToList();
            var textureRects = new List<Texture2D>();

            // Create individual textures for each sprite
            for (int groupIndex = 0; groupIndex < textureGroups.Count(); groupIndex++)
            {
                var group = textureGroups[groupIndex];
                Texture2D sourceTexture = GetReadableTexture(group.Key);

                foreach (Sprite sprite in group)
                {
                    // Extract sprite region from source texture
                    Texture2D spriteTexture = ExtractSpriteTexture(sourceTexture, sprite);
                    if (spriteTexture)
                    {
                        textureRects.Add(spriteTexture);
                    }
                }
            }

            if (textureRects.Count == 0) return null;

            // Pack textures into atlas
            Texture2D atlas = new(1, 1, _textureFormat, false)
            {
                filterMode = _filterMode
            };

            atlas.PackTextures(textureRects.ToArray(), _padding, _maxAtlasSize, false);

            return atlas;
        }

        private Texture2D ExtractSpriteTexture(Texture2D sourceTexture, Sprite sprite)
        {

            Rect textureRect = sprite.textureRect;
            int width = Mathf.FloorToInt(textureRect.width);
            int height = Mathf.FloorToInt(textureRect.height);

            if (width <= 0 || height <= 0) return null;

            Texture2D spriteTexture = new(width, height, _textureFormat, false)
            {
                name = sprite.name
            };

            // Copy pixels from source texture
            var pixels = sourceTexture.GetPixels(
                Mathf.FloorToInt(textureRect.x),
                Mathf.FloorToInt(textureRect.y),
                width,
                height
            );

            spriteTexture.SetPixels(pixels);
            spriteTexture.Apply();

            return spriteTexture;
        }

        private Texture2D GetReadableTexture(Texture2D texture)
        {
            if (texture == null) return null;

            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                throw new InvalidOperationException($"Could not get TextureImporter for {texture.name}");
            }

            // If already readable, create a copy
            if (importer.isReadable)
            {
                Texture2D copy = new(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
                copy.name = texture.name;
                Graphics.CopyTexture(texture, copy);
                return copy;
            }

            // Create readable version using RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readableTexture = new(texture.width, texture.height);
            readableTexture.name = texture.name;
            readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }

        private bool ConfigureAtlasImporter(string atlasPath, List<Sprite> originalSprites, Texture2D atlas)
        {
            TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
            if (!importer) return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = _filterMode;
            importer.textureCompression = TextureImporterCompression.Uncompressed; // For better quality

            // Create sprite rectangles
            var spriteRects = new List<SpriteRect>();

            // Recreate the packing to get correct rectangles

            Texture2D tempAtlas = new(1, 1);
            var packedRects = tempAtlas.PackTextures(originalSprites
                .Select(sprite => new { sprite, sourceTexture = GetReadableTexture(sprite.texture) })
                .Select(t => ExtractSpriteTexture(t.sourceTexture, t.sprite))
                .Where(spriteTexture => spriteTexture is not null).ToArray(), _padding, _maxAtlasSize, false);

            for (int i = 0; i < originalSprites.Count && i < packedRects.Length; i++)
            {
                Sprite sprite = originalSprites[i];
                Rect packedRect = packedRects[i];

                SpriteRect spriteRect = new()
                {
                    name = sprite.name,
                    rect = new Rect(
                        packedRect.x * atlas.width,
                        packedRect.y * atlas.height,
                        packedRect.width * atlas.width,
                        packedRect.height * atlas.height
                    ),
                    pivot = sprite.textureRect.size == Vector2.zero
                        ? new Vector2(0.5f, 0.5f)
                        : sprite.pivot / sprite.textureRect.size,
                    border = sprite.border,
                    alignment = SpriteAlignment.Custom
                };

                spriteRects.Add(spriteRect);
            }

            // Apply sprite rectangles
            SpriteDataProviderFactories factories = new();
            factories.Init();
            ISpriteEditorDataProvider dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null)
            {
                Debug.LogError(
                    "Failed to get SpriteEditorDataProvider from importer. The atlas asset may not be fully imported yet.");

                return false;
            }

            dataProvider.InitSpriteEditorDataProvider();
            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            if (originalSprites.Count > 0 && originalSprites[0] && originalSprites[0].texture)
            {
                string srcPath = AssetDatabase.GetAssetPath(originalSprites[0].texture);
                TextureImporter srcImporter = AssetImporter.GetAtPath(srcPath) as TextureImporter;
                if (srcImporter)
                {
                    importer.sRGBTexture = srcImporter.sRGBTexture;
                    importer.mipmapEnabled = srcImporter.mipmapEnabled;
                    importer.filterMode = srcImporter.filterMode;
                    importer.anisoLevel = srcImporter.anisoLevel;
                    importer.wrapMode = srcImporter.wrapMode;
                    importer.npotScale = srcImporter.npotScale;
                    importer.alphaIsTransparency = srcImporter.alphaIsTransparency;
                    importer.compressionQuality = srcImporter.compressionQuality;
                    importer.textureCompression = srcImporter.textureCompression;
                    importer.isReadable = srcImporter.isReadable;
                    importer.maxTextureSize = srcImporter.maxTextureSize;
                    importer.spritePixelsPerUnit = srcImporter.spritePixelsPerUnit;
                }
            }

            return true;
        }

        private void UpdateAllProjectReferences(List<Sprite> originalSprites, string atlasPath)
        {
            // Create mapping from old sprites to new sprites
            var spriteMap = CreateSpriteMapping(originalSprites, atlasPath);
            if (spriteMap.Count == 0)
            {
                Debug.LogWarning("No sprite mapping created. References will not be updated.");
                return;
            }

            int totalUpdated = 0;
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/"))
                .ToArray();

            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string path = allAssetPaths[i];

                if (EditorUtility.DisplayCancelableProgressBar("Updating References",
                        $"Processing: {Path.GetFileName(path)}", (float)i / allAssetPaths.Length))
                {
                    break;
                }

                if (_updateScenes && path.EndsWith(".unity"))
                {
                    totalUpdated += UpdateSceneReferences(path, spriteMap);
                }
                else if (_updatePrefabs && path.EndsWith(".prefab"))
                {
                    totalUpdated += UpdateAssetReferences(path, spriteMap);
                }
                else if (_updateScriptableObjects && path.EndsWith(".asset"))
                {
                    totalUpdated += UpdateAssetReferences(path, spriteMap);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully updated {totalUpdated} sprite references across the project.");
        }

        private Dictionary<int, Sprite> CreateSpriteMapping(List<Sprite> originalSprites, string atlasPath)
        {
            var mapping = new Dictionary<int, Sprite>();
            var newSprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath).OfType<Sprite>().ToList();

            foreach (Sprite originalSprite in originalSprites)
            {
                if (originalSprite == null) continue;

                Sprite newSprite = newSprites.FirstOrDefault(s => s != null && s.name == originalSprite.name);
                if (newSprite != null)
                {
                    mapping[originalSprite.GetInstanceID()] = newSprite;
                }
                else
                {
                    Debug.LogWarning($"Could not find matching sprite '{originalSprite.name}' in new atlas.");
                }
            }

            return mapping;
        }

        private int UpdateSceneReferences(string scenePath, Dictionary<int, Sprite> spriteMap)
        {
            int count = 0;
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!scene.IsValid()) return 0;

            bool sceneModified = false;

            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                if (WillGameObjectBeModified(rootGo, spriteMap))
                {
                    Undo.RegisterCompleteObjectUndo(rootGo, UndoGroupName);
                }
            }

            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                count += UpdateGameObjectReferences(rootGo, spriteMap, ref sceneModified);
            }

            if (sceneModified)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Updated {count} references in scene: {scene.name}");
            }

            EditorSceneManager.CloseScene(scene, false);
            return count;
        }

        private int UpdateGameObjectReferences(GameObject go, Dictionary<int, Sprite> spriteMap, ref bool modified)
        {
            int count = 0;

            foreach (Component component in go.GetComponentsInChildren<Component>(true))
            {
                if (!component) continue;

                // Register component for undo before modification
                bool componentWillBeModified = WillComponentBeModified(component, spriteMap);
                if (componentWillBeModified)
                {
                    Undo.RecordObject(component, UndoGroupName);
                }

                SerializedObject so = new(component);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue is Sprite oldSprite &&
                        oldSprite != null &&
                        spriteMap.TryGetValue(oldSprite.GetInstanceID(), out Sprite newSprite))
                    {
                        // Store original reference for potential undo
                        if (!OriginalReferences.ContainsKey(component))
                        {
                            OriginalReferences[component] = oldSprite;
                        }

                        prop.objectReferenceValue = newSprite;
                        modified = true;
                        count++;
                    }
                }

                if (so.hasModifiedProperties)
                {
                    so.ApplyModifiedProperties();
                    if (componentWillBeModified)
                    {
                        EditorUtility.SetDirty(component);
                    }
                }
            }

            return count;
        }

        private int UpdateAssetReferences(string assetPath, Dictionary<int, Sprite> spriteMap)
        {
            int count = 0;
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in assets)
            {
                if (!asset || asset is Texture2D || asset is Sprite) continue;

                // Check if this asset will be modified and register for undo
                bool assetWillBeModified = WillAssetBeModified(asset, spriteMap);
                if (assetWillBeModified)
                {
                    Undo.RecordObject(asset, UndoGroupName);
                }

                SerializedObject so = new(asset);
                SerializedProperty prop = so.GetIterator();
                bool assetModified = false;

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue is Sprite oldSprite &&
                        oldSprite != null &&
                        spriteMap.TryGetValue(oldSprite.GetInstanceID(), out Sprite newSprite))
                    {
                        // Store original reference for potential undo
                        if (!OriginalReferences.ContainsKey(asset))
                        {
                            OriginalReferences[asset] = oldSprite;
                        }

                        prop.objectReferenceValue = newSprite;
                        assetModified = true;
                        count++;
                    }
                }

                if (assetModified)
                {
                    so.ApplyModifiedProperties();
                    if (assetWillBeModified)
                    {
                        EditorUtility.SetDirty(asset);
                    }
                }
            }

            return count;
        }

        #region Undo Support Methods

        private void RegisterCreatedAssetForUndo(string assetPath)
        {
            // Custom undo operation for created assets
            Undo.RegisterCreatedObjectUndo(AssetDatabase.LoadAssetAtPath<Object>(assetPath), UndoGroupName);
        }

        private bool WillGameObjectBeModified(GameObject go, Dictionary<int, Sprite> spriteMap)
        {
            foreach (Component component in go.GetComponentsInChildren<Component>(true))
            {
                if (component != null && WillComponentBeModified(component, spriteMap))
                {
                    return true;
                }
            }

            return false;
        }

        private bool WillComponentBeModified(Component component, Dictionary<int, Sprite> spriteMap)
        {
            if (component == null) return false;

            SerializedObject so = new(component);
            SerializedProperty prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                    prop.objectReferenceValue is Sprite sprite &&
                    sprite != null &&
                    spriteMap.ContainsKey(sprite.GetInstanceID()))
                {
                    return true;
                }
            }

            return false;
        }

        private bool WillAssetBeModified(Object asset, Dictionary<int, Sprite> spriteMap)
        {
            if (asset == null || asset is Texture2D || asset is Sprite) return false;

            SerializedObject so = new(asset);
            SerializedProperty prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                    prop.objectReferenceValue is Sprite sprite &&
                    sprite != null &&
                    spriteMap.ContainsKey(sprite.GetInstanceID()))
                {
                    return true;
                }
            }

            return false;
        }

        // Custom undo callback for cleaning up created assets
        [Serializable]
        public class AtlasMergeUndoOperation : ScriptableObject
        {
            public List<string> createdAssetPaths = new();

            private void OnEnable()
            {
                Undo.undoRedoPerformed += OnUndoRedo;
            }

            private void OnDisable()
            {
                Undo.undoRedoPerformed -= OnUndoRedo;
            }

            private void OnUndoRedo()
            {
                // Clean up created assets when undoing
                foreach (string path in createdAssetPaths)
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }

                AssetDatabase.Refresh();
            }
        }

        #endregion

    }
}

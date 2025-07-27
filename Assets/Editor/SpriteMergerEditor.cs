using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Editor
{
    public class SpriteMergerEditor : EditorWindow
    {
        private readonly List<Sprite> _spritesToMerge = new List<Sprite>();
        private string _outputFileName = "MergedSpriteAtlas";
        private int _padding = 2;
        private int _maxAtlasSize = 4096;
        private bool _updateScenes = true;
        private bool _updatePrefabs = true;
        private bool _updateScriptableObjects = true;
        private bool _makeBackup = true;
        private Vector2 _scrollPosition;
        private FilterMode _filterMode = FilterMode.Point;
        private TextureFormat _textureFormat = TextureFormat.RGBA32;

        [MenuItem("Tools/Enhanced Sprite Atlas Merger")]
        public static void ShowWindow()
        {
            GetWindow<SpriteMergerEditor>("Enhanced Atlas Merger");
        }

        void OnGUI()
        {
            GUILayout.Label("Enhanced Sprite Atlas Merger", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Merge multiple sprite sheets into a single atlas and update ALL project references automatically.", MessageType.Info);
            EditorGUILayout.HelpBox("IMPORTANT: This tool requires the '2D Sprite' package. Make backups before use!", MessageType.Warning);

            DrawSpriteList();
            DrawOutputSettings();
            DrawUpdateOptions();
            DrawMergeButton();
        }

        private void DrawSpriteList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Sprites to Merge ({_spritesToMerge.Count})", EditorStyles.centeredGreyMiniLabel);
        
            // Drag and drop area
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Sprites or Textures Here\n(Individual sprites or entire sprite sheets)", EditorStyles.helpBox);
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
                    GUILayout.Label($"[{_spritesToMerge[i].texture.name}]", EditorStyles.miniLabel, GUILayout.Width(100));
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
        
            _outputFileName = EditorGUILayout.TextField("Output File Name", _outputFileName);
            _padding = EditorGUILayout.IntSlider("Padding", _padding, 0, 10);
            _maxAtlasSize = EditorGUILayout.IntPopup("Max Atlas Size", _maxAtlasSize, 
                new string[] { "1024", "2048", "4096", "8192" }, 
                new int[] { 1024, 2048, 4096, 8192 });
            _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _filterMode);
            _textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", _textureFormat);
        
            EditorGUILayout.EndVertical();
        }

        private void DrawUpdateOptions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Reference Update Options", EditorStyles.centeredGreyMiniLabel);
        
            _updateScenes = EditorGUILayout.Toggle("Update Scene References", _updateScenes);
            _updatePrefabs = EditorGUILayout.Toggle("Update Prefab References", _updatePrefabs);
            _updateScriptableObjects = EditorGUILayout.Toggle("Update ScriptableObject References", _updateScriptableObjects);
            _makeBackup = EditorGUILayout.Toggle("Create Backup Before Merge", _makeBackup);
        
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
                        EditorUtility.DisplayDialog("Backup Failed", "Could not create backup. Operation cancelled.", "OK");
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
        
            if (string.IsNullOrEmpty(_outputFileName))
            {
                EditorUtility.DisplayDialog("Error", "Please provide an output file name.", "OK");
                return false;
            }
        
            // Check for duplicate sprite names
            var duplicates = _spritesToMerge.Where(s => s != null)
                .GroupBy(s => s.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
        
            if (duplicates.Any())
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
                string backupFolder = "Assets/SpriteAtlasBackup_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                Directory.CreateDirectory(backupFolder);
            
                // Backup original textures
                var originalTextures = _spritesToMerge.Where(s => s != null)
                    .Select(s => s.texture)
                    .Distinct();
            
                foreach (var texture in originalTextures)
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
            catch (System.Exception e)
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
                    foreach (var s in spritesInTexture)
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
            string outputPath = EditorUtility.SaveFilePanelInProject("Save Atlas Texture", _outputFileName, "png", "Save the merged atlas texture");
            if (string.IsNullOrEmpty(outputPath)) return;

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

                EditorUtility.DisplayDialog("Success", 
                    $"Atlas created successfully!\n" +
                    $"• Merged {validSprites.Count} sprites\n" +
                    $"• Atlas size: {atlas.width}x{atlas.height}\n" +
                    $"• All project references updated", "OK");
            }
            catch (System.Exception e)
            {
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
            var spriteToTextureMap = new Dictionary<Sprite, int>();

            // Create individual textures for each sprite
            for (int groupIndex = 0; groupIndex < textureGroups.Count(); groupIndex++)
            {
                var group = textureGroups[groupIndex];
                Texture2D sourceTexture = GetReadableTexture(group.Key);
            
                foreach (var sprite in group)
                {
                    // Extract sprite region from source texture
                    Texture2D spriteTexture = ExtractSpriteTexture(sourceTexture, sprite);
                    if (spriteTexture != null)
                    {
                        spriteToTextureMap[sprite] = textureRects.Count;
                        textureRects.Add(spriteTexture);
                    }
                }
            }

            if (textureRects.Count == 0) return null;

            // Pack textures into atlas
            Texture2D atlas = new Texture2D(1, 1, _textureFormat, false);
            atlas.filterMode = _filterMode;
        
            Rect[] packedRects = atlas.PackTextures(textureRects.ToArray(), _padding, _maxAtlasSize, false);
        
            return atlas;
        }

        private Texture2D ExtractSpriteTexture(Texture2D sourceTexture, Sprite sprite)
        {
            if (sourceTexture == null || sprite == null) return null;

            var textureRect = sprite.textureRect;
            int width = Mathf.FloorToInt(textureRect.width);
            int height = Mathf.FloorToInt(textureRect.height);
        
            if (width <= 0 || height <= 0) return null;

            Texture2D spriteTexture = new Texture2D(width, height, _textureFormat, false);
            spriteTexture.name = sprite.name;

            // Copy pixels from source texture
            Color[] pixels = sourceTexture.GetPixels(
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
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                throw new System.InvalidOperationException($"Could not get TextureImporter for {texture.name}");
            }

            // If already readable, create a copy
            if (importer.isReadable)
            {
                var copy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
                copy.name = texture.name;
                Graphics.CopyTexture(texture, copy);
                return copy;
            }
        
            // Create readable version using RenderTexture
            var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, rt);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
        
            var readableTexture = new Texture2D(texture.width, texture.height);
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
            if (importer == null) return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = _filterMode;
            importer.textureCompression = TextureImporterCompression.Uncompressed; // For better quality

            // Create sprite rectangles
            var spriteRects = new List<SpriteRect>();
            var textureRects = new List<Texture2D>();

            // Recreate the packing to get correct rectangles
            foreach (var sprite in originalSprites)
            {
                var sourceTexture = GetReadableTexture(sprite.texture);
                var spriteTexture = ExtractSpriteTexture(sourceTexture, sprite);
                if (spriteTexture != null)
                {
                    textureRects.Add(spriteTexture);
                }
            }

            Texture2D tempAtlas = new Texture2D(1, 1);
            Rect[] packedRects = tempAtlas.PackTextures(textureRects.ToArray(), _padding, _maxAtlasSize, false);

            for (int i = 0; i < originalSprites.Count && i < packedRects.Length; i++)
            {
                var sprite = originalSprites[i];
                var packedRect = packedRects[i];

                var spriteRect = new SpriteRect
                {
                    name = sprite.name,
                    rect = new Rect(
                        packedRect.x * atlas.width,
                        packedRect.y * atlas.height,
                        packedRect.width * atlas.width,
                        packedRect.height * atlas.height
                    ),
                    pivot = sprite.textureRect.size == Vector2.zero ? 
                        new Vector2(0.5f, 0.5f) : 
                        sprite.pivot / sprite.textureRect.size,
                    border = sprite.border,
                    alignment = SpriteAlignment.Custom
                };
                spriteRects.Add(spriteRect);
            }

            // Apply sprite rectangles
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null) return false;

            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

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

            foreach (var originalSprite in originalSprites)
            {
                if (originalSprite == null) continue;

                var newSprite = newSprites.FirstOrDefault(s => s != null && s.name == originalSprite.name);
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
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!scene.IsValid()) return 0;

            bool sceneModified = false;

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
                if (component == null) continue;

                var so = new SerializedObject(component);
                var prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue is Sprite oldSprite &&
                        oldSprite != null &&
                        spriteMap.TryGetValue(oldSprite.GetInstanceID(), out Sprite newSprite))
                    {
                        prop.objectReferenceValue = newSprite;
                        modified = true;
                        count++;
                    }
                }

                if (so.hasModifiedProperties)
                {
                    so.ApplyModifiedProperties();
                }
            }

            return count;
        }

        private int UpdateAssetReferences(string assetPath, Dictionary<int, Sprite> spriteMap)
        {
            int count = 0;
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in assets)
            {
                if (asset == null || asset is Texture2D || asset is Sprite) continue;

                var so = new SerializedObject(asset);
                var prop = so.GetIterator();
                bool assetModified = false;

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue is Sprite oldSprite &&
                        oldSprite != null &&
                        spriteMap.TryGetValue(oldSprite.GetInstanceID(), out Sprite newSprite))
                    {
                        prop.objectReferenceValue = newSprite;
                        assetModified = true;
                        count++;
                    }
                }

                if (assetModified)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(asset);
                }
            }

            return count;
        }
    }
}
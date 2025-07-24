using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
// REQUIRED: The "2D Sprite" package must be installed via the Package Manager for this to compile.
using UnityEditor.U2D.Sprites; 

public class SpriteMergerEditor : EditorWindow
{
    private List<Sprite> spritesToMerge = new List<Sprite>();
    private string outputFileName = "MergedSpriteSheet";
    private int padding = 1;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Advanced Sprite Merger")]
    public static void ShowWindow()
    {
        GetWindow<SpriteMergerEditor>("Advanced Sprite Merger");
    }

    void OnGUI()
    {
        GUILayout.Label("Advanced Sprite Merger", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select sprites, merge them into an atlas, and update ALL references across the entire project (Prefabs, Scenes, ScriptableObjects, MonoBehaviours, etc.).", MessageType.Info);
        EditorGUILayout.HelpBox("NOTE: This tool requires the '2D Sprite' package to be installed from the Unity Package Manager.", MessageType.Warning);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Sprites to Merge", EditorStyles.centeredGreyMiniLabel);
        
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Sprites Here");
        ProcessDragAndDrop(dropArea);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        for (int i = spritesToMerge.Count - 1; i >= 0; i--)
        {
            if (spritesToMerge[i] == null)
            {
                spritesToMerge.RemoveAt(i);
                continue;
            }
            EditorGUILayout.BeginHorizontal();
            spritesToMerge[i] = (Sprite)EditorGUILayout.ObjectField(spritesToMerge[i], typeof(Sprite), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                spritesToMerge.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Selected Sprites from Project View"))
        {
            AddSelectedSprites();
        }
        if (GUILayout.Button("Clear List"))
        {
            spritesToMerge.Clear();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Output Settings", EditorStyles.centeredGreyMiniLabel);
        outputFileName = EditorGUILayout.TextField("Output File Name", outputFileName);
        padding = EditorGUILayout.IntField("Padding", padding);
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("Merge Sprites and Update All References", GUILayout.Height(40)))
        {
            if (spritesToMerge.Count > 1 && !string.IsNullOrEmpty(outputFileName))
            {
                MergeSprites();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select at least two sprites and provide an output file name.", "OK");
            }
        }
        GUI.backgroundColor = Color.white;
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
                    if (draggedObject == null) continue;

                    if (draggedObject is Sprite sprite)
                    {
                        if (!spritesToMerge.Contains(sprite)) spritesToMerge.Add(sprite);
                    }
                    else if (draggedObject is Texture2D tex)
                    {
                        string path = AssetDatabase.GetAssetPath(tex);
                        if (string.IsNullOrEmpty(path)) continue;

                        IEnumerable<Sprite> spritesInTexture = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
                        foreach(var s in spritesInTexture)
                        {
                             if (s != null && !spritesToMerge.Contains(s)) spritesToMerge.Add(s);
                        }
                    }
                }
            }
        }
    }

    private void AddSelectedSprites()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj == null) continue;

            if (obj is Sprite sprite && !spritesToMerge.Contains(sprite))
            {
                spritesToMerge.Add(sprite);
            }
        }
    }

    private void MergeSprites()
    {
        string outputPath = EditorUtility.SaveFilePanelInProject("Save Merged Texture", outputFileName, "png", "Please enter a file name to save the texture to.");
        if (string.IsNullOrEmpty(outputPath)) return;

        var validSprites = spritesToMerge.Where(s => s != null && s.texture != null).ToList();
        var texturesToPack = validSprites.Select(s => s.texture).Distinct().ToList();
        var readableTextures = new List<Texture2D>();

        try
        {
            foreach (var tex in texturesToPack)
            {
                readableTextures.Add(GetReadableTexture(tex));
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Could not process one of the textures. Make sure it's readable in import settings. Error: {e.Message}", "OK");
            return;
        }

        Texture2D atlas = new Texture2D(1, 1);
        Rect[] packedRects = atlas.PackTextures(readableTextures.ToArray(), padding, 8192, false);

        byte[] bytes = atlas.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
        AssetDatabase.Refresh();

        TextureImporter textureImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (textureImporter == null)
        {
            Debug.LogError($"Failed to get TextureImporter for the new atlas at {outputPath}. Aborting.");
            return;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;

        var spriteRects = new List<SpriteRect>();
        var rectsByTextureName = new Dictionary<string, Rect>();
        for(int i = 0; i < readableTextures.Count; i++)
        {
            if (readableTextures[i] == null) continue;
            rectsByTextureName[readableTextures[i].name] = packedRects[i];
        }

        foreach (Sprite sprite in validSprites)
        {
            if (!rectsByTextureName.TryGetValue(sprite.texture.name, out Rect newRectInAtlas))
            {
                Debug.LogWarning($"Could not find packed rect for texture '{sprite.texture.name}'. Skipping sprite '{sprite.name}'.");
                continue;
            }

            var spriteRect = new SpriteRect
            {
                name = sprite.name,
                rect = new Rect(
                    newRectInAtlas.x * atlas.width + sprite.textureRect.x,
                    newRectInAtlas.y * atlas.height + sprite.textureRect.y,
                    sprite.textureRect.width,
                    sprite.textureRect.height
                ),
                pivot = sprite.textureRect.size == Vector2.zero ? new Vector2(0.5f, 0.5f) : sprite.pivot / sprite.textureRect.size,
                border = sprite.border,
                alignment = SpriteAlignment.Custom
            };
            spriteRects.Add(spriteRect);
        }

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dataProvider = factories.GetSpriteEditorDataProviderFromObject(textureImporter);
        if (dataProvider == null)
        {
            Debug.LogError($"Failed to create ISpriteEditorDataProvider for the new atlas at {outputPath}. This is likely because the '2D Sprite' package is not installed. Aborting.");
            return;
        }
        dataProvider.SetSpriteRects(spriteRects.ToArray());

        dataProvider.Apply();
        EditorUtility.SetDirty(textureImporter);
        textureImporter.SaveAndReimport();

        UpdateAllReferences(validSprites, outputPath);

        EditorUtility.DisplayDialog("Success", "Sprites merged and all project references updated successfully!", "OK");
    }
    
    private Texture2D GetReadableTexture(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            throw new System.InvalidOperationException($"Could not get a TextureImporter for {texture.name}. It might be a built-in asset.");
        }

        if (importer.isReadable)
        {
             var copy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
             copy.name = texture.name;
             Graphics.CopyTexture(texture, copy);
             return copy;
        }
        
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

    private void UpdateAllReferences(IEnumerable<Sprite> oldSprites, string newAtlasPath)
    {
        var oldSpriteInstanceIdToNewSprite = new Dictionary<int, Sprite>();
        var newSprites = AssetDatabase.LoadAllAssetsAtPath(newAtlasPath).OfType<Sprite>();

        foreach (Sprite oldSprite in oldSprites)
        {
            Sprite newSprite = newSprites.FirstOrDefault(s => s != null && s.name == oldSprite.name);
            if (newSprite != null)
            {
                oldSpriteInstanceIdToNewSprite[oldSprite.GetInstanceID()] = newSprite;
            }
            else
            {
                Debug.LogWarning($"Could not find a matching sprite in the new atlas for old sprite '{oldSprite.name}'. References to it will not be updated.");
            }
        }

        if (oldSpriteInstanceIdToNewSprite.Count == 0)
        {
            Debug.LogError("Failed to map any of the old sprites to the new atlas. Aborting reference update.");
            return;
        }

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        int updatedCount = 0;

        try
        {
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string path = allAssetPaths[i];
                if (!path.StartsWith("Assets/")) continue;

                if (EditorUtility.DisplayCancelableProgressBar("Updating All References", $"Processing: {Path.GetFileName(path)}", (float)i / allAssetPaths.Length))
                {
                    break;
                }

                if (path.EndsWith(".unity"))
                {
                    updatedCount += ProcessScene(path, oldSpriteInstanceIdToNewSprite);
                }
                else if (path.EndsWith(".prefab") || path.EndsWith(".asset"))
                {
                    updatedCount += ProcessAsset(path, oldSpriteInstanceIdToNewSprite);
                }
            }
        }
        finally
        {
            // The saving is now handled inside ProcessScene, so this is just for other assets.
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"Successfully updated {updatedCount} sprite references across the project.");
    }

    private int ProcessScene(string scenePath, IReadOnlyDictionary<int, Sprite> replacementMap)
    {
        int count = 0;
        // Open the scene additively to inspect it without closing the user's current scene.
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        if (!scene.IsValid()) return 0;
        
        bool sceneModified = false;

        foreach (GameObject rootGo in scene.GetRootGameObjects())
        {
            if (rootGo == null) continue;

            foreach (Component component in rootGo.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;

                var so = new SerializedObject(component);
                var prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue is Sprite oldSprite &&
                        oldSprite != null &&
                        replacementMap.TryGetValue(oldSprite.GetInstanceID(), out Sprite newSprite))
                    {
                        prop.objectReferenceValue = newSprite;
                        sceneModified = true;
                        count++;
                    }
                }
                if (so.hasModifiedProperties)
                {
                    // Apply changes to the component. This supports Undo.
                    so.ApplyModifiedProperties();
                }
            }
        }

        // --- FIXED: Correctly save the scene if it was modified ---
        if (sceneModified)
        {
            Debug.Log($"Found and replaced {count} sprite(s) in scene: {scene.name}. Saving...");
            EditorSceneManager.SaveScene(scene);
        }
        
        // Now close the scene without saving, as we've already handled it.
        EditorSceneManager.CloseScene(scene, false); 
        // --- END FIX ---

        return count;
    }

    private int ProcessAsset(string assetPath, IReadOnlyDictionary<int, Sprite> replacementMap)
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
                    replacementMap.TryGetValue(oldSprite.GetInstanceID(), out Sprite newSprite))
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

using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class SceneSnappingTool : EditorWindow
    {
        private float _pixelsPerUnit = 100f;

        // This method draws the UI for the editor window.
        private void OnGUI()
        {
            GUILayout.Label("Scene Snapping Tools", EditorStyles.boldLabel);

            // Scene Objects Section
            GUILayout.Space(10);
            GUILayout.Label("Snap Scene Objects", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool will iterate through every object in the active scene and snap its position to the pixel grid defined by the PPU value below. This is a destructive action, but it is undoable.",
                MessageType.Info);

            // Create a field for the user to input the Pixels Per Unit value.
            _pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit (PPU)", _pixelsPerUnit);

            // Add some space before the button.
            GUILayout.Space(10);

            // Change the GUI color to make the button more prominent.
            GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);

            // Create the button that will trigger the snapping logic.
            if (GUILayout.Button("Snap All Objects in Scene"))
            {
                if (_pixelsPerUnit > 0)
                {
                    SnapAllObjects();
                }
                else
                {
                    // Show an error if the PPU is not a valid number.
                    EditorUtility.DisplayDialog("Invalid PPU", "Pixels Per Unit must be greater than 0.", "OK");
                }
            }

            // Sprite Pivots Section
            GUILayout.Space(20);
            GUILayout.Label("Snap Sprite Pivots", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select sprite textures in the Project window, then click the button below to snap all sprite pivots to the nearest integer values.",
                MessageType.Info);

            // Change button color for sprite pivot snapping
            GUI.backgroundColor = new Color(0.8f, 0.8f, 1f);

            if (GUILayout.Button("Snap Selected Sprite Pivots"))
            {
                SnapSpritePivots();
            }

            // Reset the color to default.
            GUI.backgroundColor = Color.white;
        }

        // Creates a new menu item under "Tools" to open this window.
        [MenuItem("Tools/Scene Snapping Tool")]
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one.
            GetWindow<SceneSnappingTool>("Scene Snapper");
        }

        private void SnapAllObjects()
        {
            // Get all root GameObjects in the currently active scene.
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            int snappedObjectCount = 0;

            // Register an undo operation for the entire scene snapping action.
            Undo.SetCurrentGroupName("Snap All Scene Objects");
            int group = Undo.GetCurrentGroup();

            foreach (GameObject root in rootObjects)
            {
                // Use GetComponentsInChildren to get the transform of the root and all its children.
                var allTransforms = root.GetComponentsInChildren<Transform>();
                foreach (Transform trans in allTransforms)
                {
                    // Record the object's state before we modify it, allowing for undo.
                    Undo.RecordObject(trans, "Snap Position");

                    // Snap the position and update the object.
                    trans.position = SnapToPixelGrid(trans.position);
                    snappedObjectCount++;
                }
            }

            // Finalize the undo group.
            Undo.CollapseUndoOperations(group);

            // Log a confirmation message to the console.
            Debug.Log($"Successfully snapped {snappedObjectCount} objects in the scene.");

            // Show a popup to the user confirming the action is complete.
            EditorUtility.DisplayDialog("Snapping Complete", $"Snapped {snappedObjectCount} objects to the grid.",
                "OK");
        }

        private void SnapSpritePivots()
        {
            // Get all selected textures
            var selectedObjects = Selection.objects;
            int processedSprites = 0;

            foreach (Object obj in selectedObjects)
            {
                if (obj is Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (importer && importer.textureType == TextureImporterType.Sprite)
                    {
                        // Handle sprite sheet with multiple sprites
                        if (importer.spriteImportMode == SpriteImportMode.Multiple)
                        {
                            SpriteDataProviderFactories factory = new();
                            factory.Init();
                            ISpriteEditorDataProvider dataProvider =
                                factory.GetSpriteEditorDataProviderFromObject(importer);

                            dataProvider.InitSpriteEditorDataProvider();

                            var spriteRects = dataProvider.GetSpriteRects();

                            for (int i = 0; i < spriteRects.Length; i++)
                            {
                                SpriteRect spriteRect = spriteRects[i];

                                // Convert normalized pivot (0-1) to pixel coordinates
                                float pixelPivotX = spriteRect.pivot.x * spriteRect.rect.width;
                                float pixelPivotY = spriteRect.pivot.y * spriteRect.rect.height;

                                // Snap to nearest integer
                                float snappedPixelPivotX = Mathf.Round(pixelPivotX);
                                float snappedPixelPivotY = Mathf.Round(pixelPivotY);

                                // Convert back to normalized coordinates
                                spriteRect.pivot = new Vector2(
                                    snappedPixelPivotX / spriteRect.rect.width,
                                    snappedPixelPivotY / spriteRect.rect.height
                                );

                                spriteRects[i] = spriteRect;
                                processedSprites++;
                            }

                            dataProvider.SetSpriteRects(spriteRects);
                            dataProvider.Apply();
                        }
                        // Handle single sprite
                        else if (importer.spriteImportMode == SpriteImportMode.Single)
                        {
                            Vector2 pivot = importer.spritePivot;

                            // For single sprites, spritePivot is already in pixel coordinates
                            importer.spritePivot = new Vector2(
                                Mathf.Round(pivot.x),
                                Mathf.Round(pivot.y)
                            );

                            processedSprites++;
                        }

                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"Snapped pivots for: {texture.name}");
                    }
                }
            }

            AssetDatabase.Refresh();

            if (processedSprites > 0)
            {
                Debug.Log($"Pivot snapping complete! Processed {processedSprites} sprites.");
                EditorUtility.DisplayDialog("Pivot Snapping Complete",
                    $"Successfully snapped pivots for {processedSprites} sprites.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Sprites Selected",
                    "Please select sprite textures in the Project window before running this tool.", "OK");
            }
        }

        /// <summary>
        ///     Snaps a given world position to the nearest pixel boundary based on the PPU setting.
        /// </summary>
        private Vector3 SnapToPixelGrid(Vector3 position)
        {
            // Calculate the size of a single pixel in world units.
            float pixelSize = 1.0f / _pixelsPerUnit;

            // Snap the X and Y coordinates to the nearest pixel boundary.
            float x = Mathf.Round(position.x / pixelSize) * pixelSize;
            float y = Mathf.Round(position.y / pixelSize) * pixelSize;

            // We don't snap the Z-axis, so we preserve its original value.
            return new Vector3(x, y, position.z);
        }
    }
}

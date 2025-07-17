using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SceneSnappingTool : EditorWindow
    {
        private float _pixelsPerUnit = 100f;

        // Creates a new menu item under "Tools" to open this window.
        [MenuItem("Tools/Scene Snapping Tool")]
        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one.
            GetWindow<SceneSnappingTool>("Scene Snapper");
        }

        // This method draws the UI for the editor window.
        void OnGUI()
        {
            GUILayout.Label("Snap All Scene Objects to Grid", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tool will iterate through every object in the active scene and snap its position to the pixel grid defined by the PPU value below. This is a destructive action, but it is undoable.", MessageType.Info);

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

            // Reset the color to default.
            GUI.backgroundColor = Color.white;
        }

        private void SnapAllObjects()
        {
            // Get all root GameObjects in the currently active scene.
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
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
            EditorUtility.DisplayDialog("Snapping Complete", $"Snapped {snappedObjectCount} objects to the grid.", "OK");
        }

        /// <summary>
        /// Snaps a given world position to the nearest pixel boundary based on the PPU setting.
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

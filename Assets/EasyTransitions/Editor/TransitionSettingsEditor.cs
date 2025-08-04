using UnityEditor;
using UnityEngine;

namespace EasyTransition
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TransitionSettings))]
    public class TransitionSettingsEditor : UnityEditor.Editor
    {
        public Texture transitionManagerSettingsLogo;
        private SerializedProperty transitionsList;

        private void OnEnable()
        {
            transitionsList = serializedObject.FindProperty("transitions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Texture2D bgTexture = new(1, 1, TextureFormat.RGBAFloat, false);
            GUIStyle style = new(GUI.skin.box);
            style.normal.background = bgTexture;

            GUILayout.Box(transitionManagerSettingsLogo, style, GUILayout.Width(Screen.width - 20),
                GUILayout.Height(Screen.height / 15));

            EditorGUILayout.Space();

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }

}

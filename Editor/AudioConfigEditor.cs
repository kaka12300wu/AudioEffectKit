using UnityEngine;
using UnityEditor;

namespace AudioEffectKit.Editor
{
    [CustomEditor(typeof(AudioConfig))]
    public class AudioConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AudioConfig config = (AudioConfig)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Config"))
            {
                bool isValid = config.ValidateConfig();
                EditorUtility.DisplayDialog("Validation Result", 
                    isValid ? "Configuration is valid!" : "Configuration has errors. Check console for details.", 
                    "OK");
            }

            if (GUILayout.Button("Create Default"))
            {
                if (EditorUtility.DisplayDialog("Create Default Config", 
                    "This will reset all settings to default values. Continue?", 
                    "Yes", "Cancel"))
                {
                    var defaultConfig = AudioConfig.CreateDefault();
                    EditorUtility.CopySerialized(defaultConfig, config);
                    DestroyImmediate(defaultConfig);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Audio Clips: {config.AudioClips.Count}");
            
            foreach (AudioCategory category in System.Enum.GetValues(typeof(AudioCategory)))
            {
                int count = config.GetAudioClipsByCategory(category).Count;
                EditorGUILayout.LabelField($"{category}: {count} clips");
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
            }
        }
    }
}
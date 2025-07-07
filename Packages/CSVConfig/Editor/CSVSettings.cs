using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GSDev.CSVConfig.Editor
{
    [Serializable]
    public class CSVSettings : ScriptableObject
    {
        public const string SettingsPath = "Assets/Editor/CSVSettings.asset";
        
        [SerializeField]
        private DefaultAsset _defaultConfigFolder;

        public string DefaultConfigPath => _defaultConfigFolder == null 
            ? "" 
            : AssetDatabase.GetAssetPath(_defaultConfigFolder);
        
        [SerializeField]
        private DefaultAsset _scriptOutputFolder;

        public string ScriptOutputPath => _scriptOutputFolder == null 
            ? "" 
            : AssetDatabase.GetAssetPath(_scriptOutputFolder);

        internal static CSVSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CSVSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<CSVSettings>();
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/CSV Settings", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                // label = "CSV Settings",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = CSVSettings.GetOrCreateSettings();
                    var serializedSettings = new SerializedObject(settings);
                    EditorGUILayout.PropertyField(serializedSettings.FindProperty("_defaultConfigFolder"), new GUIContent("Default Config Folder"));
                    EditorGUILayout.PropertyField(serializedSettings.FindProperty("_scriptOutputFolder"), new GUIContent("Script Output Folder"));
                    serializedSettings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "CSV", "Config" })
            };

            return provider;
        }
    }
}
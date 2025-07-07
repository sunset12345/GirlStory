using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JsonConfig.ConfigGenerateEditor
{
    [Serializable]
    public class JsonConfigSettings : ScriptableObject
    {
        public const string SettingsPath = "Assets/Editor/JsonConfigSettings.asset";

        [SerializeField] private string _defaultSheetFolder = "/_ConfigBase/";
        public string SheetFolder => _defaultSheetFolder;

        [SerializeField] private string _outputJsonFolder = "/Assets/_ConfigBase/Resources/";
        public string OutputJsonFolder => _outputJsonFolder;
        
        [SerializeField] private string _outputScriptFolder = "/Assets/_ConfigBase/Scripts/";
        public string OutputScriptFolder => _outputScriptFolder;
        
        [SerializeField] private int _headerStartRow = 4;
        public int HeaderStartRow => _headerStartRow;
        
        
        [SerializeField] private string _defaultIosSheetFolder = "/_ConfigBaseIos/";
        public string IosSheetFolder => _defaultIosSheetFolder;

        [SerializeField] private string _outputIosJsonFolder = "/Assets/_ConfigBase/Resources/";
        public string OutputIosJsonFolder => _outputIosJsonFolder;
        


        internal static JsonConfigSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<JsonConfigSettings>(SettingsPath);
            if (settings) return settings;
            
            settings = CreateInstance<JsonConfigSettings>();
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir ?? string.Empty);
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();

            return settings;

        }
        
        // [SettingsProvider]
        // public static SettingsProvider CreateSettingsProvider()
        // {
        //     var provider = new SettingsProvider("Tools/DataCenter/Settings", SettingsScope.Project)
        //     {
        //         // By default the last token of the path is used as display name if no label is provided.
        //         // label = "CSV Settings",
        //         // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
        //         guiHandler = (searchContext) =>
        //         {
        //             var settings = DataCenterSettings.GetOrCreateSettings();
        //             var serializedSettings = new SerializedObject(settings);
        //             EditorGUILayout.PropertyField(serializedSettings.FindProperty("_defaultSheetFolder"), new GUIContent("Default Sheet Folder"));
        //             EditorGUILayout.PropertyField(serializedSettings.FindProperty("_scriptOutputFolder"), new GUIContent("Script Output Folder"));
        //             serializedSettings.ApplyModifiedProperties();
        //         },
        //
        //         // Populate the search keywords to enable smart search filtering and label highlighting:
        //         keywords = new HashSet<string>(new[] { "CSV", "Config" })
        //     };
        //
        //     return provider;
        // }

    }
}
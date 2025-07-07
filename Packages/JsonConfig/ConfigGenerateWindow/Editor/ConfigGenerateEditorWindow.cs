using System;
using JsonConfig.ConfigGenerateEditor.DirectoryDocument;
using UnityEditor;
using UnityEngine;

namespace JsonConfig.ConfigGenerateEditor
{
    public class ConfigGenerateEditorWindow : EditorWindow
    {
        [MenuItem("Tools/JsonConfig/EditorWindow")]
        public static void Init()
        {
            var window = (ConfigGenerateEditorWindow) EditorWindow.GetWindow(typeof(ConfigGenerateEditorWindow));
            if (window == null) return;
            window.titleContent = new GUIContent("JsonConfig");
            window.Show();
        }

        private WindData _data;
        private DirectoryDocumentList _rootDirectory, _rootIosDirectory;
        
        private void OnEnable()
        {
            _data = new WindData(this);
            var settings = JsonConfigSettings.GetOrCreateSettings();
            _rootDirectory = new DirectoryDocumentList(_data, Environment.CurrentDirectory + settings.SheetFolder, "", PlatformType.Android);
            _rootIosDirectory = new DirectoryDocumentList(_data, Environment.CurrentDirectory + settings.IosSheetFolder, "", PlatformType.Ios);
        }
        
        private void OnGUI()
        {
            DrawToolBarGUI();
            DrawXlsxScrollGUI();
        }

        #region draw tool bar

        private SerializedObject _serializedSettings;
        private SerializedObject SerializedSettings =>
            _serializedSettings ?? (_serializedSettings = new SerializedObject(JsonConfigSettings.GetOrCreateSettings()));

        private bool _isShowSettings = false;
        
        private void DrawToolBarGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button(new GUIContent("Settings"), EditorStyles.toolbarButton,GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                    _isShowSettings = !_isShowSettings;
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            if (_isShowSettings)
            {
                EditorGUILayout.PropertyField(SerializedSettings.FindProperty("_headerStartRow"), new GUIContent("表头开始行:"), GUILayout.ExpandWidth(true));
                
                EditorGUILayout.LabelField("Android配置", _data.StyleSeparator, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(SerializedSettings.FindProperty("_defaultSheetFolder"), new GUIContent("Xlsx文件夹:"), GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(SerializedSettings.FindProperty("_outputJsonFolder"), new GUIContent("Json文件夹:"), GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(SerializedSettings.FindProperty("_outputScriptFolder"), new GUIContent("CS文件夹:"), GUILayout.ExpandWidth(true));
                
                EditorGUILayout.LabelField("IOS配置", _data.StyleSeparator, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(SerializedSettings.FindProperty("_defaultIosSheetFolder"), new GUIContent("Xlsx文件夹:"), GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(SerializedSettings.FindProperty("_outputIosJsonFolder"), new GUIContent("Json文件夹:"), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("",EditorStyles.toolbar,GUILayout.ExpandWidth(true));
                
                SerializedSettings.ApplyModifiedProperties();
            }
        }

        #endregion

        #region draw xlsx scroll
        
        private Vector2 _scrollPos;
        private void DrawXlsxScrollGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            {
                _rootDirectory.OnGUI();
                EditorGUILayout.LabelField("", EditorStyles.toolbar,GUILayout.ExpandWidth(true));
                _rootIosDirectory.OnGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}
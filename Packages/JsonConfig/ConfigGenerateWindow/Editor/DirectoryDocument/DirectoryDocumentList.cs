using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JsonConfig.ConfigGenerateEditor.DirectoryDocument
{
    public class DirectoryDocumentList
    {
        private readonly WindData _data;
        private readonly List<DocumentInfo> _document = new List<DocumentInfo>();
        private readonly List<DirectoryDocumentList> _directory = new List<DirectoryDocumentList>();
        private readonly string _directoryPath, _directoryAlias;

        private bool isShowGroup = true;
        private readonly bool _isChild = false;
        private readonly PlatformType _platformType;

        public DirectoryDocumentList(WindData data, string curPath, string parentPath, PlatformType type)
        {
            _platformType = type;
            _data = data;
            _directoryPath = curPath;
            _directoryAlias = curPath.Replace(Environment.CurrentDirectory, "");
            _isChild = _directoryAlias.Split('/').Length > 4;
            if (_isChild) _directoryAlias = curPath.Replace(parentPath, "");
            if (!Directory.Exists(_directoryPath)) return;
            foreach (var file in Directory.GetFiles(_directoryPath))
            {
                if (file.Contains("~") || !file.Contains(".xlsx") && !file.Contains(".xml") && !file.Contains(".csv")) continue;
                _document.Add(new DocumentInfo
                {
                    file = file,
                    alias = file.Replace(_directoryPath, "")
                });
            }

            foreach (var directory in Directory.GetDirectories(_directoryPath))
            {
                if (directory.Contains("~")) continue;
                _directory.Add(new DirectoryDocumentList(_data, directory + "/", _directoryPath, type));
            }
        }

        public void OnGUI()
        {
            if(_isChild) EditorGUI.indentLevel++;
            GUI.enabled = !ExecutePython.isRunPython;
            EditorGUILayout.BeginHorizontal();
            if (!_isChild)
            {
                isShowGroup = EditorGUILayout.BeginFoldoutHeaderGroup(isShowGroup, _directoryAlias);
                EditorGUILayout.EndFoldoutHeaderGroup();
            }else isShowGroup = EditorGUILayout.Foldout(isShowGroup, _directoryAlias, EditorStyles.foldoutHeader);
            if (GUILayout.Button(new GUIContent("ALL"), GUILayout.ExpandWidth(false), GUILayout.MaxHeight(20)))
            {
                ExportClass(_directoryPath);
            }
            EditorGUILayout.EndHorizontal();
            if(isShowGroup)
            {
                for (var i = 0; i < _directory.Count; i++)
                {
                    _directory[i].OnGUI();
                }
                for (var i = 0; i < _document.Count; i++)
                {
                    DrawDocumentOne(_document[i]);
                }
            }
            
            if(_isChild) EditorGUI.indentLevel--;
        }

        private void DrawDocumentOne(DocumentInfo info)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();

            
            EditorGUILayout.LabelField(info.alias, _data.StyleDocument,GUILayout.ExpandWidth(true), GUILayout.MinHeight(22));
            
            if (GUILayout.Button(new GUIContent(_data.ExportIcon), GUILayout.ExpandWidth(false), GUILayout.MaxHeight(22)))
            {
                ExportClass(info.file);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        private void ExportClass(string file)
        {
            var settings = JsonConfigSettings.GetOrCreateSettings();
            if(_platformType == PlatformType.Android)
                ExecutePython.ExportClass(file, settings.SheetFolder, settings.OutputJsonFolder);
            else if(_platformType == PlatformType.Ios)
                ExecutePython.ExportClass(file, settings.IosSheetFolder, settings.OutputIosJsonFolder);
        }
    }
}
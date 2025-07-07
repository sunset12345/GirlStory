using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace GSDev.AssetBundles.Editor
{
    public class AssetBundleAnalysisWindow : EditorWindow
    {
        private string _path;
        private readonly Dictionary<string, HashSet<string>> _dependency = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> Dependency => _dependency;
        private readonly Dictionary<string, HashSet<string>> _requiredRelation = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> RequiredRelation => _requiredRelation;
        private TreeViewState _dependencyTreeViewState;
        private AssetBundleDependencyTree _dependencyTree;

        private void OnGUI()
        {
            var checkButtonRect = GUILayoutUtility.GetRect(
                new GUIContent("Check Loop Dependency"),
                EditorStyles.toolbarButton,
                GUILayout.ExpandWidth(true));
            if (GUI.Button(checkButtonRect, "Check Loop Dependency"))
                LogLoopDependency();
            var titleRect = GUILayoutUtility.GetRect(
                new GUIContent("Input asset bundle path:"),
                EditorStyles.textField,
                GUILayout.ExpandWidth(true));
            GUI.Label(
                titleRect,
                "Input asset bundle path:");
            var inputRect = GUILayoutUtility.GetRect(
                new GUIContent("Input Field"),
                EditorStyles.textField,
                GUILayout.ExpandWidth(true));
            _path = GUI.TextField(inputRect, _path);
            var refreshRect = GUILayoutUtility.GetRect(
                new GUIContent("Load Dependency"),
                EditorStyles.toolbarButton,
                GUILayout.ExpandWidth(true));
            if (GUI.Button(refreshRect, "Load Dependency") && !string.IsNullOrEmpty(_path))
                LoadDependency();
            EditorGUILayout.Space();

            if (_dependencyTree != null)
            {
                float y = 80;
                var treeRect = new Rect(0, y, position.width, position.height - y);
                _dependencyTree.OnGUI(treeRect);
            }
        }

        private void LoadDependency()
        {
            var pathNames = AssetDatabase.GetAssetPathsFromAssetBundle(_path);
            if (pathNames == null)
                return;
            _dependency.Clear();
            _requiredRelation.Clear();
            foreach (var path in pathNames)
            {
                var dependentAssets = AssetDatabase.GetDependencies(path, true);
                foreach (var dependentAssetPath in dependentAssets)
                {
                    var assetBundleName = AssetDatabase.GetImplicitAssetBundleName(dependentAssetPath);
                    if (string.IsNullOrEmpty(assetBundleName) || assetBundleName == _path)
                        continue;
                    if (!_dependency.TryGetValue(assetBundleName, out var set))
                    {
                        set = new HashSet<string>();
                        _dependency.Add(assetBundleName, set);
                    }

                    set.Add(dependentAssetPath);

                    if (!_requiredRelation.TryGetValue(dependentAssetPath, out var requiredSet))
                    {
                        requiredSet = new HashSet<string>();
                        _requiredRelation.Add(dependentAssetPath, requiredSet);
                    }

                    requiredSet.Add(path);
                }
            }

            if (_dependency.Count > 0)
            {
                _dependencyTreeViewState = new TreeViewState();
                _dependencyTree = new AssetBundleDependencyTree(
                    _dependencyTreeViewState,
                    this);
                _dependencyTree.Reload();
            }
            else
                _dependencyTree = null;
        }

        private static void LogLoopDependency()
        {
            var loopDependency = AssetBundleAnalyser.CheckLoopDependency(BundleInfo.GetAllBundles());
            foreach (var loop in loopDependency)
            {
                var output = new StringBuilder("Loop asset bundle dependency: ");
                foreach (var info in loop)
                {
                    output.Append($"{info.Name} -> \n");
                }
                Debug.LogError(output);
            }
        }

        [MenuItem("Asset Bundle/Bundle Analyse")]
        private static void Init()
        {
            EditorWindow.GetWindow<AssetBundleAnalysisWindow>("Bundle Analyse");
        }
    }
}
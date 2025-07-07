using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace GSDev.AssetBundles.Editor
{
    public class AssetBundleDependencyTree : TreeView
    {
        private readonly AssetBundleAnalysisWindow _window;
        private TreeViewItem _root;

        public AssetBundleDependencyTree(TreeViewState state, AssetBundleAnalysisWindow window) : base(state)
        {
            _window = window;
        }
        
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
                return;

            var id = selectedIds[0];
            var item = FindItem(id, _root);
            if (item == null || item.depth < 1)
                return;

            var path = item.displayName;
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        protected override TreeViewItem BuildRoot()
        {
            _root = new TreeViewItem {id = 0, depth = -1, displayName = "Dependency"};
            var id = 1;
            foreach (var dependency in _window.Dependency)
            {
                var bundle = dependency.Key;
                var set = dependency.Value;
                var bundleItem = new TreeViewItem {id = id, displayName = bundle};
                ++id;
                _root.AddChild(bundleItem);
                foreach (var asset in set)
                {
                    var assetItem = new TreeViewItem {id = id, displayName = asset};
                    ++id;
                    bundleItem.AddChild(assetItem);
                    if (_window.RequiredRelation.TryGetValue(asset, out var requiredByAssets))
                    {
                        foreach (var requiredByAsset in requiredByAssets)
                        {
                            var requiredByAssetItem = new TreeViewItem {id = id, displayName = requiredByAsset};
                            ++id;
                            assetItem.AddChild(requiredByAssetItem);
                        }
                    }
                }
            }
            
            SetupDepthsFromParentsAndChildren(_root);
            return _root;
        }
    }
}


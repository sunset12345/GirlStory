using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace GSDev.AssetBundles.Editor
{
    internal  enum ViewState
    {
        Package,
        List
    }

    internal class AssetBundleUpdaterWindow : EditorWindow
    {
        List<BundleInfo> _selections = new List<BundleInfo>();
        BundleInfo _lastSelection;
        bool _packageInApp;
        ViewState _viewState = ViewState.Package;
        Vector2 _scrollPosition;
    
        private static AssetBundleUpdater _updater;
    
        // Use this for initialization
        void OnEnable()
        {
            if (_updater == null)
            {
                _updater = new AssetBundleUpdater();
                _updater.UpdateAll(false);
            }
        }

        void OnDestroy()
        {
            _updater.SaveConfig();
        }

        void OnFocus()
        {  
            _updater?.UpdateAll(true);
        }
    
        
        // Update is called once per frame
        void Update()
        {
            if (_lastSelection != GetLastSelection())
            {
                _lastSelection = GetLastSelection();
                AssetBundleInfoEditor.Show(_lastSelection);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                Rect createRect = GUILayoutUtility.GetRect(new GUIContent("Version Manager"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
                if (GUI.Button(createRect, "Version Manager", EditorStyles.toolbarDropDown))
                {
                    _viewState = ViewState.Package;
                }
                Rect listRect = GUILayoutUtility.GetRect(new GUIContent("AssetBundle List"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
                if (GUI.Button(listRect, "AssetBundle List", EditorStyles.toolbarDropDown))
                {
                    _viewState = ViewState.List;
                }
            }
            EditorGUILayout.EndHorizontal();
            if (_viewState == ViewState.List)
            {
                EditorGUILayout.BeginVertical();
                _scrollPosition = EditorGUILayout.BeginScrollView(
                    _scrollPosition,
                    false,
                    true);
                {
                    for (int i = 0; i < _updater.Bundles.Count; ++i)
                    {
                        BundleInfo bundle = _updater.Bundles[i];
                        var selected = bundle == _lastSelection ? true : false;
                        Rect itemRect =
                            EditorGUILayout.BeginHorizontal(selected
                                ? AssetUpdaterStyle.GetStyle("SelectItem")
                                : AssetUpdaterStyle.GetStyle("UnselectItem"));
                        EditorGUILayout.LabelField(bundle.Name);
                        bundle.Encrypted = EditorGUILayout.Toggle(bundle.Encrypted);
                        EditorGUILayout.EndHorizontal();
                        ProcessSelection(itemRect, bundle);
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else if (_viewState == ViewState.Package)
            {
                EditorGUILayout.BeginVertical();
                {
                    _updater.Config.VersionFileName = EditorGUILayout.TextField("Version File Name", _updater.Config.VersionFileName);
                    _updater.Config.Password = EditorGUILayout.TextField("Bundle Encrypt Key", _updater.Config.Password);
                    _updater.Config.BundleRelativePath = EditorGUILayout.TextField("Bundle Output Folder", _updater.Config.BundleRelativePath);
                    _packageInApp = EditorGUILayout.Toggle("Package In App", _packageInApp);
                    _updater.platform = (TargetPlatform)EditorGUILayout.EnumPopup("Target Platform", _updater.platform, GUILayout.MaxWidth(300));
                    EditorGUILayout.TextField("Version Number", _updater.Config.VersionNumber.ToString());
                    _updater.increaseVersionNumber = EditorGUILayout.Toggle("Increase Version Number", _updater.increaseVersionNumber);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                bool isHorizontalBlockActive = true; 
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create New Version", GUILayout.MaxWidth(150)))
                {
                    if (_updater.CheckTargetWithEditor())
                    {
                        _updater.CreateNewVersion(_packageInApp);
                    }
                    else
                    {
                        bool confirm = UnityEditor.EditorUtility.DisplayDialog(
                                           "Warning!",
                                           "Selected target platform is different from editor build settings.",
                                           "Continue",
                                           "Cancel");
                        if (confirm)
                        {
                            _updater.CreateNewVersion(_packageInApp);
                        }
                    }
                    isHorizontalBlockActive = false;
                }
                GUILayout.FlexibleSpace();
                if (isHorizontalBlockActive)
                    EditorGUILayout.EndHorizontal();
            }
            
        }

        void ProcessSelection(Rect itemRect, BundleInfo bundle)
        {
            if (IsRectClicked(itemRect))
            {
                if (Event.current.button == 0 || !_selections.Contains(bundle))
                {
                    _selections.Clear();
                    _selections.Add(bundle);
                }
            }
            
        }

        bool IsRectClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && IsMouseOn(rect);
        }

        bool IsMouseOn(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        BundleInfo GetLastSelection()
        {
            if (_selections.Count > 0)
                return _selections[0];
            else
                return null;
        }

        [MenuItem("Asset Bundle/Bundle Updater")]
        private static void Init()
        {
            EditorWindow.GetWindow<AssetBundleUpdaterWindow>("Bundle Updater");
        }

        private const string SimulateMenuItem = "Asset Bundle/Simulate AssetBundles";

        [MenuItem(SimulateMenuItem)]
        private static void ToggleSimulateMode()
        {
            AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
        }

        [MenuItem(SimulateMenuItem, true)]
        private static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(SimulateMenuItem, AssetBundleManager.SimulateAssetBundleInEditor);
            return true;
        }

        private const string HotfixMenuItem = "Asset Bundle/Enable Hotfix";
        [MenuItem(HotfixMenuItem)]
        private static void ToggleHotfixMode()
        {
            AssetBundleManager.EnableAssetBundleHotfix = !AssetBundleManager.EnableAssetBundleHotfix;
        }

        [MenuItem(HotfixMenuItem, true)]
        private static bool ToggleHotfixModeValidate()
        {
            Menu.SetChecked(HotfixMenuItem, AssetBundleManager.EnableAssetBundleHotfix);
            return true;
        }
    }
}
  




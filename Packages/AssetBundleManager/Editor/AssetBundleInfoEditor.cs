using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace GSDev.AssetBundles.Editor
{
    [CustomEditor(typeof(BundleInfoInspectorItem))]
    internal class AssetBundleInfoEditor : UnityEditor.Editor
    {
        static BundleInfoInspectorItem bundleInfoInspectorObj = null;
        public static BundleInfo bundle = null;
        public static AssetBundleInfoEditor current = null;
    
        void OnEnable()
        {
            AssetBundleInfoEditor.current = this;
        }
    
        public static void Show(BundleInfo bundle)
        {
            if (bundleInfoInspectorObj == null)
            {
                bundleInfoInspectorObj = ScriptableObject.CreateInstance<BundleInfoInspectorItem>();
                bundleInfoInspectorObj.hideFlags = HideFlags.DontSave;
                bundleInfoInspectorObj.name = "AssetBundle Detail";
            }
            Selection.activeObject = bundleInfoInspectorObj;
    
            AssetBundleInfoEditor.bundle = bundle;
            if (AssetBundleInfoEditor.current != null)
            {
                AssetBundleInfoEditor.current.Repaint();
            }
        }
    
        public override void OnInspectorGUI()
        {
            if (bundle == null) return;
    
            EditorGUILayout.BeginVertical();
            /*
            GUILayout.Label("文件大小:", GameUpdaterStyle.GetStyle("Title"));
            GUILayout.Label(Mathf.CeilToInt(bundle.size / 1024f) + " KB");
    
            GUILayout.Label("文件MD5:", GameUpdaterStyle.GetStyle("Title"));
            GUILayout.Label(bundle.md5.ToString());
             * */
    
            GUILayout.Label("Dependent Assets", AssetUpdaterStyle.GetStyle("Title"));
            foreach (var path in bundle.GetDependentAssets())
            {
                GUILayout.Label(path);
            }
    
            GUILayout.FlexibleSpace();
    
            GUILayout.Label("Dependent Bundles", AssetUpdaterStyle.GetStyle("Title"));
            //EditorGUILayout.BeginVertical();
            foreach (var path in bundle.Dependency)
            {
                GUILayout.Label(path);
            }
            EditorGUILayout.EndVertical();
        }
    }
    
    internal class BundleInfoInspectorItem : ScriptableObject
    {
    
    }
}




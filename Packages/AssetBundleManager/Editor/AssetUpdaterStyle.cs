using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace GSDev.AssetBundles.Editor
{
    internal class AssetUpdaterStyle
    {
        readonly Dictionary<string, GUIStyle> _styleDict = new Dictionary<string, GUIStyle>();
        readonly Dictionary<string, Texture2D> _iconDict = new Dictionary<string, Texture2D>();
    
        private static AssetUpdaterStyle _instance = null;
        private static AssetUpdaterStyle GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AssetUpdaterStyle();
                var styleSet = EditorHelper.LoadOrCreateScriptableObject<AssetUpdaterStyleSet>("Assets/Editor/AssetBundle/BundleUpdaterStyleSet.asset");
                foreach (GUIStyle style in styleSet.Styles)
                {
                    if (_instance._styleDict.ContainsKey(style.name))
                        Debug.LogError("Duplicated GUIStyle " + style.name);
                    else
                        _instance._styleDict.Add(style.name, style);
                }
    
                foreach (Texture2D icon in styleSet.Icons)
                {
                    if (_instance._iconDict.ContainsKey(icon.name))
                        Debug.LogError("Duplicated icon " + icon.name);
                    else
                        _instance._iconDict.Add(icon.name, icon);
                }
            }
    
            return _instance;
        }
    
        public static GUIStyle GetStyle(string name)
        {
            if (!GetInstance()._styleDict.ContainsKey(name))
                GetInstance()._styleDict.Add(name, new GUIStyle(name));
    
            return GetInstance()._styleDict[name];
        }
    
        public static Texture2D GetIcon(string name)
        {
            if (!GetInstance()._iconDict.ContainsKey(name))
                return null;
            else
                return GetInstance()._iconDict[name];
        }
    }
}



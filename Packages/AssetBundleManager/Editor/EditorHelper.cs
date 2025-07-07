using System.IO;
using UnityEngine;
using UnityEditor;

namespace GSDev.AssetBundles.Editor
{
    public static class EditorHelper
    {
        public static void CreateAsset(UnityEngine.Object asset, string path)
        {
            var folder = Path.GetDirectoryName(path);
            var root = Directory.GetParent(Application.dataPath);
            var rootFolder = Path.Combine(root.FullName, folder);
            if (!Directory.Exists(rootFolder))
                Directory.CreateDirectory(rootFolder);
            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(asset, path);
        }

        public static T LoadOrCreateScriptableObject<T>(string path) where T : ScriptableObject
        {
            var scriptableObject = AssetDatabase.LoadAssetAtPath<T>(path);
            if (scriptableObject == null)
            {
                scriptableObject = ScriptableObject.CreateInstance<T>();
                CreateAsset(scriptableObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return scriptableObject;
        }
    }
}
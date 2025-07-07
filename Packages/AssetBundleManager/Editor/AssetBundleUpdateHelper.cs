using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace GSDev.AssetBundles.Editor
{
    public static class AssetBundleUpdateHelper
    {
        private static TargetPlatform GetTargetPlatform(BuildTargetGroup buildTargetGroup)
        {
            if (buildTargetGroup == BuildTargetGroup.Android)
                return TargetPlatform.Android;
            else if (buildTargetGroup == BuildTargetGroup.iOS)
                return TargetPlatform.iOS;
            else if (buildTargetGroup == BuildTargetGroup.Standalone)
                return TargetPlatform.Standalone;
// #if UNITY_EDITOR_OSX
//                 return TargetPlatform.StandaloneOSXIntel;
// #else
//                 return TargetPlatform.StandaloneWindows;
// #endif
            else
                return TargetPlatform.Android;
        }
        
        private static TargetPlatform GetTargetPlatform()
        {
#if UNITY_ANDROID
            return TargetPlatform.Android;
#elif UNITY_IOS
            return TargetPlatform.iOS;
#elif UNITY_STANDALONE
            return TargetPlatform.Standalone;
#else
            return TargetPlatform.Android;
#endif
        }

        private static int GetDefaultVersion()
        {
            var time = (DateTime.UtcNow - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMinutes;
            return (int)time / 5;
        }
        
        private static int GetVersion()
        {
            var methods = Assembly.Load("Assembly-CSharp-Editor")
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.IsStatic)
                .Where(m => m.GetCustomAttributes(typeof(AssetBundleVersionGetter), false).Length > 0)
                .Where(m => m.ReturnType == typeof(int))
                .ToArray();
            var getter = methods.FirstOrDefault();
            if (getter == null)
                return GetDefaultVersion();

            var version = (int)getter.Invoke(null, null);
            return version;
        }
        
        [UnityEditor.MenuItem("Asset Bundle/Build/In App AB", false, 11)]
        public static void BuildInAppAssetBundles()
        {
            BuildAssetBundles(
                true, 
                GetTargetPlatform());
        }
        
        [UnityEditor.MenuItem("Asset Bundle/Build/Patch AB", false, 12)]
        public static void BuildPatchAssetBundles()
        {
            BuildAssetBundles(
                false, 
                GetTargetPlatform());
        }

        public static void BuildAssetBundles(bool inApp, BuildTargetGroup buildTargetGroup)
        {
            BuildAssetBundles(inApp, GetTargetPlatform(buildTargetGroup));
        }
        
        public static void BuildAssetBundles(bool inApp, TargetPlatform platform)
        {
            var updater = new AssetBundleUpdater();
            updater.increaseVersionNumber = false;
            updater.UpdateAll(false);
            updater.CreateNewVersion(
                inApp,
                platform,
                GetVersion());
        }
        
        [UnityEditor.MenuItem("Asset Bundle/Build/Selected AB", false, 10)]
        public static void BuildSelectedAssetBundle()
        {
            var obj = Selection.activeObject;
            if (obj == null)
                return;
            var bundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GetAssetPath(obj));
            if (string.IsNullOrEmpty(bundleName))
                return;
            var updater = new AssetBundleUpdater();
            updater.BuildSpecificAssetBundles(
                GetTargetPlatform(),
                new [] {bundleName});
        }
    }
}
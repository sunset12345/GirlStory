using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GSDev.Singleton;
using Xxtea;

namespace GSDev.AssetBundles
{
    public delegate void UpdateNeeded(uint totalSize);

    public delegate void UpdateInfoHandler(UpdateInfo info);

    public class AsyncOperationEnumerator : IEnumerator
    {
        private readonly AsyncOperation _operation;
        public AsyncOperationEnumerator(AsyncOperation operation) => _operation = operation;
        public bool MoveNext() => !_operation.isDone;

        public void Reset() { }

        public object Current => null;
    }

    public partial class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        private readonly HashSet<string> _localizedBundleSet = new HashSet<string>();

        private VersionConfig _localVersionConfig;
        private VersionConfig _remoteVersionConfig;
        private string _localVersionFileRelativePath;
        private string _remoteVersionFileURL;
        private Dictionary<string, BundleInfo> _bundleInfoDict;
        private string _password = "";

        private int _versionNumber = 0;

        public int VersionNumber => _versionNumber;

        public static string LocalizeExtension { get; set; }

        private class AssetBundleCache
        {
            internal string Name;
            internal AssetBundle AssetBundle;
            internal bool Persistent;
            internal AssetBundleCreateRequest Request;

            private int _referenceCount;

            internal bool InUse => _referenceCount > 0 || Persistent;

            internal void Retain()
            {
                if (Persistent)
                    return;
                ++_referenceCount;
                // Debug.Log($"AssetBundleRef retain {Name}, ref count {_referenceCount}");
            }

            internal void Release(int refCount = 1)
            {
                if (Persistent)
                    return;
                // Debug.Log($"AssetBundleRef release {Name}, origin ref count {_referenceCount}, final ref count {_referenceCount - refCount}");
                _referenceCount -= refCount;
                if (!InUse && AssetBundle != null)
                {
                    AssetBundle.Unload(true);
                    // Debug.Log($"AssetBundleRef unloaded {Name}");
                }

            }
        }

        private Dictionary<string, AssetBundleCache> _assetBundleCache = new Dictionary<string, AssetBundleCache>();

        public static readonly string Suffix = ".bytes";

#if UNITY_EDITOR
        const string kSimulateAssetBundles = "SimulateAssetBundles";

        public static bool SimulateAssetBundleInEditor
        {
            get => UnityEditor.EditorPrefs.GetBool(kSimulateAssetBundles, true);
            set => UnityEditor.EditorPrefs.SetBool(kSimulateAssetBundles, value);
        }

        const string kEnableHotfix = "EnableAssetBundlesHotfix";
        public static bool EnableAssetBundleHotfix
        {
            get => UnityEditor.EditorPrefs.GetBool(kEnableHotfix, true);
            set => UnityEditor.EditorPrefs.SetBool(kEnableHotfix, value);
        }
#endif

        private AssetBundleManager()
        {
        }

        #region Functions
        private string GetBundleName(string bundleName)
        {
            if (bundleName.Contains(".")) // name has been localized
                return bundleName;

            return _localizedBundleSet.Contains(bundleName) ? $"{bundleName}.{LocalizeExtension}" : bundleName;
        }

        public IEnumerable<string> GetAllBundleNames()
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                return UnityEditor.AssetDatabase.GetAllAssetBundleNames();
            }
            else
#endif
            {
                return _bundleInfoDict.Keys;
            }
        }

        private void FetchAllLocalizedAssetBundleNames()
        {
            _localizedBundleSet.Clear();
            var bundleNameList = new List<string>(GetAllBundleNames());

            foreach (var bundleName in bundleNameList)
            {
                if (bundleName.Contains("."))
                {
                    string bundleNameWithoutExtension = bundleName.Split('.')[0];
                    _localizedBundleSet.Add(bundleNameWithoutExtension);
                }
            }
        }

        public void Init(
            string localVersionFilePath,
            string password = "")
        {
            _password = password;

#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                FetchAllLocalizedAssetBundleNames();
                return;
            }
#endif

            LoadLocalVersionFile(localVersionFilePath);
        }

        private void LoadLocalVersionFile(string relativeFilePath)
        {
            _localVersionFileRelativePath = relativeFilePath;
            string updatedPath = GetUpdatedVersionFilePath();
            VersionConfig cachedVersionFile = null;
            VersionConfig inAppVersionFile = null;
            if (File.Exists(updatedPath))
            {
                cachedVersionFile = VersionConfig.ParseJson(File.ReadAllBytes(updatedPath));
            }

            TextAsset vcInResources = Resources.Load<TextAsset>(relativeFilePath);
            inAppVersionFile = VersionConfig.ParseJson(vcInResources.bytes);
            Resources.UnloadAsset(vcInResources);

            // choose the right version file
            if (cachedVersionFile != null)
            {
                if (cachedVersionFile.VersionNum < inAppVersionFile.VersionNum)
                {
                    _localVersionConfig = inAppVersionFile;

                    // delete all old updated files
                    string directory = new FileInfo(updatedPath).Directory.FullName;
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    dirInfo.Delete(true);
                }
                else
                {
                    _localVersionConfig = cachedVersionFile;
                }
            }
            else
            {
                _localVersionConfig = inAppVersionFile;
            }

            string updatedBundleRoot = Application.persistentDataPath + "/" + _localVersionConfig.BundleRelativePath;
            if (!Directory.Exists(updatedBundleRoot))
            {
                Directory.CreateDirectory(updatedBundleRoot);
            }

            _bundleInfoDict = _localVersionConfig.CreateDictionary();
            _versionNumber = _localVersionConfig.VersionNum;
            FetchAllLocalizedAssetBundleNames();
        }

        public byte[] Decrypt(byte[] data)
        {
            return XXTEA.Decrypt(data, _password);
        }

        private string GetInPackagePath(string bundleName)
        {
            return Path.Combine(
                Application.streamingAssetsPath,
                _localVersionConfig.BundleRelativePath,
                bundleName + Suffix);
        }

        private string GetUpdatedPath(BundleInfo bundleInfo)
        {
            return GetUpdatedPath(bundleInfo, _localVersionConfig);
        }

        private string GetUpdatedPath(BundleInfo bundleInfo, VersionConfig versionConfig)
        {
            return Path.Combine(
                Application.persistentDataPath,
                Application.version,
                versionConfig.BundleRelativePath,
                bundleInfo.GetUpdatedBundleName() + Suffix);
        }

        private string GetUpdatedVersionFilePath()
        {
            return Path.Combine(
                Application.persistentDataPath,
                Application.version,
                _localVersionFileRelativePath + Suffix);
        }

        private AssetBundleCache CacheAssetBundle(
            string name,
            AssetBundle assetBundle,
            bool persistent = false)
        {
            if (_assetBundleCache.TryGetValue(name, out var cached))
            {
                if (cached.AssetBundle == null && assetBundle != null)
                    cached.AssetBundle = assetBundle;
                else
                    Debug.LogError($"Cache asset bundle duplicated! name:{name}");
                return cached;
            }

            var cache = new AssetBundleCache
            {
                Name = name,
                AssetBundle = assetBundle,
                Persistent = persistent,
            };
            cache.Retain();
            _assetBundleCache.Add(name, cache);

#if ENABLE_UWA
	        uint size = 0;
	        _assetBundleCache.ForEach(kv => size += GetAssetBundleSize(kv.Key));
	        var sizeInMB = size / (1024.0f * 1024.0f);
	        Logs.Res.Debug($"Total cached asset bundle size: {sizeInMB}MB");
#endif
            return cache;
        }

        private AssetBundleCache GetCachedAssetBundle(string bundleName)
        {
            return _assetBundleCache.TryGetValue(bundleName, out var bundleCache) ?
                bundleCache :
                null;
        }

        public bool AssetBundleExists(string bundleName)
        {
            bundleName = GetBundleName(bundleName);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                return assetPaths.Length > 0;
            }
            else
#endif
                return _bundleInfoDict.ContainsKey(bundleName);
        }

        public string[] GetAssetBundleDependency(string bundleName)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                return null;
            }
#endif
            bundleName = GetBundleName(bundleName);
            return _bundleInfoDict.TryGetValue(bundleName, out var info) ?
                info.Dependency :
                null;
        }

        public void UnloadAssetBundle(string bundleName)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                return;
            }
#endif
            bundleName = GetBundleName(bundleName);
            var assetBundleCache = GetCachedAssetBundle(bundleName);
            if (assetBundleCache == null)
                return;

            assetBundleCache.Release();
            if (!assetBundleCache.InUse)
            {
                _assetBundleCache.Remove(bundleName);
                // only if the asset bundle will be removed from cache then its dependency should be released
                // because retaining its dependency only happens when the asset bundle is loaded not from cache
                if (_bundleInfoDict.TryGetValue(bundleName, out var info))
                {
                    foreach (var dependency in info.Dependency)
                    {
                        UnloadAssetBundle(dependency);
                    }
                }
            }
        }

        internal void UnloadAssetBundle(string bundleName, int refCount)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                return;
            }
#endif
            bundleName = GetBundleName(bundleName);
            var assetBundleCache = GetCachedAssetBundle(bundleName);
            if (assetBundleCache == null)
                return;

            assetBundleCache.Release(refCount);
            if (!assetBundleCache.InUse)
            {
                _assetBundleCache.Remove(bundleName);
                if (_bundleInfoDict.TryGetValue(bundleName, out var info))
                {
                    // only if the asset bundle will be removed from cache then its dependency should be released
                    // because retaining its dependency only happens when the asset bundle is loaded not from cache
                    foreach (var dependency in info.Dependency)
                    {
                        // the ref count to be release for dependency is and should be 1
                        UnloadAssetBundle(dependency);
                    }
                }
            }
        }

        public void UnloadAllAssetBundles(
            bool unloadAllLoadedObjects = false,
            bool includePersistent = false)
        {
            var removeList = new List<string>();
            foreach (var cache in _assetBundleCache)
            {
                if (!includePersistent &&
                    cache.Value.Persistent)
                    continue;

                if (cache.Value.AssetBundle != null)
                    cache.Value.AssetBundle.Unload(unloadAllLoadedObjects);

                removeList.Add(cache.Key);
            }

            foreach (var key in removeList)
            {
                _assetBundleCache.Remove(key);
            }

            UnloadUnusedAssets();
        }

        public void UnloadUnusedAssets()
        {
            StartCoroutine(UnloadUnusedAssetsAsync());
        }

        private static IEnumerator UnloadUnusedAssetsAsync()
        {
            yield return Resources.UnloadUnusedAssets();
        }

        public uint GetAssetBundleSize(string bundleName)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                return 0;
            }
            else
#endif
            {
                bundleName = GetBundleName(bundleName);
                BundleInfo info;
                if (!_bundleInfoDict.TryGetValue(bundleName, out info))
                {
                    return 0;
                }
                else
                {
                    return _bundleInfoDict[bundleName].Size;
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var abRecord in _assetBundleCache)
            {
                if (abRecord.Value.AssetBundle != null)
                    abRecord.Value.AssetBundle.Unload(true);
            }

            _assetBundleCache.Clear();
            _assetBundleCache = null;
        }

        #endregion
    }
}
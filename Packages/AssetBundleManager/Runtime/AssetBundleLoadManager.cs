using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

namespace GSDev.AssetBundles
{
    public partial class AssetBundleManager
    {
        #region Sync Loading

        public AssetBundle LoadAssetBundle(
            string bundleName,
            bool persistent = false)
        {
            bundleName = GetBundleName(bundleName);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
                return null;
#endif
            var bundleCache = GetCachedAssetBundle(bundleName);
            if (bundleCache != null)
            {
                bundleCache.Retain();
                if (persistent)
                    bundleCache.Persistent = true;
                AssetBundle cachedBundle = null;
                if (bundleCache.AssetBundle != null)
                    cachedBundle = bundleCache.AssetBundle;
                else if (bundleCache.Request != null)
                    cachedBundle = bundleCache.Request.assetBundle;
                
                Debug.Assert(
                    cachedBundle != null, 
                    $"Asset bundle loading error! Null cached bundle for name:{bundleName}");
                return cachedBundle;
            }

            if (!_bundleInfoDict.TryGetValue(bundleName, out var info))
            {
                Debug.LogWarning($"Asset bundle loading error! Bundle info not found for name:{bundleName}");
                return null;
            }

            foreach (var dependentBundle in info.Dependency)
            {
                LoadAssetBundle(dependentBundle, persistent);
            }

            string path = GetUpdatedPath(info);
            if (!File.Exists(path))
            {
                if (info.Encrypted)
                {
                    Debug.LogError("In package asset bundle cannot be loaded synchronously. Bundle name: " +
                                   bundleName);
                    return null;
                }

                path = GetInPackagePath(bundleName);
            }

            AssetBundle bundle = null;
            if (info.Encrypted)
            {
                byte[] data = File.ReadAllBytes(path);
                bundle = AssetBundle.LoadFromMemory(Decrypt(data));
            }
            else
            {
                bundle = AssetBundle.LoadFromFile(path);
            }

            CacheAssetBundle(
                bundleName,
                bundle,
                persistent);

            return bundle;
        }

        public T LoadAsset<T>(
            string bundleName,
            string assetName)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                bundleName = GetBundleName(bundleName);
                string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    bundleName,
                    assetName);
                if (assetPaths.Length == 0)
                {
                    return null;
                }
                else if (assetPaths.Length > 1)
                {
                    Debug.LogError("Duplicated asset name:" + assetName + " in bundle:" + bundleName);
                }

                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPaths[0]);
            }
            else
#endif
            {
                var bundle = LoadAssetBundle(bundleName);
                if (bundle == null)
                {
                    return null;
                }

                var asset = bundle.LoadAsset<T>(assetName);
                if (asset == null)
                {
                    Debug.LogWarning($"Asset loading failed bundle:{bundleName}, asset:{assetName}");
                }

                return asset;
            }
        }

        public UnityEngine.Object LoadAsset(string bundleName, string assetName, Type type)
        {
            bundleName = GetBundleName(bundleName);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    bundleName,
                    assetName);
                if (assetPaths.Length == 0)
                {
                    return null;
                }
                else if (assetPaths.Length > 1)
                {
                    Debug.LogError("Duplicated asset name:" + assetName + " in bundle:" + bundleName);
                }

                return UnityEditor.AssetDatabase.LoadAssetAtPath(assetPaths[0], type);
            }
            else
#endif
            {
                var bundle = LoadAssetBundle(bundleName);
                if (bundle == null)
                {
                    return null;
                }

                var asset = bundle.LoadAsset(assetName, type);
                if (asset == null)
                {
                    Debug.LogError($"Asset loading failed bundle:{bundleName}, asset:{assetName}");
                }

                return asset;
            }
        }

        public T[] LoadAllAssets<T>(string bundleName, bool persistent = false)
            where T : UnityEngine.Object
        {
            bundleName = GetBundleName(bundleName);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                if (assetPaths.Length == 0)
                {
                    Debug.LogError("There is no asset with in bundle " + bundleName);
                    return null;
                }

                return assetPaths.SelectMany(UnityEditor.AssetDatabase.LoadAllAssetsAtPath).OfType<T>().ToArray();
            }
            else
#endif
            {
                var bundle = LoadAssetBundle(bundleName, persistent);
                if (bundle == null)
                {
                    Debug.LogError("Asset bundle not exist with name " + bundleName);
                    return null;
                }

                return bundle.LoadAllAssets<T>();
            }
        }

        public T[] LoadAssetWithSubAssets<T>(string bundleName, string assetName)
            where T : UnityEngine.Object
        {
            bundleName = GetBundleName(bundleName);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    bundleName,
                    assetName);
                if (assetPaths.Length == 0)
                {
                    Debug.LogError("There is no asset with name \"" + assetName + "\" in " + bundleName);
                    return null;
                }

                UnityEngine.Object[] list = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPaths[0]);
                List<T> outputList = new List<T>();
                foreach (UnityEngine.Object obj in list)
                {
                    T t = obj as T;
                    if (t != null)
                    {
                        outputList.Add(t);
                    }
                }

                return outputList.ToArray();
            }
            else
#endif
            {
                var bundle = LoadAssetBundle(bundleName);
                if (bundle == null)
                {
                    Debug.LogError("Asset bundle not exist with name " + bundleName);
                    return null;
                }

                return bundle.LoadAssetWithSubAssets<T>(assetName);
            }
        }

        byte[] LoadFile(string path, bool fromResourcesPath = false)
        {
            byte[] data = null;
            TextAsset objInResources = null;
            if (fromResourcesPath)
            {
                objInResources = Resources.Load(path, typeof(TextAsset)) as TextAsset;
                data = objInResources.bytes;
            }
            else
            {
                data = File.ReadAllBytes(path);
            }

            if (objInResources != null)
            {
                Resources.UnloadAsset(objInResources);
            }

            return data;
        }
        
        public void LoadScene(
            string bundleName,
            string levelName,
            bool additive = false)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    bundleName,
                    levelName);
                if (levelPaths.Length == 0)
                {
                    Debug.LogError("There is no scene with name \"" +
                                   levelName +
                                   "\" in " +
                                   bundleName);
                    return;
                }

                if (additive)
                    UnityEditor.EditorApplication.LoadLevelAdditiveInPlayMode(levelPaths[0]);
                else
                    UnityEditor.EditorApplication.LoadLevelInPlayMode(levelPaths[0]);
            }
            else
#endif
            {
                LoadAssetBundle(bundleName);
                var loadMode = additive ?
                    LoadSceneMode.Additive :
                    LoadSceneMode.Single;
                SceneManager.LoadScene(
                    levelName,
                    loadMode);
            }
        }

        #endregion


        #region Async Loading

        public LoadAssetOperation<T> LoadAssetAsync<T>(
            string bundleName,
            string assetName)
            where T : UnityEngine.Object
        {
            var operation = new LoadAssetOperation<T>(
                bundleName,
                assetName);

            StartCoroutine(LoadAssetAsync<T>(operation));
            return operation;
        }

        public IEnumerator LoadAssetAsync<T>(LoadAssetOperation<T> operation)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                var bundleName = GetBundleName(operation.bundleName);
                var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    bundleName,
                    operation.assetName);
                if (assetPaths.Length == 0)
                {
                    Debug.LogError($"There is no asset with name {operation.assetName} in {operation.bundleName}");
                    yield break;
                }

                var target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPaths[0]);
                operation.asset = target;
                yield break;
            }
#endif

            var bundleLoadOperation = new LoadAssetBundleOperation(
                operation.bundleName,
                operation.persistent);
            yield return LoadAssetBundleAsync(bundleLoadOperation);
            operation.bundle = bundleLoadOperation.bundle;

            var request = operation.bundle.LoadAssetAsync<T>(operation.assetName);
            yield return new AsyncOperationEnumerator(request);
            if (request.asset == null)
            {
                Debug.LogError($"Asset loading failed bundle:{operation.bundleName}, asset:{operation.assetName}");
            }

            operation.asset = request.asset as T;
        }

        public LoadMultiAssetsOperation<T> LoadAssetWithSubAssetsAsync<T>(
            string bundleName,
            string assetName)
            where T : UnityEngine.Object
        {
            var operation = new LoadMultiAssetsOperation<T>(
                bundleName,
                assetName);

            StartCoroutine(LoadAssetWithSubAssetsAsync<T>(operation));
            return operation;
        }

        public IEnumerator LoadAssetWithSubAssetsAsync<T>(LoadMultiAssetsOperation<T> operation)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                var bundleName = GetBundleName(operation.bundleName);
                string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    bundleName,
                    operation.assetName);
                if (assetPaths.Length == 0)
                {
                    Debug.LogError($"There is no asset with name {operation.assetName} in {operation.bundleName}");
                    yield break;
                }

                UnityEngine.Object[] list = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPaths[0]);

                operation.assets = list.Where(asset => asset is T).ToArray();
                yield break;
            }
#endif

            var bundleLoadOperation = new LoadAssetBundleOperation(
                operation.bundleName,
                operation.persistent);
            yield return LoadAssetBundleAsync(bundleLoadOperation);
            operation.bundle = bundleLoadOperation.bundle;

            var request = operation.bundle.LoadAssetWithSubAssetsAsync<T>(operation.assetName);
            yield return new AsyncOperationEnumerator(request);
            if (request.allAssets?.Length <= 0)
            {
                Debug.LogError($"Asset loading failed bundle:{operation.bundleName}, asset:{operation.assetName}");
            }

            operation.assets = request.allAssets;
        }

        public LoadMultiAssetsOperation<T> LoadAllAssetsAsync<T>(string bundleName)
            where T : UnityEngine.Object
        {
            var operation = new LoadMultiAssetsOperation<T>(
                bundleName,
                "");

            StartCoroutine(LoadAllAssetsAsync<T>(operation));
            return operation;
        }

        public IEnumerator LoadAllAssetsAsync<T>(LoadMultiAssetsOperation<T> operation)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                var bundleName = GetBundleName(operation.bundleName);
                var paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                var output = paths
                    .SelectMany(UnityEditor.AssetDatabase.LoadAllAssetsAtPath)
                    .Where(asset => asset is T);
                operation.assets = output.ToArray();
                yield break;
            }
#endif

            var bundleLoadOperation = new LoadAssetBundleOperation(
                operation.bundleName,
                operation.persistent);
            yield return LoadAssetBundleAsync(bundleLoadOperation);
            operation.bundle = bundleLoadOperation.bundle;

            var request = operation.bundle.LoadAllAssetsAsync<T>();
            yield return new AsyncOperationEnumerator(request);
            if (request.allAssets?.Length <= 0)
            {
                yield break;
            }

            operation.assets = request.allAssets;
        }

        public LoadAssetBundleOperation LoadAssetBundleAsync(
            string bundleName,
            bool persistent = false)
        {
            var operation = new LoadAssetBundleOperation(
                bundleName,
                persistent);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                operation.ForceDone();
            }
            else
#endif
            {
                StartCoroutine(LoadAssetBundleAsync(operation));
            }

            return operation;
        }

        public IEnumerator LoadAssetBundleAsync(LoadAssetBundleOperation operation)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                operation.ForceDone();
                yield break;
            }
#endif
            var bundleName = GetBundleName(operation.bundleName);
            // find cache
            var bundleCache = GetCachedAssetBundle(bundleName);
            if (bundleCache != null)
            {
                if (operation.persistent)
                    bundleCache.Persistent = true;
                bundleCache.Retain();
                if (bundleCache.AssetBundle is null)
                    yield return new WaitUntil(() => bundleCache.AssetBundle != null);
                operation.bundle = bundleCache.AssetBundle;
                yield break;
            }

            if (!_bundleInfoDict.TryGetValue(bundleName, out var info))
            {
                Debug.LogError(
                    $"Asset bundle loading asynchronous error! Bundle info not found by name: {bundleName}");
                yield break;
            }

            // create request first
            string bundlePath = GetUpdatedPath(info);
            bool inPackage = false;
            if (!File.Exists(bundlePath))
            {
                inPackage = true;
                bundlePath = GetInPackagePath(bundleName);
            }

            AssetBundleCreateRequest request;
            if (info.Encrypted)
            {
                if (inPackage)
                {
                    string path;
#if UNITY_EDITOR
                    path = "file://" + bundlePath;
#elif UNITY_ANDROID
                        path = bundlePath;
#elif UNITY_IOS
                        path = "file://" + bundlePath;
#else
                        //Desktop (Mac OS or Windows)
                        path = "file://" + bundlePath;
#endif
                    UnityWebRequest fileDownloader = UnityWebRequest.Get(path);
                    fileDownloader.downloadHandler = new DownloadHandlerBuffer();
                    yield return fileDownloader.SendWebRequest();
                    request = AssetBundle.LoadFromMemoryAsync(Decrypt(fileDownloader.downloadHandler.data));
                    fileDownloader.downloadHandler.Dispose();
                    fileDownloader.Dispose();
                }
                else
                {
                    byte[] data = File.ReadAllBytes(bundlePath);
                    request = AssetBundle.LoadFromMemoryAsync(Decrypt(data));
                }
            }
            else
            {
                request = AssetBundle.LoadFromFileAsync(bundlePath);
            }
            
            // cache bundle before loading
            var cachedBundle = CacheAssetBundle(
                bundleName,
                null,
                operation.persistent);
            // store request in cache for sync loading
            cachedBundle.Request = request;
            
            // load dependency
            foreach (var dependentBundle in info.Dependency)
            {
                var dependentBundleName = GetBundleName(dependentBundle);
                var cachedDependentBundle = GetCachedAssetBundle(dependentBundleName);
                if (cachedDependentBundle != null)
                {
                    cachedDependentBundle.Retain();
                    continue;
                }
                var dependencyLoadOperation = new LoadAssetBundleOperation(
                    dependentBundle,
                    operation.persistent);
                yield return LoadAssetBundleAsync(dependencyLoadOperation);
            }

            yield return request;
            operation.bundle = request.assetBundle;

            // save bundle in cache
            if (cachedBundle.InUse)
            {
                cachedBundle.AssetBundle = operation.bundle;
                cachedBundle.Request = null;
            }
            else
                operation.bundle.Unload(true);    // at this moment, the cached bundle is unloaded
        }

        public LoadSceneOperation LoadSceneAsync(
            string bundleName,
            string levelName,
            bool additive = false)
        {
            bundleName = GetBundleName(bundleName);
            LoadSceneOperation operation = new LoadSceneOperation(
                bundleName,
                levelName,
                additive);
            StartCoroutine(LoadSceneAsync(operation));
            return operation;
        }

        private IEnumerator LoadSceneAsync(LoadSceneOperation operation)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    operation.bundleName,
                    operation.levelName);
                if (levelPaths.Length == 0)
                {
                    Debug.LogError("There is no scene with name \"" +
                                   operation.levelName +
                                   "\" in " +
                                   operation.bundleName);
                    yield break;
                }

                if (operation.additive)
                    operation.sceneLoadOperation =
                        UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(levelPaths[0]);
                else
                    operation.sceneLoadOperation =
                        UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);
            }
            else
#endif
            {
                var bundleLoadOperation = new LoadAssetBundleOperation(
                    operation.bundleName,
                    false);
                yield return LoadAssetBundleAsync(bundleLoadOperation);
                if (operation.additive)
                    operation.sceneLoadOperation = SceneManager.LoadSceneAsync(
                        operation.levelName,
                        LoadSceneMode.Additive);
                else
                    operation.sceneLoadOperation = SceneManager.LoadSceneAsync(operation.levelName);
            }

            yield return operation.sceneLoadOperation;
        }
    }

    #endregion
}
using System.Collections.Generic;

namespace GSDev.AssetBundles
{
    public class AssetBundleRecorder
    {
        private readonly Dictionary<string, int> _loadedAssetBundles = new Dictionary<string, int>(16);
        
        public void RecordAssetBundle(string assetBundleName)
        {
            if (_loadedAssetBundles.TryGetValue(assetBundleName, out var count))
                _loadedAssetBundles[assetBundleName] = count + 1;
            else
                _loadedAssetBundles[assetBundleName] = 1;
            
            // Debug.Log($"Bundle Recorder ADD {assetBundleName} count: {_loadedAssetBundles[assetBundleName]}");
        }

        public void RemoveAssetBundle(string assetBundleName)
        {
            if (_loadedAssetBundles.TryGetValue(assetBundleName, out var count))
            {
                _loadedAssetBundles[assetBundleName] = count - 1;
                // Debug.Log($"Bundle Recorder REMOVE {assetBundleName} count: {_loadedAssetBundles[assetBundleName]}");
            }
        }
        
        public void UnloadAll()
        {
            foreach (var entity in _loadedAssetBundles)
            {
                if (entity.Value <= 0)
                    continue;
                AssetBundleManager.Instance.UnloadAssetBundle(entity.Key, entity.Value);
            }
            _loadedAssetBundles.Clear();
        }
    }
}
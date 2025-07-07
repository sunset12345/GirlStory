using UnityEngine;

namespace GSDev.AssetBundles
{
    public class AssetBundleAutoReleaser : MonoBehaviour
    {
        private readonly AssetBundleRecorder _assetBundleRecorder = new AssetBundleRecorder();
    
        public AssetBundleRecorder Recorder => _assetBundleRecorder;

        private void OnDestroy()
        {
            _assetBundleRecorder.UnloadAll();
        }

        public static void AutoReleaseAssetBundle(GameObject go, string assetBundleName)
        {
            var releaser = go.GetComponent<AssetBundleAutoReleaser>();
            if (releaser is null || releaser == null)
                releaser = go.AddComponent<AssetBundleAutoReleaser>();
            releaser.Recorder.RecordAssetBundle(assetBundleName);
        }
    }
}


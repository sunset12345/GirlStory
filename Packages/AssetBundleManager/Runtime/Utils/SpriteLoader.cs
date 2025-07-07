using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GSDev.AssetBundles
{
    [RequireComponent(typeof(Image))]
    public class SpriteLoader : MonoBehaviour
    {
        [SerializeField]
        private string _bundleName = null;
        [SerializeField]
        private string _assetName = null;

        private IEnumerator Start ()
        {
            LoadAssetOperation<Sprite> loader = AssetBundleManager.Instance.LoadAssetAsync<Sprite>(
                _bundleName,
                _assetName);
            yield return loader;

            GetComponent<Image>().sprite = loader.asset;
        }

        private void OnDestroy()
        {
            AssetBundleManager.Instance.UnloadAssetBundle(_bundleName);
        }
    }
}
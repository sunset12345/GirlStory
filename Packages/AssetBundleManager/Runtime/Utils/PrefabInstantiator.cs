using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GSDev.AssetBundles
{
    public class PrefabInstantiator : MonoBehaviour
    {
        [SerializeField]
        private string _bundleName = null;
        [SerializeField]
        private string _assetName = null;
        [SerializeField]
        private Transform _root = null;
        [SerializeField]
        private UnityEvent _onFinish = null;
	
        private IEnumerator Start ()
        {
            LoadAssetOperation<GameObject> loader = AssetBundleManager.Instance.LoadAssetAsync<GameObject>(
                _bundleName,
                _assetName);
            yield return loader;
            
            if (_root != null)
            {
                Instantiate(loader.asset, _root);
            }
            else
            {
                Instantiate(loader.asset, transform.parent);
            }

            if (_onFinish != null)
                _onFinish.Invoke();
        }

        private void OnDestroy()
        {
            AssetBundleManager.Instance.UnloadAssetBundle(_bundleName);
        }
    }
}
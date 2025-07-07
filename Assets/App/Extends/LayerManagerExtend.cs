using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GSDev.AssetBundles;
using GSDev.UI.Layer;
using UnityEngine;
using UnityEngine.UI;
namespace App.Extends
{
    // This class is used to extend the LayerManager functionality.
    // It can be used to add custom behaviors or properties to the LayerManager.
    // Currently, it does not contain any specific implementation.

    public static class LayerManagerExtend
    {
        public static async UniTask<LayerContent> LoadContentAsync(this LayerManager self, int layer, string path)
        {
            path.ResolveAssetPath(out var bundleName, out var assetName);
            var loader = AssetBundleManager.Instance.LoadAssetAsync<GameObject>(bundleName, assetName);
            await loader;

            var go = UnityEngine.Object.Instantiate(loader.asset);
            go.name = loader.asset.name;
            var content = go.GetComponent<LayerContent>();
            Debug.Assert(content);

            return LayerManager.Instance.LoadContent(
                layer,
                content);
        }

        public static async void LoadContentAsync(this LayerManager self, int layer, string path,
            Action<LayerContent> callback)
        {
            var content = await self.LoadContentAsync(layer, path);
            callback?.Invoke(content);
        }


        private static readonly int UISortingLayer = SortingLayer.NameToID("UI");
        private static readonly LayerMask UILayerMask = 1 << LayerMask.NameToLayer("UI");
        public static void CreateUILayer(this LayerManager self, int layer, LayerControllerBase controller)
        {
            var content = self.CreateLayer(layer, controller);
            self.StartCoroutine(SetLayerSoringLayer(content.Root));
        }

        private static IEnumerator SetLayerSoringLayer(Component root)
        {
            yield return new WaitForEndOfFrame();
            var canvas = root.GetComponent<Canvas>();
            canvas.sortingLayerID = UISortingLayer;
            var graphicRaycaster = root.GetComponent<GraphicRaycaster>();
            graphicRaycaster.blockingMask = UILayerMask;
        }
    }
}

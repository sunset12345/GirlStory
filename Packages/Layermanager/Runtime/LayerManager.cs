using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GSDev.Singleton;

namespace GSDev.UI.Layer
{
    public class LayerManager : MonoSingleton<LayerManager>
    {
        public class ContentLayer
        {
            public Transform Root { get; internal set; }
            public LayerControllerBase Controller { get; internal set; }

            public int Layer => Controller.Layer;
        }

        private readonly Dictionary<int, ContentLayer> _contentLayers = new Dictionary<int, ContentLayer>();
        public Canvas RootCanvas { get; private set; }

        public delegate GameObject LayerLoader(string path, ContentLayer layer);
        private readonly List<LayerLoader> _loaders = new List<LayerLoader>();

        private void Start()
        {
            transform.SetParent(null);
            gameObject.name = "LayerManager";
            RootCanvas = gameObject.GetComponent<Canvas>();
        }

        public void AddLayerLoader(LayerLoader loader)
        {
            _loaders.Add(loader);
        }

        private GameObject LoadContentByPath(string path, ContentLayer layer)
        {
            foreach (var loader in _loaders)
            {
                var content = loader(path, layer);
                if (content != null)
                    return content;
            }

            return null;
        }

        public LayerContent LoadContent(
            int layer,
            string path)
        {
            var contentLayer = GetLayer(layer);
            var go = LoadContentByPath(path, contentLayer);
            if (go == null)
                return null;

            var content = go.GetComponent<LayerContent>();
            Debug.Assert(content != null);

            return LoadContent(
                layer, 
                content);
        }

        public LayerContent LoadContent(
            int layer, 
            LayerContent content)
        {
            var contentLayer = GetLayer(layer);
            if (content.transform.parent != contentLayer.Root)
                content.transform.SetParent(contentLayer.Root, false);

            content.Layer = layer;
            contentLayer.Controller?.OnNewContent(content);
            return content;
        }

        public ContentLayer CreateLayer(
            int layer,
            LayerControllerBase controller = null)
        {
            return CreateLayer(
                layer,
                string.Empty,
                controller);
        }

        public ContentLayer CreateLayer(
            int layer,
            string name,
            LayerControllerBase controller = null)
        {
            if (_contentLayers.TryGetValue(layer, out var contentLayer))
                return contentLayer;

            if (string.IsNullOrWhiteSpace(name))
                name = "*";
            var go = new GameObject($"Layer:{layer}({name})",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(UnityEngine.UI.GraphicRaycaster))
            {
                layer = LayerMask.NameToLayer("UI")
            };
            contentLayer = new ContentLayer();
            if (controller == null)
                controller = new DefaultLayerController();
            
            controller.Manager = this;
            controller.Layer = layer;
            contentLayer.Controller = controller;
            var rectTransform = go.transform as RectTransform;
            contentLayer.Root = rectTransform;
            
            // make layer full screen
            rectTransform.SetParent(transform, false);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            
            _contentLayers.Add(layer, contentLayer);
            var canvas = go.GetComponent<Canvas>();
            // canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            StartCoroutine(SetLayerSoringOrder(canvas, layer));
            return contentLayer;
        }

        private static IEnumerator SetLayerSoringOrder(Canvas canvas, int order)
        {
            yield return new WaitForEndOfFrame();
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
        }

        private ContentLayer GetLayer(int layerIndex)
        {
            if (!_contentLayers.TryGetValue(layerIndex, out var layer))
                layer = CreateLayer(layerIndex);
            return layer;
        }

        public void SetLayerVisible(int layerIndex, bool on)
        {
            if (!_contentLayers.TryGetValue(layerIndex, out var contentLayer))
                return;
            contentLayer.Root.gameObject.SetActive(on);
        }

        public bool IsLayerVisible(int layerIndex)
        {
            if (!_contentLayers.TryGetValue(layerIndex, out var contentLayer))
                return false;
            return contentLayer.Root.gameObject.activeSelf && contentLayer.Controller.ContentCount > 0;
        }

        public LayerControllerBase GetLayerController(int layerIndex)
        {
            if (!_contentLayers.TryGetValue(layerIndex, out var contentLayer))
                return null;
            return contentLayer.Controller;
        }

        public event Action<LayerControllerBase, LayerContent> ContentAddedEvent; 
        internal void OnContentAdded(LayerControllerBase controller, LayerContent content)
        {
            ContentAddedEvent?.Invoke(controller, content);
        }

        public event Action<LayerControllerBase> ContentClosedEvent;
        internal void OnContentClosed(LayerControllerBase controller)
        {
            ContentClosedEvent?.Invoke(controller);
        }
    }
}

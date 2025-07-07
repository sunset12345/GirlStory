using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSDev.UI.Layer
{
    public class StackLayerController : LayerControllerBase
    {
        private readonly List<LayerContent> _contentStack = new List<LayerContent>(4);
        public override int ContentCount => _contentStack.Count;
        public override LayerContent CurrentContent => _contentStack.Count == 0 ? null : CurrentContentUnsafe;

        private int CurrentContentIndex => _contentStack.Count - 1;
        private LayerContent CurrentContentUnsafe => _contentStack[CurrentContentIndex];

        public override void OnNewContent(LayerContent content)
        {
            Debug.Assert(content != null);
            if (_contentStack.Count > 0)
                CurrentContentUnsafe.OnHide();
            _contentStack.Add(content);
            content.OnShown();
            Manager.OnContentAdded(this, content);
        }

        public override void CloseCurrent()
        {
            if (_contentStack.Count == 0)
                return;

            var content = CurrentContentUnsafe;
            _contentStack.RemoveAt(CurrentContentIndex);
            content.OnClose();
            Manager.OnContentClosed(this);
            if (ContentCount > 0)
                CurrentContent.OnShown();
        }

        public override void CloseContent(LayerContent content)
        {
            var index = _contentStack.IndexOf(content);
            if (index < 0)
                return;
            
            if (index == CurrentContentIndex)
                CloseCurrent();
            else
            {
                _contentStack.RemoveAt(index);
                content.OnClose();
                Manager.OnContentClosed(this);
            }
        }

        public override void CloseAll()
        {
            if (_contentStack.Count == 0)
                return;
            foreach (var content in _contentStack)
            {
                content.OnClose();
            }
            _contentStack.Clear();
            Manager.OnContentClosed(this);
        }
    }
}

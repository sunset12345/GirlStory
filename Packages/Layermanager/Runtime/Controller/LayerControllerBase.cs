using System;

namespace GSDev.UI.Layer
{
    public abstract class LayerControllerBase
    {
        internal LayerManager Manager;

        public int Layer
        {
            get;
            internal set;
        }
        
        public abstract void OnNewContent(LayerContent content);
        public abstract void CloseCurrent();
        public abstract void CloseContent(LayerContent content);
        public abstract void CloseAll();
        public abstract int ContentCount { get; }
        public abstract LayerContent CurrentContent { get; }
    }
}
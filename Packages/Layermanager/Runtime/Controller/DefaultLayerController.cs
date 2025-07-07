namespace GSDev.UI.Layer
{
    public class DefaultLayerController : LayerControllerBase
    {
        private LayerContent _current;
        
        public override void OnNewContent(LayerContent content)
        {
            if (_current != null)
                CloseCurrent();

            _current = content;
            _current.OnShown();
            Manager.OnContentAdded(this, content);
        }

        public override void CloseCurrent()
        {
            if (_current == null)
                return;
            _current.OnClose();
            _current = null;
            Manager.OnContentClosed(this);
        }

        public override void CloseContent(LayerContent content)
        {
            if (_current == content)
                CloseCurrent();
        }

        public override void CloseAll()
        {
            CloseCurrent();
        }

        public override int ContentCount => _current == null ? 0 : 1;
        public override LayerContent CurrentContent => _current;
    }
}
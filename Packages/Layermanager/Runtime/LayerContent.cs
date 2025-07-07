using UnityEngine;

namespace GSDev.UI.Layer
{
    public abstract class LayerContent : MonoBehaviour
    {
        public int Layer { get; set; }

        public void Close()
        {
            var controller = LayerManager.Instance.GetLayerController(Layer);
            if (controller != null)
                controller.CloseContent(this);
            else
                OnClose();
        }
    
        public virtual void OnShown() { gameObject.SetActive(true); }
        public virtual void OnHide() { gameObject.SetActive(false); }
        public virtual void OnClose() { Destroy(gameObject); }
    }
}

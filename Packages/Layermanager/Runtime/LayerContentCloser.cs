using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GSDev.UI.Layer
{
    [RequireComponent(typeof(Button))]
    public class LayerContentCloser : MonoBehaviour
    {
        [Serializable]
        public class CloseAction : UnityEvent {}
        [SerializeField] private CloseAction _onClose;
        private Button _button;
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        public void OnClick()
        {
            var layerContent = GetComponentInParent<LayerContent>();
            Debug.Assert(layerContent != null);
        
            _onClose?.Invoke();
            layerContent.Close();
        }
    }
}

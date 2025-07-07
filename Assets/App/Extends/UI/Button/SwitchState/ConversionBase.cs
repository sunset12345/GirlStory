using UnityEngine;

namespace GSDev.UI
{
    [ExecuteAlways]
    public abstract class ConversionBase : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private bool _inversion;

        private void Awake()
        {
            if (!_button) _button = GetComponent<Button>();
            if (_button) _button.AddInteractableChangeEvent(Switch);
            OnAwake();
        }
        
        protected virtual void OnAwake() {}

        public void Switch(bool conversion)
        {
            if (_inversion) conversion = !conversion;
            OnSwitch(conversion);
        }

        protected abstract void OnSwitch(bool conversion);
    }
}
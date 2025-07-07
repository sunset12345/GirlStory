using UnityEngine;

namespace GSDev.UI
{
    public class ConversionPosition : ConversionBase
    {
        [SerializeField] private Transform _transform;
        public Vector3 position;
        private Vector3 _oldPosition;

        protected override void OnAwake()
        {
            if (!_transform) _transform = transform;
            _oldPosition = _transform.localPosition;
        }

        protected override void OnSwitch(bool conversion)
        {
            _transform.localPosition = conversion ? _oldPosition : position;
        }
    }
}

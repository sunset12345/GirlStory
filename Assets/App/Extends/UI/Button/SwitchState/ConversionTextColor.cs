using UnityEngine;
using UnityEngine.UI;

namespace GSDev.UI
{
    public class ConversionTextColor : ConversionBase
    {
        [SerializeField] private Text _text;
        public Color color = Color.gray;
        private Color _oldColor;

        protected override void OnAwake()
        {
            if (!_text) _text = GetComponent<Text>();
            _oldColor = _text.color;
        }

        protected override void OnSwitch(bool conversion)
        {
            _text.color = conversion ? _oldColor : color;
        }
    }
}

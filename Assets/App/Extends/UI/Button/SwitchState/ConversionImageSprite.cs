using UnityEngine;
using UnityEngine.UI;

namespace GSDev.UI
{
    public class ConversionImageSprite : ConversionBase
    {
        [SerializeField] private Image _image;
        public Sprite sprite;
        
        public Sprite OldSprite { get; set; }
        
        protected override void OnAwake()
        {
            if (!_image) _image = GetComponent<Image>();
            OldSprite = _image.sprite;
        }

        protected override void OnSwitch(bool conversion)
        {
            _image.sprite = conversion ? OldSprite : sprite;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace GSDev.UI
{
    public class ConversionImageGray : ConversionBase
    {
        [SerializeField] private Image _image;

        private static Material _grayscaleMaterial;
        private Material _originMaterial;

        protected override void OnAwake()
        {
            if (!_image) 
                _image = GetComponent<Image>();
        }

        protected override void OnSwitch(bool conversion)
        {
            if (_grayscaleMaterial == null)
            {
                _grayscaleMaterial = new Material(Shader.Find("UI/SpriteGrayscale"));
                _grayscaleMaterial.SetFloat("_EffectAmount", 1f);
            }

            if (conversion)
            {
                if (_image.material == _grayscaleMaterial)
                    _image.material = _originMaterial != null ? _originMaterial : null;
            }
            else
            {
                if (_image.material != _grayscaleMaterial)
                {
                    _originMaterial = _image.material;
                    _image.material = _grayscaleMaterial;
                }
            }
            
        }
    }
}

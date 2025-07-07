using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace GSDev.UI
{
    public class ConversionGraphicGray : ConversionBase
    {
        [SerializeField] private Graphic _graphic;

        private static Material _grayscaleMaterial;
        private Material _originMaterial;

        protected override void OnAwake()
        {
            if (!_graphic) 
                _graphic = GetComponent<Graphic>();
        }

        protected override void OnSwitch(bool conversion)
        {
            if (!_grayscaleMaterial)
            {
                _grayscaleMaterial = new Material(Shader.Find("UI/SpriteGrayscale"));
                _grayscaleMaterial.SetFloat("_EffectAmount", 1f);
            }

            if (conversion)
            {
                if (_graphic.material == _grayscaleMaterial)
                    _graphic.material = _originMaterial ? _originMaterial : null;
            }
            else if (_graphic.material != _grayscaleMaterial)
            {
                _originMaterial = _graphic.material;
                _graphic.material = _grayscaleMaterial;
            }
        }
    }
}

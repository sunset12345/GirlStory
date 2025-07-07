using DG.Tweening;
using UnityEngine;

namespace GSDev.UI.TweenAni
{
    public class ButtonClickBlendableScale : ButtonClickAnimationBase
    {
        [SerializeField] private float _scale = -0.1f;
        [SerializeField] private float _delay = 0.1f;

        protected override void ClickDown()
        {
            transform.DOBlendableScaleBy(Vector3.one * _scale, _delay)
                .SetId("ButtonClickBlendableScale_Down");
        }

        protected override void ClickUp()
        {
            transform.DOBlendableScaleBy(Vector3.one * -_scale, _delay)
                .SetId("ButtonClickBlendableScale_Up");
        }
    }
}
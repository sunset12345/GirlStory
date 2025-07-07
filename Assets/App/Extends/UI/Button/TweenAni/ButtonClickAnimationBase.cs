using System;
using DG.Tweening;
using UnityEngine;
using GSDev.UI;

namespace GSDev.UI.TweenAni
{
    [ExecuteAlways]
    public abstract class ButtonClickAnimationBase : MonoBehaviour
    {
        [SerializeField] protected Button _button;
        private bool _tweenPlay = false;

        protected abstract void ClickDown();
        protected abstract void ClickUp();

        private void OnDestroy()
        {
            transform.DOKill();
        }

        private void OnDisable()
        {
            if (!_tweenPlay)
                return;
            _tweenPlay = false;
            ClickUp();
        }

        private void Awake()
        {
            if (!_button) 
                _button = GetComponent<Button>();
            _button.AddPointerDown(ClickDownBase);
            _button.AddPointerUp(ClickUpBase);
        }

        private void ClickDownBase()
        {
            if (!_button.interactable && !_button.HasRegisterUnClick) return;
            _tweenPlay = true;
            ClickDown();
        }

        private void ClickUpBase()
        {
            if (!_tweenPlay || (!_button.interactable && !_button.HasRegisterUnClick)) return;
            _tweenPlay = false;
            ClickUp();
        }
    }
}
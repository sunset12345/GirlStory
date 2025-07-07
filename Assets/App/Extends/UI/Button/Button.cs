using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace GSDev.UI
{
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Button : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ICancelHandler
    {
        [SerializeField] private bool _interactable = true;
        public bool interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                _interactableChangeEvent?.Invoke(value);
            }
        }

        [SerializeField] private Transform _targetTransform;
        public Transform TargetTransform => _targetTransform;
        
        private event Action<bool> _interactableChangeEvent;
        
        
        [Serializable]
        public class ButtonPointerEvent : UnityEvent { }
        
        [FormerlySerializedAs("onClick")]
        [SerializeField] 
        private ButtonPointerEvent _onClick = null;
        
        private ButtonPointerEvent _onUnClick = null;
        private ButtonPointerEvent _onPointerUp = null;
        private ButtonPointerEvent _onPointerDown = null;
        
        public bool HasRegisterUnClick => _onUnClick != null;

        private void OnDestroy()
        {
            _targetTransform.DOKill();
        }

        private void Awake()
        {
            if (_targetTransform == null)
                _targetTransform = transform;
        }

        #region 按键事件

        public void AddInteractableChangeEvent(Action<bool> interactableChange)
        {
            _interactableChangeEvent += interactableChange;
        }
        public void RemoveInteractableChangeEvent(Action<bool> interactableChange)
        {
            _interactableChangeEvent -= interactableChange;
        }
        
        public void AddClick(UnityAction call) => _onClick.AddListener(call);
        public void AddUnClick(UnityAction call)
        {
            _onUnClick ??= new ButtonPointerEvent();
            _onUnClick.AddListener(call);
        }
        public void AddPointerUp(UnityAction call)
        {
            _onPointerUp ??= new ButtonPointerEvent();
            _onPointerUp.AddListener(call);
        }
        public void AddPointerDown(UnityAction call)
        {
            _onPointerDown ??= new ButtonPointerEvent();
            _onPointerDown.AddListener(call);
        }

        public void RemoveClick(UnityAction call) => _onClick.RemoveListener(call);
        public void RemoveUnClick(UnityAction call) => _onUnClick?.RemoveListener(call);
        public void RemovePointerUp(UnityAction call) => _onPointerUp?.RemoveListener(call);
        public void RemovePointerDown(UnityAction call) => _onPointerDown?.RemoveListener(call);

        #endregion
        
        #region 点击事件

        public bool ClickSelf()
        {
            if (_onClick == null || !_interactable) return false;
            _onClick.Invoke();
            return true;
        }

        public void UnClickSelf()
        {
            if (_onUnClick == null || _interactable) return;
            _onUnClick.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!ClickSelf()) 
                UnClickSelf();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_interactable) 
                _onPointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_interactable) 
                _onPointerUp?.Invoke();
        }

        public void OnCancel(BaseEventData eventData)
        {
            OnPointerUp((PointerEventData) eventData);
        }

        #endregion
    }
}
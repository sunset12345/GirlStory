using GSDev.UI.Layer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GSDev.UI.Layer
{
    public class PopupLayerContent : LayerContent
    {
        [SerializeField] private Image _imageMask;
        [SerializeField] private Transform _rootTrans;

        // private static readonly Subject<GameObject> _onOpenSubject = new();
        // public static IObservable<GameObject> OnOpenObservable => _onOpenSubject;
        // private static readonly Subject<GameObject> _onCloseSubject = new();
        // public static IObservable<GameObject> OnCloseObservable => _onCloseSubject;

        public Image ImageMask => _imageMask;
        public Transform RootTrans => _rootTrans;
        public override void OnShown()
        {
            // AudioManager.PlaySound("UI_Open");
            gameObject.SetActive(true);
            if (_imageMask)
                _imageMask.DOFade(0, 0.15f).From();
            if (_rootTrans)
            {
                _rootTrans.DOKill();
                _rootTrans.SetScale(0);
                _rootTrans.DOScale(1, 0.18f)
                    .SetEase(Ease.OutBack);
                //.OnComplete(() => _onOpenSubject.OnNext(gameObject));
            }//else_onOpenSubject.OnNext(gameObject);
        }

        public override void OnClose()
        {
            // _onCloseSubject.OnNext(gameObject);
            if (_imageMask)
            {
                _imageMask.DOFade(0, 0.08f);
                _imageMask.raycastTarget = false;
            }

            if (_rootTrans)
            {
                _rootTrans.DOKill();
                _rootTrans.DOScale(0, 0.1f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
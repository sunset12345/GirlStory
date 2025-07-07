using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickZoom : MonoBehaviour, 
    IPointerDownHandler, 
    IPointerUpHandler, 
    IPointerExitHandler
{
    [SerializeField] private float _zoomScale = -0.06f;
    [SerializeField] private float _duration = 0.06f;

    private bool _zoomed = false;

    private void Zoom()
    {
        if (_zoomed)
            return;

        _zoomed = true;
        transform.DOKill();
        transform.DOBlendableScaleBy(Vector3.one * _zoomScale, _duration);
    }

    private void Reset()
    {
        if (!_zoomed)
            return;

        _zoomed = false;
        transform.DOKill();
        transform.DOScale(Vector3.one, _duration);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Zoom();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Reset();
    }
}

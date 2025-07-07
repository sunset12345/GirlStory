using App.LoadingFunction;
using GSDev.UI.Layer;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI.Loading
{

    public class LoadingLayer : LayerContent
    {
        [SerializeField] private Image _progress;
        [SerializeField] private TextMeshProUGUI _value;
        [SerializeField] private TextMeshProUGUI _info;

        void Start()
        {
            LoadingManager.Instance.Progress
               .Subscribe(OnLoadingProgressUpdated)
               .AddTo(this);
            LoadingManager.Instance.Info
                .Subscribe(OnLoadingInfoUpdated)
                .AddTo(this);
        }

        void OnEnable()
        {
            _info.text = "Loading...";
            _value.text = "0.0%";
        }

        private void OnLoadingProgressUpdated(float progress)
        {
            var positionX = progress * 460 - 230;
            _progress.transform.localPosition = new Vector3(positionX, -124, 0);
            _value.text = $"{progress:P2}";
        }

        private void OnLoadingInfoUpdated(string info)
        {
            _info.text = info;
        }

        public static void Create()
        {
            LayerManager.Instance.LoadContent(
                LayerTag.Loading,
                "ui/loading/LoadingLayer");
        }
    }
}

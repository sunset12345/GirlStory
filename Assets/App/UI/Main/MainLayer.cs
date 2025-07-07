using App.LoadingFunction;
using GSDev.EventSystem;
using GSDev.UI.Layer;
using UnityEngine;

namespace App.UI.Main
{
    public class MainLayer : LayerContent
    {
        [SerializeField] private Transform navigatorRoot;
        [SerializeField] private Transform _panelRoot;


        private EventSubscriber _eventSubscriber;

        void Awake()
        {
            _eventSubscriber = new EventSubscriber();
        }

        void OnDestroy()
        {
            _eventSubscriber.Dispose();
        }

        public static void Create()
        {
            LayerManager.Instance.LoadContent(
                LayerTag.Main,
                "ui/main/MainLayer");
        }
    }

    public enum MainPanelType
    {
        None = 0,
        Main = 1,
        Video = 2,
        Match = 3,
        Message = 4,
        UserInfo = 5,
    }
}

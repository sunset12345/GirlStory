using GSDev.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI.Main
{
    public class MainNavigateItem : MonoBehaviour, IEventSender
    {
        [SerializeField] private string _panelPrefabPath;
        [SerializeField] private MainPanelType _type;

        public MainPanelType Type => _type;

        [SerializeField] private Toggle _toggle;
        public Toggle Toggle => _toggle;

        [SerializeField] private GameObject _notSelected;
        [SerializeField] private GameObject _selected;

        private AssetRef _panelAssetRef;

        public GameObject PanelPrefab => _panelAssetRef.Load<GameObject>();


        private EventSubscriber _subscriber;

        void Awake()
        {
            _subscriber = new EventSubscriber();

            _toggle.onValueChanged.AddListener(OnToggleChanged);
            _panelAssetRef = AssetRef.Parse(_panelPrefabPath);

            _subscriber.Subscribe<MainNavigatorItemSwitchEvent>(OnMainNavigatorItemSwitchEvent);
        }

        void Start()
        {
            UpdateState(_toggle.isOn);
        }

        private void OnToggleChanged(bool on)
        {
            UpdateState(on);
            if (on)
            {
                this.DispatchEvent(Witness<MainNavigatorItemSelectedEvent>._, this);
            }
        }

        private void UpdateState(bool on)
        {
            _notSelected.SetActive(!on);
            _selected.SetActive(on);
        }

        private void OnMainNavigatorItemSwitchEvent(MainNavigatorItemSwitchEvent @event)
        {
            if (@event.PanelType == _type)
            {
                _toggle.isOn = true;
            }
        }

        void OnDestroy()
        {
            _subscriber.Dispose();
        }
        public EventDispatcher Dispatcher => EventDispatcher.Global;
    }

    public class MainNavigatorItemSelectedEvent : EventBase<MainNavigateItem>
    {
        public MainNavigateItem Item => Field1;
    }

    public class MainNavigatorItemSwitchEvent : EventBase<MainPanelType>
    {
        public MainPanelType PanelType => Field1;
    }
}

using App.LoadingFunction;
using App.UI.Main;
using GSDev.UI;
using GSDev.UI.Layer;
using UnityEngine;

namespace App.UI.LogininFunction
{
    public class LogininLayer : PopupLayerContent
    {
        [SerializeField] private Button _loginButton;

        void Awake()
        {
            _loginButton.AddClick(OnClickLgoin);
        }

        private void OnClickLgoin()
        {
            Close();
            MainLayer.Create();
        }

        public static void Create()
        {
            LayerManager.Instance.LoadContent(
                LayerTag.Dialog,
                "ui/loginin/LogininLayer");
        }
    }
}

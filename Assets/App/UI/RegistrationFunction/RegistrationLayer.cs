using App.LoadingFunction;
using GSDev.UI;
using GSDev.UI.Layer;
using UnityEngine;
using TMPro;
using App.UI.Common;
using App.UI.Main;
using App.LocalData;

namespace App.UI.RegistrationFunction
{
    public class RegistrationLayer : PopupLayerContent
    {
        [SerializeField] private Button _registerButton;
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private TMP_InputField _confirmPasswordInput;

        void Awake()
        {
            _registerButton.AddClick(OnClickRegister);
        }

        private void OnClickRegister()
        {
            if (string.IsNullOrEmpty(_emailInput.text) ||
                string.IsNullOrEmpty(_passwordInput.text) ||
                string.IsNullOrEmpty(_confirmPasswordInput.text))
            {
                CommonMessageTip.Create("Please fill in all fields.");
                return;
            }
            if (!IsValidEmail(_emailInput.text))
            {
                CommonMessageTip.Create("Please enter a valid email address.");
                return;
            }
            if (_passwordInput.text != _confirmPasswordInput.text)
            {
                CommonMessageTip.Create("Passwords do not match.");
                return;
            }
            LocalDataManager.Instance.SetPlayerData(
                _emailInput.text,
                _passwordInput.text);
            CommonMessageTip.Create("Registration successful!");
            MainLayer.Create();
            Close();
        }

        private bool IsValidEmail(string text)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(text);
                return addr.Address == text;
            }
            catch
            {
                return false;
            }
        }

        public static void Create()
        {
            LayerManager.Instance.LoadContent(
                LayerTag.Dialog,
                "ui/registration/RegistrationLayer");
        }
    }
}

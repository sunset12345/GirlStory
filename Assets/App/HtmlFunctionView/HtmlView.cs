using App.IAP;
using App.LoadingFunction;
using GSDev.UI.Layer;
using GSDev.UI.Layer;
using UnityEngine;

namespace App.UI.HtmlFunction
{
    public class HtmlView : LayerContent
    {
        [SerializeField] private WebViewObject _webView;

        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("url is null");
                return;
            }
            if (_instance != null)
                return;
            _instance = this;
            _webView.Init(
                cb: MessageReceived, // 消息回调
                err: (msg) => { Debug.Log($"CallOnError [{msg}]"); },
                httpErr: (msg) => { Debug.Log($"CallOnHttpError [{msg}]"); },
                pay: (msg) =>
                {
                    IAPManager.Instance.Purchase(msg);
                },
                ld: (msg) =>
                {
                    Debug.Log($"CallOnLoaded [{msg}]");
                },
                handleSkipStore: (msg) =>
                {
#if UNITY_IOS
                    if (SystemInfo.operatingSystem.StartsWith("iOS 10.3") ||
                        IsNewerIOSVersion(10, 3))
                        UnityEngine.iOS.Device.RequestStoreReview();
                    else
                    {
                        // 旧版本跳转App Store
                        string url = $"itms-apps://itunes.apple.com/app/id{BaseConfig.AppID}?action=write-review";
                        Application.OpenURL(url);
                    }
#endif
                },
                started: (msg) => { Debug.Log($"CallOnStarted [{msg}]"); },
                hooked: (msg) => { Debug.Log($"CallOnHooked [{msg}]"); },
                cookies: (msg) => { Debug.Log($"CallOnCookies [{msg}]"); },
                transparent: true,
                enableWKWebView: true// 必须启用 WKWebView
            );

            _webView.IsInitialized();
            _webView.LoadURL(url);
            _webView.SetVisibility(true);
        }

        private bool IsNewerIOSVersion(int major, int minor)
        {
            string[] versions = SystemInfo.operatingSystem.Split(' ');
            if (versions.Length < 2) return false;

            string[] nums = versions[1].Split('.');
            int currentMajor = int.Parse(nums[0]);
            int currentMinor = nums.Length > 1 ? int.Parse(nums[1]) : 0;

            return (currentMajor > major) ||
                   (currentMajor == major && currentMinor >= minor);
        }

        private void MessageReceived(string message)
        {
            if (message.StartsWith("LOG: "))
            {
                string logMessage = message.Substring(5);
                Debug.Log("H5 日志: " + logMessage); // 输出到 Unity 控制台
            }
            else
            {
                Debug.Log("收到 H5 消息: " + message);
                // 处理业务逻辑
            }
        }

        private void OnDestroy()
        {
            if (_webView != null)
            {
                _webView.SetVisibility(false);
                _webView.DestroySelf();
                _webView = null;
            }
        }

        public void Hide()
        {
            _webView.SetVisibility(false);
        }

        public void Show()
        {
            _webView.SetVisibility(true);
        }

        private static HtmlView _instance;
        public static HtmlView Instance => _instance;

        public static void Create(string url)
        {
            var htmlView = LayerManager.Instance.LoadContent(
                          LayerTag.Dialog,
                          "ui/htmlview/HtmlView") as HtmlView;

            htmlView.OpenUrl(url);
        }
    }
}

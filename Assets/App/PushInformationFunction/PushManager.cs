using System.Runtime.InteropServices;
using GSDev.Singleton;
using UnityEngine;

namespace PushFunction
{
    public class PushManager : MonoSingleton<PushManager>
    {
#if UNITY_IOS && !UNITY_EDITOR

    // 链接原生代码
    [DllImport("__Internal")]
    private static extern void RequestPushToken();
#endif

        public void TriggerTokenRequest()
        {
#if UNITY_IOS && !UNITY_EDITOR
        RequestPushToken();
#endif
        }

        // 由 iOS 原生代码调用（成功时）
        public void OniOSPushTokenReceived(string token)
        {
            Debug.Log("iOS Push Token: " + token);
            _deviceToken = token;
            // 将 token 发送到你的服务器

            CloudPushManager.Instance.InitSDK(
                CloudPushConfig.AppKey,
                CloudPushConfig.AppSecret);
        }

        // 由 iOS 原生代码调用（失败时）
        public void OniOSPushTokenFailed(string error)
        {
            Debug.LogError("Failed to get token: " + error);
        }

        private string _deviceToken;

        public string DeviceToken => _deviceToken;
    }
}

using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using App.Network;
using GSDev.Singleton;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;

namespace App.UI.HtmlFunction
{
    [System.Serializable]
    public class DeviceInfo
    {
        public string app;
        public string appVersio;
        public int deviceOs;
        public string deviceOsVersion;
        public string deviceToken; // 建议后续改为动态获取
        public string lang;
        public int dataType;
    }

    [System.Serializable]
    public class LoginRequest
    {
        public DeviceInfo device;
        public object data = new object(); // 根据实际需求替换数据结构
    }

    [System.Serializable]
    public class LoginResponse
    {
        public LoginResponseData data;
        public int code;
        public string msg;
        public string time;
        public bool success;
    }

    [System.Serializable]
    public class LoginResponseData
    {
        public int userId;
        public string tokenKey;
        public string tokenValue;
    }

    public class HtmlViewManager : MonoSingleton<HtmlViewManager>
    {

        #region Token
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern IntPtr GetDeviceID();
#endif

        public static string GetDeviceUUID()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Marshal.PtrToStringAuto(GetDeviceID());
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }

        #endregion

        [DllImport("__Internal")]
        private static extern bool isVPNConnected();

#if UNITY_IOS && !UNITY_EDITOR
        private bool _isVpn = isVPNConnected();
#else
        private bool _isVpn = false;
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private static readonly string _language = Application.systemLanguage.ToString();
#else
        private static readonly string _language = "en";
#endif
        public string GetLanguage() => _language;

        // 登录成功回调
        public System.Action<LoginResponseData> OnLoginSuccess;
        // 登录失败回调
        public System.Action<string> OnLoginFailed;

        private bool _login = false;

        public bool Logining => _login;

        private string _deviceToken;

        public string GetDeviceToken()
        {
            if (string.IsNullOrEmpty(_deviceToken))
            {
                _deviceToken = GetDeviceUUID();
            }
            return _deviceToken;
        }

        public async Task<LoginResponse> LoginCoroutine()
        {
            try
            {
                _login = true;
                var client = HttpManager.Instance.Client;
                client.DefaultRequestHeaders.Clear();
                client.Timeout = TimeSpan.FromSeconds(5); // 设置超时时间

                // 构造请求体（自动处理默认值）
                var requestBody = new LoginRequest
                {
                    device = new DeviceInfo
                    {
                        app = BaseConfig.H5Key,
                        appVersio = Application.version,
                        deviceOs = 0,
                        deviceOsVersion = SystemInfo.operatingSystem,
                        deviceToken = GetDeviceToken(),
                        lang = _language,
                        dataType = 0,
                    }
                };
                // 序列化请求体
                string jsonPayload = JsonUtility.ToJson(requestBody);
                // 创建请求内容
                var content = new StringContent(
                    jsonPayload,
                    Encoding.UTF8,
                    "application/json" // 根据接口文档明确指定
                );
                // 创建请求对象
                var baeUrl = BaseConfig.BaseUrl;
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baeUrl}/docking/login");
                request.Content = content;
                request.Headers.TryAddWithoutValidation("Connection", "Close");
                Debug.Log($"login : {jsonPayload}");
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    LoginResponse result = JsonUtility.FromJson<LoginResponse>(responseString);
                    if (result.success && result.code == 200)
                    {
                        Debug.Log($"登录成功！Token: {result.data.tokenValue}");
                        Debug.Log($"登录成功！UserId: {result.data.userId}");
                        SaveSessionToken(result.data);
                        OnLoginSuccess?.Invoke(result.data);
                    }
                    else
                    {
                        string errorMsg = $"服务端错误: {result.msg} (Code: {result.code})";
                        Debug.LogError(errorMsg);
                        OnLoginFailed?.Invoke(errorMsg);
                        _login = false;
                    }
                }
                else
                {
                    _login = false;
                }
                return null;
            }
            catch (Exception ex)
            {
                _login = false;
                OnLoginFailed?.Invoke(ex.Message);
                Debug.LogError($"登录失败: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        void SaveSessionToken(LoginResponseData data)
        {
            // 安全存储token（建议使用加密存储）
            PlayerPrefs.SetString("auth_token", data.tokenValue);
            PlayerPrefs.SetString("token_key", data.tokenKey);
            PlayerPrefs.SetInt("user_id", data.userId);
            PlayerPrefs.Save();
        }

        [System.Serializable]
        public class UploadRequest
        {
            public DeviceInfo device;
            public UploadRequestData data;
        }

        [System.Serializable]
        public class UploadResponseData
        {
            public int direct;
            public string directLink;
        }

        [System.Serializable]
        public class UploadResponse
        {
            public UploadResponseData data;
            public int code;
            public string msg;
            public string time;
            public bool success;
        }

        [System.Serializable]
        public class UploadRequestData
        {
            public int responseType;
            public int reqType;
            public int queryType;
            public string simContent; // 根据实际情况修改
            public int sendType;
            public string osKbLan;
            public string osNaLan;
            public string osNaReg;
            public long osNaTiCo;
            public string osNaTi;
        }

        public async Task<UploadResponse> UploadCoroutine()
        {
            try
            {
                var client = HttpManager.Instance.Client;
                client.DefaultRequestHeaders.Clear();
                client.Timeout = TimeSpan.FromSeconds(20); // 设置超时时间

                // 构造请求体（自动处理默认值）
                var requestBody = new UploadRequest
                {
                    device = new DeviceInfo
                    {
                        app = BaseConfig.H5Key,
                        appVersio = Application.version,
                        deviceOs = 0,
                        deviceOsVersion = SystemInfo.operatingSystem,
                        deviceToken = GetDeviceToken(),
                        lang = _language,
                        dataType = 0,
                    },
                    data = new UploadRequestData()
                    {
                        responseType = 0,
                        reqType = _isVpn ? 1 : 0,
                        queryType = 0,
                        simContent = "",
                        sendType = 0,
                        osKbLan = SystemInfoHelper.GetKeyboardLanguage(),//键盘语言zh-Hans,en-CN,emoji
                        osNaLan = Application.systemLanguage.ToString(),//本机语言：zh-Hans
                        osNaReg = SystemInfoHelper.GetCountryCode(),//本机地区：CN
                        osNaTiCo = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),//本机时间戳：1733822847
                        osNaTi = SystemInfoHelper.GetDeviceTimeZone()//本机时区：Asia/Shanghai
                    }
                };
                // 序列化请求体
                string jsonPayload = JsonUtility.ToJson(requestBody);

                // 创建请求内容
                var content = new StringContent(
                    jsonPayload,
                    Encoding.UTF8,
                    "application/json" // 根据接口文档明确指定
                );
                // 创建请求对象
                var baeUrl = BaseConfig.BaseUrl;
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baeUrl}/docking/uploadInfo");
                request.Content = content;
                request.Headers.TryAddWithoutValidation("Connection", "Close");
                Debug.Log($"login : {jsonPayload}");
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    UploadResponse result = JsonUtility.FromJson<UploadResponse>(responseString);

                    if (result.success && result.code == 200)
                    {
                        Debug.Log($"上传成功！直连地址: {result.data.directLink}");
                        if (string.IsNullOrEmpty(result.data.directLink))
                        {
                            OnLoginFailed?.Invoke("");
                            return null;
                        }
                        HandleDirectLink(result.data);
                    }
                    else
                    {
                        Debug.LogError($"服务端错误: {result.msg} (Code: {result.code})");
                        OnLoginFailed?.Invoke(result.msg);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"上传失败: {ex.Message}\n{ex.StackTrace}");
                OnLoginFailed?.Invoke(ex.Message);
                return null;
            }
        }

        void HandleDirectLink(UploadResponseData data)
        {
            if (data.direct == 1)
            {
                // 返回的H5链接如：https://web.homichat.fun?d=deviceInfo&t=tokenValue&k=tokenKey
                // d ： 替换成将设备JSON信息Base64加密后的字符串 设备信息跟请求参数中的device一致 
                // t : 是快捷登录后的token值
                // k : 是快捷登录后的token键
                if (string.IsNullOrEmpty(data.directLink))
                    return;
                var tokenValue = PlayerPrefs.GetString("auth_token");
                var tokenKey = PlayerPrefs.GetString("token_key");

                string base64String = Base64Encoder.CreateBase64Request();
                var url = data.directLink + $"?d={base64String}&t={tokenValue}&k={tokenKey}";
                Debug.Log($"进入h5: {url}");
                HtmlView.Create(url);
            }
            else
            {
                // 正常业务逻辑
                Debug.Log($"收到服务器响应: {data.directLink}");
            }
        }



        [System.Serializable]
        public class ApplePayRequest
        {
            public DeviceInfo device;
            public ApplePayRequestData data;
        }

        [System.Serializable]
        public class ApplePayRequestData
        {
            public string chargeId;
            public string transactionId;
            public string payload;
        }

        ApplePayRequest CreateApplePayRequest(
            string chargeId,
            string transactionId,
            string payload)
        {
            return new ApplePayRequest
            {
                device = new DeviceInfo
                {
                    app = BaseConfig.H5Key,
                    appVersio = Application.version,
                    deviceOs = 0,
                    deviceOsVersion = SystemInfo.operatingSystem,
                    deviceToken = GetDeviceToken(),
                    lang = _language,
                    dataType = 0,
                },
                data = new ApplePayRequestData()
                {
                    chargeId = chargeId,
                    transactionId = transactionId,
                    payload = payload
                }
            };
        }

        public async void ApplyPay(Product product)
        {
            try
            {
                var client = HttpManager.Instance.Client;
                client.DefaultRequestHeaders.Clear();

                ReceiptInfo receipt = JsonConvert.DeserializeObject<ReceiptInfo>(product.receipt);
                var requestBody = new IosPayReqVo
                {
                    orderCode = "",
                    password = "",
                    payload = receipt.Payload,
                    transactionId = product.transactionID,
                    type = "direct",
                };

                // 序列化请求体
                string jsonPayload = JsonUtility.ToJson(requestBody);

                // 创建请求内容
                var content = new StringContent(
                    jsonPayload,
                    Encoding.UTF8,
                    "application/json" // 根据接口文档明确指定
                );
                // 创建请求对象
                var baeUrl = BaseConfig.BaseUrl;
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baeUrl}/docking/verifyPay");
                request.Content = content;
                request.Headers.TryAddWithoutValidation("Connection", "Close");
                Debug.Log($"login : {jsonPayload}");
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ApplePayResponse result = JsonUtility.FromJson<ApplePayResponse>(responseString);

                    if (result.success)
                        Debug.Log($"充值成功: {result.data}， mes: {result.msg}");
                    else
                        Debug.LogError($"充值成功失败: {result.msg} (Code: {result.code})");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [System.Serializable]
        public class ApplePayResponse
        {
            public string data;
            public int code;
            public string msg;
            public string time;
            public bool success;
        }

        [Serializable]
        public class IosPayReqVo
        {
            public string orderCode;
            public string password;
            public string payload;
            public string transactionId;
            public string type;
        }

        public class ReceiptInfo
        {
            [JsonProperty("Payload")] // Newtonsoft.Json属性
            public string Payload { get; set; }

            [JsonProperty("Store")]
            public string Store { get; set; }

            [JsonProperty("TransactionID")]
            public string TransactionId { get; set; }
        }

    }
}

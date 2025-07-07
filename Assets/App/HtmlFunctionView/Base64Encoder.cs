using System;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using App.UI.HtmlFunction;

public static class Base64Encoder 
{
    [System.Serializable]
    private class RequestData
    {
        // 使用JsonProperty处理命名规范差异
        [JsonProperty("app")]
        public string App { get; set; }

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }

        [JsonProperty("deviceOs")]
        public int DeviceOs { get; set; }

        [JsonProperty("deviceOsVersion")]
        public string DeviceOsVersion { get; set; }

        [JsonProperty("deviceToken")]
        public string DeviceToken { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("dataType")]
        public int DataType { get; set; }
    }

    public static string CreateBase64Request()
    {
        try
        {
            var requestData = new RequestData
            {
                App = BaseConfig.H5Key,
                AppVersion = Application.version,
                DeviceOs = 0,
                DeviceOsVersion = SystemInfo.operatingSystem,
                DeviceToken = HtmlViewManager.Instance.GetDeviceToken(),
                Lang = HtmlViewManager.Instance.GetLanguage(),
                DataType = 0
            };

            // 序列化为JSON字符串
            string json = JsonConvert.SerializeObject(requestData);
            
            // 转换为UTF8字节数组
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            
            // Base64编码
            return Convert.ToBase64String(jsonBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Base64编码失败: {ex.Message}");
            return string.Empty;
        }
    }
}
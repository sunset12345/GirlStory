using Newtonsoft.Json;
using UnityEngine;

namespace App.LocalData
{
    public static class LocalData
    {
        // 通用保存方法
        public static void Save<T>(string key, T data)
        {
            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
        }

        // 通用加载方法
        public static T Load<T>(string key, T defaultValue = default)
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;

            string json = PlayerPrefs.GetString(key);
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return defaultValue;
            }
        }

        // 基础类型快捷方法
        public static void SaveInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public static int LoadInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);
    }
}

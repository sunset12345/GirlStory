using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class SystemInfoHelper
{

    #region ------------------------------------------------- 键盘语言 -------------------------------------------------

    /// <summary>
    /// 获取当前输入法语言（优先获取键盘语言）
    /// </summary>
    public static string GetKeyboardLanguage()
    {
    #if UNITY_IOS && !UNITY_EDITOR
        try 
        {
            IntPtr ptr = GetiOSKeyboardLanguageNative();
            string result = Marshal.PtrToStringAnsi(ptr);
            Marshal.FreeHGlobal(ptr); // 手动释放
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError("iOS键盘语言获取失败: " + e.Message);
            return GetSystemLanguageCode();
        }
    #else
            return GetSystemLanguageCode();
    #endif
    }
    
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr GetiOSKeyboardLanguageNative();
#endif
    
    private static string GetSystemLanguageCode()
    {
        SystemLanguage lang = Application.systemLanguage;
        return lang switch
        {
            SystemLanguage.ChineseSimplified => "zh-CN",
            SystemLanguage.ChineseTraditional => "zh-TW",
            SystemLanguage.English => "en",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.Korean => "ko",
            _ => CultureInfo.CurrentCulture.TwoLetterISOLanguageName
        };
    }

    #endregion
    
    
#region ------------------------------------------------- 本机地区 -------------------------------------------------

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr GetIOSRegion();
#endif
    
    public static string GetCountryCode()
    {
#if UNITY_EDITOR
        return GetFormattedCountryCode("en-US");
#elif UNITY_IOS
        IntPtr ptr = GetIOSRegion();
        string region = Marshal.PtrToStringAnsi(ptr);
        Marshal.FreeHGlobal(ptr); // 释放内存
        return GetFormattedCountryCode(region);
#else
        return GetFormattedCountryCode("en-US");
#endif
    }
    
    private static string GetFormattedCountryCode(string rawCode)
    {
        if (string.IsNullOrEmpty(rawCode)) 
            return "US";

        // 统一替换分隔符为下划线
        var formatted = rawCode.Replace("_", "-");
        string[] segments = formatted.Split('-');
        
        // 中文特别处理
        if (segments[0].ToLower() == "zh")
        {
            foreach (var segment in segments)
            {
                if (segment == "TW" || segment == "HK" || segment == "MO")
                    return segment;
            }
            return segments.Any(s => s == "Hans") ? "CN" : "HK";
        }

        // 通用国家代码提取规则
        for (int i = segments.Length - 1; i > 0; i--)
        {
            if (segments[i].Length == 2 && char.IsLetter(segments[i][0]))
            {
                return segments[i].ToUpper();
            }
        }

        // 兜底逻辑
        return segments[0].ToUpper() switch
        {
            "EN" => "US",
            "ES" => "ES",
            "FR" => "FR",
            _ => "US"
        };
    }
    
#endregion
#region ------------------------------------------------- 本机时区 -------------------------------------------------
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr GetIOSTimeZone();
#endif

    public static string GetDeviceTimeZone()
    {
#if UNITY_EDITOR
        return "Asia/Beijing";
#elif UNITY_IOS
        IntPtr ptr = GetIOSTimeZone();
        string timeZone = Marshal.PtrToStringAnsi(ptr);
        Marshal.FreeHGlobal(ptr); // 释放内存
        return timeZone;
#else
        return "UTC";
#endif
    }
#endregion
}

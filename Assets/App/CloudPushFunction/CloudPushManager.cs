using System.Runtime.InteropServices;
using GSDev.Singleton;
using PushFunction;
using UnityEngine;

public class CloudPushManager : MonoSingleton<CloudPushManager>
{
#if UNITY_IOS && !UNITY_EDITOR
    // 声明外部方法（链接到 .mm 文件）
    [DllImport("__Internal")]
    private static extern void _InitCloudPush(string appKey, string appSecret);

    [DllImport("__Internal")]
    private static extern void _RequestAPNsPermission();

    [DllImport("__Internal")]
    private static extern void _RegisterDeviceToken(string deviceToken);
#endif

    // 供 Unity 调用的公开方法
    public void InitSDK(string appKey, string appSecret)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _InitCloudPush(appKey, appSecret);
#else
        Debug.LogWarning("CloudPush SDK 仅在 iOS 设备生效");
#endif
    }

    // 初始化成功回调（由 Objective-C 调用）
    public void OnInitSuccess()
    {
        RequestAPNsPermission();
        Debug.Log("CloudPush SDK 初始化成功");
    }

    // 初始化失败回调（由 Objective-C 调用）
    public void OnInitFailed(string error)
    {
        Debug.LogError($"CloudPush SDK 初始化失败: {error}");
    }

    public void RequestAPNsPermission()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _RequestAPNsPermission();
#else
        Debug.LogWarning("APNs 权限请求仅在 iOS 设备生效");
#endif
    }

    // APNs 权限回调方法
    public void OnAPNsPermissionGranted()
    {
        Debug.Log("APNs 权限已授权");
        var token = PushManager.Instance.DeviceToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("DeviceToken 为空，无法注册");
            OnDeviceTokenInvalid();
            return;
        }
        RegisterDeviceToken(PushManager.Instance.DeviceToken);
    }

    public void OnAPNsPermissionDenied()
    {
        Debug.LogWarning("APNs 权限被拒绝");
    }

    public void OnAPNsPermissionError(string error)
    {
        Debug.LogError($"APNs 权限请求错误: {error}");
    }

    // 供 C# 调用的注册方法
    public void RegisterDeviceToken(string hexDeviceToken)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _RegisterDeviceToken(hexDeviceToken);
#else
        Debug.LogWarning("DeviceToken 注册仅在 iOS 设备生效");
#endif
    }

    // 新增回调：处理无效 Token 格式
    public void OnDeviceTokenInvalid()
    {
        Debug.LogError("DeviceToken 格式无效（需为十六进制字符串）");
        PushManager.Instance.TriggerTokenRequest();
    }
}
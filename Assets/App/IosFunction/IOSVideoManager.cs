using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using App.UI.Common;
using GSDev.EventSystem;
using GSDev.Singleton;
using UnityEngine;
using UnityEngine.Networking;

public class IOSVideoManager : MonoSingleton<IOSVideoManager>, IEventSender
{
    public EventDispatcher Dispatcher => EventDispatcher.Global;

    [DllImport("__Internal")]
    private static extern void OpenVideoPicker();

    [DllImport("__Internal")]
    private static extern void DeleteCachedVideo(string filePath);

    // 缓存目录路径
    private string cacheDir;

    void Awake()
    {
        cacheDir = Path.Combine(Application.persistentDataPath, "CachedVideos");
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
    }

    public void PickVideo()
    {
#if UNITY_IOS && !UNITY_EDITOR
        OpenVideoPicker();
#endif
    }

    // 接收iOS传回的视频路径（通过UnitySendMessage调用）
    public void OnVideoSelected(string videoPath)
    {
        Debug.Log($"iOS传回原始路径: {videoPath}");
        StartCoroutine(CopyVideoToCache(videoPath));
    }

    // 将视频复制到缓存目录
    private IEnumerator CopyVideoToCache(string sourcePath)
    {
        Debug.Log($"CopyVideoToCache :{sourcePath} -> {cacheDir}");
        string fileName = Path.GetFileName(sourcePath);
        string destPath = Path.Combine(cacheDir, fileName);

        if (File.Exists(destPath))
        {
            Debug.Log("视频已存在，跳过复制");
            this.DispatchEvent(Witness<OnVidoeSelectEvent>._, destPath);
            yield break;
        }
        string fileURL = "file://" + sourcePath;

        using (UnityWebRequest www = UnityWebRequest.Get(uri: fileURL))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"下载失败: {www.error}");
                CommonMessageTip.Create("Video Published failed");
                yield break;
            }

            File.WriteAllBytes(destPath, www.downloadHandler.data);
            this.DispatchEvent(Witness<OnVidoeSelectEvent>._, destPath);
            Debug.Log($"视频已缓存到: {destPath}");
        }
    }

    // 删除缓存视频
    public void DeleteVideo(string fileName)
    {
        string filePath = Path.Combine(cacheDir, fileName);
#if UNITY_IOS && !UNITY_EDITOR
            DeleteCachedVideo(filePath);
#else
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
#endif
    }
}


public class OnVidoeSelectEvent : EventBase<string>
{
    public string FilePath => Field1;
}
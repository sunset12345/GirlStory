using System.Collections.Generic;
using System.Diagnostics;
using GSDev.AssetBundles;
using UnityEngine;
using UnityEngine.Networking;

namespace App.LoadingFunction
{
    /// <summary>
    /// 资源包更新处理器
    /// </summary>
    /// <remarks>
    /// 资源包更新处理器

    public class AssetBundleUpdateProcessor
    {
        private bool _needUpdate = false;
        private bool _updateSucceed = false;
        private int _remoteVersion;
        private Stopwatch _stopwatch;

        public const string VersionFileRelativePath = "assetpack/vc";

        private static string GetUpdateServerRoot()
        {
            var url = $"{VersionServerConfig.Root}/patch/{VersionServerConfig.Platform}/{Application.version}/";
            return url;
        }

        public IEnumerator<float> LoadVersionInfo(LoadStepWatch watch)
        {
            using (watch.NewStep(nameof(LoadVersionInfo)))
            {
                LoadingManager.Instance.SetInfo("Check update...");
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    // LogEvent.LogCheckHotfixFailed(
                    //     "NetworkNotReachable", 
                    //     AssetBundleManager.Instance.VersionNumber.ToString());
                    yield break;
                }

                const int timeOut = 6;
                var url = $"{GetUpdateServerRoot()}version.txt";
                var versionTextDownloader = UnityWebRequest.Get(url);
                versionTextDownloader.downloadHandler = new DownloadHandlerBuffer();
                versionTextDownloader.timeout = (int)timeOut;
                var loader = versionTextDownloader.SendWebRequest();
                var timer = 0f;

                while (!loader.isDone)
                {
                    timer += UnityEngine.Time.deltaTime;
                    if (timer >= timeOut)
                        break;
                    yield return loader.progress;
                }

                if (loader.isDone)
                {
                    if (versionTextDownloader.result != UnityWebRequest.Result.Success)
                    {
                        // LogEvent.LogCheckHotfixFailed(
                        //     versionTextDownloader.result.ToString(), 
                        //     AssetBundleManager.Instance.VersionNumber.ToString());
                    }
                    else
                    {
                        if (!int.TryParse(versionTextDownloader.downloadHandler.text, out _remoteVersion))
                        {
                            // LogEvent.LogCheckHotfixFailed(
                            //     "InvalidVersionNumber", 
                            //     AssetBundleManager.Instance.VersionNumber.ToString());
                        }
                        else
                        {
                            if (_remoteVersion <= AssetBundleManager.Instance.VersionNumber)
                            {
                                // LogEvent.LogCheckHotfixSucceeded(
                                //     AssetBundleManager.Instance.VersionNumber.ToString(),
                                //     _remoteVersion.ToString(),
                                //     0);
                            }
                            else
                            {
                                LoadingManager.Instance.CreateLoadingProcess(CheckUpdate());
                            }
                        }
                    }
                }

                yield return 1.0f;
            }
        }

        private float _updateProgress;
        private bool _updateFinished;
        private IEnumerator<float> CheckUpdate()
        {
            yield return 0f;
            _needUpdate = false;
            _updateSucceed = false;
            _updateFinished = false;

            AssetBundleManager.Instance.OnAllUpdated += UpdateFinished;
            AssetBundleManager.Instance.OnUpdateFailed += UpdateFailed;
            AssetBundleManager.Instance.OnUpdateNeeded += OnUpdateNeeded;
            AssetBundleManager.Instance.OnOneUpdated += OnOneUpdated;

            var url = $"{GetUpdateServerRoot()}{VersionFileRelativePath}";
            AssetBundleManager.Instance.CheckUpdate(url);

            while (!_updateFinished)
            {
                yield return _updateProgress;
            }

            yield return 0.99f;

            AssetBundleManager.Instance.OnAllUpdated -= UpdateFinished;
            AssetBundleManager.Instance.OnUpdateFailed -= UpdateFailed;
            AssetBundleManager.Instance.OnUpdateNeeded -= OnUpdateNeeded;
            AssetBundleManager.Instance.OnOneUpdated -= OnOneUpdated;

            // Unload all cached asset bundles
            if (_needUpdate && _updateSucceed)
                AssetBundleManager.Instance.UnloadAllAssetBundles(
                    false,
                    true);

            yield return 1f;
        }

        private void UpdateFinished(UpdateInfo info)
        {
            _updateSucceed = true;
            _updateFinished = true;
            if (_needUpdate)
            {
                // LogEvent.LogHotfixResult(
                //     true, 
                //     Mathf.RoundToInt(info.totalSizeUpdated / 1024f),
                //     _stopwatch.ElapsedMilliseconds);
                _stopwatch.Stop();
            }
        }
        private void UpdateFailed(UpdateInfo info)
        {
            _updateSucceed = false;
            _updateFinished = true;
            if (_needUpdate)
            {
                // LogEvent.LogHotfixResult(
                //     false, 
                //     Mathf.RoundToInt(info.totalSizeUpdated / 1024f),
                //     _stopwatch.ElapsedMilliseconds);
                _stopwatch.Stop();
            }
        }

        private static string Size2String(uint size)
        {
            if (size > 1024 * 1024 * 10)
                return $"{(float)size / 1024 / 1024:.##}MB";
            return $"{(float)size / 1024:.##}KB";
        }

        private static string GetLoadSizeInfo(uint loadedSize, uint totalSize)
        {
            return $"{Size2String(loadedSize)}/{Size2String(totalSize)}";
        }

        private void OnOneUpdated(UpdateInfo info)
        {
            _updateProgress = (float)info.totalSizeUpdated / info.totalSize;
            LoadingManager.Instance.SetInfo(GetLoadSizeInfo(info.totalSizeUpdated, info.totalSize));
        }

        private void OnUpdateNeeded(uint totalSize)
        {
            // LogEvent.LogCheckHotfixSucceeded(
            //     AssetBundleManager.Instance.VersionNumber.ToString(), 
            //     _remoteVersion.ToString(), 
            //     Mathf.RoundToInt(totalSize / 1024.0f));
            _stopwatch = Stopwatch.StartNew();
            _needUpdate = true;
            AssetBundleManager.Instance.StartUpdating();

            LoadingManager.Instance.SetInfo($"Loading...{GetLoadSizeInfo(0, totalSize)}");
            _updateProgress = 0f;
        }
    }

}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

namespace GSDev.AssetBundles
{
    public partial class AssetBundleManager
    {
        #region Update

        public event UpdateNeeded OnUpdateNeeded;
        public event UpdateInfoHandler OnOneUpdateStart;
        public event UpdateInfoHandler OnOneUpdating;
        public event UpdateInfoHandler OnOneUpdated;
        public event UpdateInfoHandler OnAllUpdated;
        public event UpdateInfoHandler OnUpdateFailed;
        public event UpdateInfoHandler OnOneVerifying;
        public event UpdateInfoHandler OnOneVerified;

        private List<BundleInfo> _updateList;
        private int CountToUpdate => _updateList?.Count ?? 0;

        private bool _updatingPermitted;
        private int _updatingIndex = 0;
        private uint _totalSizeUpdated = 0;
        private uint _totalSize = 0;
        private int _numBundlesUpdated = 0;

        private void ResetUpdateStatusFields()
        {
            _updatingIndex = 0;
            _totalSizeUpdated = 0;
            _totalSize = 0;
            _numBundlesUpdated = 0;
            _updateFailed = false;
        }

        private bool _updateFinished = false;
        private bool _updateFailed = false;

        public void CheckUpdate(string remoteVersionFileUrl)
        {
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor || !EnableAssetBundleHotfix)
            {
                FinishUpdate();
                return;
            }
#endif
            LoadRemoteVersionFile(remoteVersionFileUrl);
        }

        private void LoadRemoteVersionFile(string url)
        {
            _remoteVersionFileURL = $"{url}{Suffix}";
            var www = new UnityWebRequest(_remoteVersionFileURL);
            StartCoroutine(LoadRemoteVersionFileHandler(www));
        }

        private const int MaxDownloader = 6;
        public float RemoteVersionLoadTimeOut = 5f;
        private IEnumerator LoadRemoteVersionFileHandler(UnityWebRequest www)
        {
            // www.certificateHandler = new HttpsCertificateHandler();
            www.downloadHandler = new DownloadHandlerBuffer();
            var loader = www.SendWebRequest();
            var timer = 0f;
            while (timer < RemoteVersionLoadTimeOut && !loader.isDone)
            {
                yield return null;
                timer += Time.deltaTime;
            }
            if (!loader.isDone || 
                www.result != UnityWebRequest.Result.Success)
            {
                FinishUpdate(false);
            }
            else
            {
                _remoteVersionConfig = VersionConfig.ParseJson(www.downloadHandler.data);
                if (NeedUpdate())
                {
                    ResetUpdateStatusFields();
                    _updatingPermitted = false;
                    MakeUpdateList();
                    if (_totalSize > 0)
                    {
                        OnUpdateNeeded?.Invoke(_totalSize);
                        yield return new WaitUntil(() => _updatingPermitted);
                    }

                    var downloaderCount = Math.Min(_updateList.Count, MaxDownloader);
                    // if there is no new file to update,
                    // meanwhile the version number is increased,
                    // the UpdateFile function must be called once
                    // in order to save new version file
                    downloaderCount = Math.Max(1, downloaderCount);
                    for (var i = 0; i < downloaderCount; ++i)
                        UpdateFile();
                }
                else
                {
                    FinishUpdate();
                }
            }
        }

        public void StartUpdating()
        {
            _updatingPermitted = true;
        }

        bool NeedUpdate()
        {
//		    var localAppVersion = _localVersionConfig.appVersion;
//		    localAppVersion = localAppVersion.Substring(0, localAppVersion.LastIndexOf('.'));
//		    var remoteAppVersion = _remoteVersionConfig.appVersion;
//		    remoteAppVersion = remoteAppVersion.Substring(0, remoteAppVersion.LastIndexOf('.'));
//            return localAppVersion == remoteAppVersion && _localVersionConfig.versionNum < _remoteVersionConfig.versionNum;
            return (_localVersionConfig.VersionNum < _remoteVersionConfig.VersionNum) ||
                   _localVersionConfig.NeedFixBundles;
        }

        private void MakeUpdateList()
        {
            _updateList = new List<BundleInfo>(16);
            foreach (var remoteBundle in _remoteVersionConfig.Bundles)
            {
                bool isNew = true;
                foreach (var localBundle in _localVersionConfig.Bundles)
                {
                    if (remoteBundle.Name == localBundle.Name)
                    {
                        isNew = false;
                        // MD5 mismatch and updated bundle not exists
                        if (!remoteBundle.MD5.Equals(localBundle.MD5) && 
                            !File.Exists(GetUpdatedPath(remoteBundle, _remoteVersionConfig)))
                        {
                            _totalSize += remoteBundle.Size;
                            _updateList.Add(remoteBundle);
                            break;
                        }
                    }
                }

                if (isNew)
                {
                    _totalSize += remoteBundle.Size;
                    _updateList.Add(remoteBundle);
                }
            }
        }

        private void UpdateFile()
        {
            if (_updatingIndex < CountToUpdate)
            {
                var updating = _updateList[_updatingIndex];

                StartCoroutine(DownloadUpdateFileHandler(_updatingIndex));
                ++_updatingIndex;
            }
            else if (_numBundlesUpdated == CountToUpdate)
            {
                // make merged version config
                _localVersionConfig.VersionNum = _remoteVersionConfig.VersionNum;
                foreach (var remoteBundle in _remoteVersionConfig.Bundles)
                {
                    var localBundle =
                        _localVersionConfig.Bundles.FirstOrDefault(bundle => bundle.Name == remoteBundle.Name);

                    if (localBundle != null)
                    {
                        localBundle.Dependency = remoteBundle.Dependency;
                        localBundle.Encrypted = remoteBundle.Encrypted;
                        localBundle.Size = remoteBundle.Size;
                        localBundle.CRC = remoteBundle.CRC;
                        localBundle.MD5 = remoteBundle.MD5;
                    }
                    else
                    {
                        _localVersionConfig.Bundles.Add(remoteBundle);
                    }
                }

                SaveVersionConfig(_localVersionConfig);
                _bundleInfoDict = _localVersionConfig.CreateDictionary();
                _versionNumber = _localVersionConfig.VersionNum;
                FetchAllLocalizedAssetBundleNames();
                FinishUpdate();
            }

            // no bundle to update and still has bundle updating
            // then do nothing and return
        }

        private void FinishUpdate(bool succeed = true)
        {
            var info = UpdateInfo.GetInfo().SetInfo(
                "",
                1,
                1,
                _totalSizeUpdated,
                _totalSize,
                _numBundlesUpdated,
                CountToUpdate);

            if (!succeed)
            {
                if (OnUpdateFailed != null)
                {
                    OnUpdateFailed(info);
                }
            }
            else
            {
                if (OnAllUpdated != null)
                {
                    OnAllUpdated(info);
                }

                _updateFinished = true;
            }

            info.Recycle();
        }

        private IEnumerator DownloadUpdateFileHandler(int bundleIndex)
        {
            var bundleInfo = _updateList[bundleIndex];
            var bundleName = bundleInfo.Name;
//            Debug.Log($"Update {bundleName} started");

            var updateStartInfo = UpdateInfo.GetInfo().SetInfo(
                bundleName,
                0,
                bundleInfo.Size,
                _totalSizeUpdated,
                _totalSize,
                _numBundlesUpdated,
                CountToUpdate);
            OnOneUpdateStart?.Invoke(updateStartInfo);
            updateStartInfo.Recycle();

            var parentFolder = Path.GetDirectoryName(bundleName);
            if (!string.IsNullOrEmpty(parentFolder))
            {
                string childFolderPath = Path.Combine(
                    Application.persistentDataPath,
                    _remoteVersionConfig.BundleRelativePath, 
                    parentFolder);

                if (!Directory.Exists(childFolderPath))
                    Directory.CreateDirectory(childFolderPath);
            }

            var filePath = GetUpdatedPath(bundleInfo, _remoteVersionConfig);
            var filePathTemp = filePath + ".tmp";
            var downloadHandler = new DownloadHandlerFile(filePathTemp)
            {
                removeFileOnAbort = true
            };
            var www = new UnityWebRequest($"{_remoteVersionFileURL}/../{bundleInfo.GetUpdatedBundleName()}{Suffix}")
            {
                downloadHandler = downloadHandler, 
                // certificateHandler = new HttpsCertificateHandler(), 
                timeout = 60
            };
            www.SendWebRequest();
            while (!www.isDone && !www.isNetworkError && !www.isHttpError)
            {
//                var bundleSizeUpdated = (uint) www.downloadedBytes;
//                var info = UpdateInfo.GetInfo().SetInfo(
//                    bundleName,
//                    bundleSizeUpdated,
//                    bundleInfo.size,
//                    _totalSizeUpdated,
//                    _totalSize,
//                    _numBundlesUpdated,
//                    CountToUpdate);
//                OnOneUpdating?.Invoke(info);
//                info.Recycle();
//                yield return new WaitForSeconds(0.1f);
                yield return new WaitForFixedUpdate();
            }

            if (www.isNetworkError || www.isHttpError)
            {
                var info = UpdateInfo.GetInfo().SetInfo(
                    bundleName,
                    bundleInfo.Size,
                    bundleInfo.Size,
                    _totalSizeUpdated + bundleInfo.Size,
                    _totalSize,
                    _numBundlesUpdated,
                    CountToUpdate);
                OnUpdateFailed?.Invoke(info);

                info.Recycle();
            }
            else if (www.isDone)
            {
                _totalSizeUpdated += bundleInfo.Size;

                var info = UpdateInfo.GetInfo().SetInfo(
                    bundleName,
                    bundleInfo.Size,
                    bundleInfo.Size,
                    _totalSizeUpdated,
                    _totalSize,
                    _numBundlesUpdated,
                    CountToUpdate);

                var failed = false;
                if (!VerifyDownloadedFile(filePathTemp, bundleInfo.MD5))
                {
                    File.Delete(filePathTemp);
                    OnUpdateFailed?.Invoke(info);
                    failed = true;
                }
                else
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    File.Move(filePathTemp, filePath);
                    // Cancel local version config updating
                    // UpdateLocalVersionConfig(bundleInfo);
                    ++_numBundlesUpdated;
                    OnOneUpdated?.Invoke(info);
//                    Debug.Log($"Update {bundleName} finished");

                    UpdateFile();
                }

                info.Recycle();
                if (failed)
                    StopAllCoroutines();
            }
        }

        private bool VerifyDownloadedFile(string filePath, string md5)
        {
            var data = File.ReadAllBytes(filePath);
            return CheckByteMd5(data, md5);
        }

        private bool CheckByteMd5(byte[] data, string md5)
        {
            return BundleInfo.EncryptMD5(data) == md5;
        }

        private void UpdateLocalVersionConfig(BundleInfo downloadedBundle)
        {
            var isNew = true;
            foreach (var localBundle in _localVersionConfig.Bundles)
            {
                if (localBundle.Name == downloadedBundle.Name)
                {
                    isNew = false;
                    localBundle.Size = downloadedBundle.Size;
                    localBundle.MD5 = downloadedBundle.MD5;
                    localBundle.Encrypted = downloadedBundle.Encrypted;
                    break;
                }
            }

            if (isNew)
            {
                _localVersionConfig.Bundles.Add(downloadedBundle);
            }

            // save new app version
            // _localVersionConfig.appVersion = _remoteVersionConfig.appVersion;
            SaveVersionConfig(_localVersionConfig);
        }

        private void SaveVersionConfig(VersionConfig vc)
        {
            // if (string.IsNullOrEmpty(_password))
            // {
            //     File.WriteAllText(
            //         GetUpdatedVersionFilePath(),
            //         JsonMapper.ToJson(vc));
            // }
            // else
            // {
            //     File.WriteAllBytes(
            //         GetUpdatedVersionFilePath(),
            //         XXTEA.Encrypt(JsonMapper.ToJson(vc), _password));
            // }
            var path = GetUpdatedVersionFilePath();
            string dirName = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            File.WriteAllText(
                path,
                JsonMapper.ToJson(vc));
        }

        public void CheckHotfixBundles(string remoteVersionFileUrl)
        {
            if (!VerifyLocalBundles())
            {
                _localVersionConfig.NeedFixBundles = true;
                SaveVersionConfig(_localVersionConfig);
                CheckUpdate(remoteVersionFileUrl);
            }
        }

        private bool VerifyLocalBundles()
        {
            bool valid = true;
            int num = _remoteVersionConfig.Bundles.Count;
            for (int i = 0; i < num; ++i)
            {
                var remoteBundle = _remoteVersionConfig.Bundles[i];
                var info = UpdateInfo.GetInfo().SetInfo(
                    remoteBundle.Name,
                    0,
                    0,
                    0,
                    0,
                    i + 1,
                    num);
                OnOneVerifying?.Invoke(info);

                do
                {
                    // LogManager.Common.Debug("verifying bundle : {0}", remoteBundle.name);
                    var path = GetUpdatedPath(remoteBundle);
                    if (!File.Exists(path))
                    {
                        path = GetInPackagePath(remoteBundle.Name);
                        if (!File.Exists(path))
                            break;
                    }

                    byte[] data = File.ReadAllBytes(path);
                    if (CheckByteMd5(data, remoteBundle.MD5))
                        break;

                    valid = false;

                    foreach (var localBundle in _localVersionConfig.Bundles)
                    {
                        if (remoteBundle.Name == localBundle.Name)
                        {
                            localBundle.MD5 = "";
                            break;
                        }
                    }
                } while (false);

                OnOneVerified?.Invoke(info);

                info.Recycle();
            }

            return valid;
        }

        #endregion
    }
    
    public class UpdateInfo
    {
        //The asset bundle name of current asset bundle
        public string bundleName;

        //The downloaded file size of current asset bundle
        public uint bundleSizeUpdated;

        //The file size of current asset bundle
        public uint bundleSize;

        //The total file size of all asset bundles already update completed.
        public uint totalSizeUpdated;

        //The total file size of all asset bundles need to update.
        public uint totalSize;

        //The number of asset bundles already update completed.
        public int numBundleUpdated;

        //The total number of asset bundles  need to update.
        public int numBundle;

        private static Stack<UpdateInfo> _cache = new Stack<UpdateInfo>();

        public static UpdateInfo GetInfo()
        {
            if (_cache.Count == 0)
            {
                return new UpdateInfo();
            }
            else
            {
                return _cache.Pop();
            }
        }

        private UpdateInfo()
        {
        }

        public UpdateInfo SetInfo(
            string bundleName,
            uint bundleSizeUpdated,
            uint bundleSize,
            uint totalSizeUpdated,
            uint totalSize,
            int numBundleUpdated,
            int numBundle)
        {
            this.bundleName = bundleName;
            this.bundleSizeUpdated = bundleSizeUpdated;
            this.bundleSize = bundleSize;
            this.totalSizeUpdated = totalSizeUpdated;
            this.totalSize = totalSize;
            this.numBundleUpdated = numBundleUpdated;
            this.numBundle = numBundle;

            return this;
        }

        public void Recycle()
        {
            _cache.Push(this);
        }
    }
}
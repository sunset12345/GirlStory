using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xxtea;
using LitJson;

namespace GSDev.AssetBundles.Editor
{
    public enum TargetPlatform
    {
        Android,
        iOS,
        Standalone
    }

    internal class AssetBundleUpdater
    {
        private List<BundleInfo> _bundles;
        internal List<BundleInfo> Bundles => _bundles;

        private readonly List<string> _encryptedAssetBundles = new List<string>();
        private AssetBundleBuildConfig _config;

        internal AssetBundleBuildConfig Config
        {
            get
            {
                if (_config == null)
                {
                    LoadConfig();
                }

                return _config;
            }
        }

        private string _poolPath;
        private string _destPath;
        private bool _packageInApp;
        private VersionConfig _inAppVersionConfig;

        public TargetPlatform platform;
        public bool increaseVersionNumber = false;

        internal AssetBundleUpdater()
        {
            LoadConfig();
#if UNITY_ANDROID
            platform = TargetPlatform.Android;
#elif UNITY_IOS
            platform = TargetPlatform.iOS;
#endif
        }
        
        private const string ConfigFilePath = "Assets/Editor/AssetBundle/AssetBundleConfig.asset";

        private void LoadConfig()
        {
            _config = EditorHelper.LoadOrCreateScriptableObject<AssetBundleBuildConfig>(ConfigFilePath);
            _encryptedAssetBundles.Clear();
            if (_config.EncryptedAssetBundles != null)
            {
                _encryptedAssetBundles.AddRange(_config.EncryptedAssetBundles);
            }
        }

        public void CreateNewVersion(bool packageInApp)
        {
            CreateNewVersion(packageInApp, platform);
        }

        private string GetPoolPath(TargetPlatform targetPlatform)
        {
            return Path.Combine(_config.BundlePoolRelativePath, targetPlatform.ToString());
        }

        private string GetDestinationPath(
            TargetPlatform targetPlatform,
            bool inApp)
        {
            return inApp ?
                Application.dataPath + "/StreamingAssets/" + _config.BundleRelativePath :
                "AssetBundles/" + targetPlatform + "/" + Application.version + "/"+ _config.BundleRelativePath;
        }

        public void CreateNewVersion(
            bool packageInApp,
            TargetPlatform targetPlatform,
            int versionNumber = 0)
        {
            UnityEditor.AssetDatabase.RemoveUnusedAssetBundleNames();

            Console.Write("Check asset bundle loop dependency\n");
            var loopDependency = AssetBundleAnalyser.CheckLoopDependency(_bundles);
            if (loopDependency.Count > 0)
            {
                foreach (var loop in loopDependency)
                {
                    var output = new StringBuilder("Loop asset bundle dependency: ");
                    foreach (var info in loop)
                    {
                        output.Append($"{info.Name} -> \n");
                    }

                    Console.Write(output);
                }

                throw new Exception("Exception: Found looped asset bundle dependency!!!");
            }

            if (versionNumber > 0)
            {
                _config.VersionNumber = versionNumber;
            }
            else if (increaseVersionNumber)
            {
                _config.VersionNumber++;
                SaveConfig();
            }

            _packageInApp = packageInApp;
            _poolPath = GetPoolPath(targetPlatform);
            _destPath = GetDestinationPath(targetPlatform, packageInApp);

            BuildAll(targetPlatform);
            
            // for patching different asset bundles
            _inAppVersionConfig = null;
            if (!_packageInApp)
            {
                var inAppVersionFilePath = GetInAppVersionFilePath();
                if (File.Exists(inAppVersionFilePath))
                {
                    _inAppVersionConfig = VersionConfig.ParseJson(File.ReadAllBytes(inAppVersionFilePath));
                }
            }
            
            CopyAll();
            CreateVersionFile();
            AssetDatabase.Refresh();
        }

        private void BuildAll(TargetPlatform targetPlatform)
        {
            if (!Directory.Exists(_poolPath))
            {
                Directory.CreateDirectory(_poolPath);
            }

            BuildPipeline.BuildAssetBundles(
                _poolPath,
                BuildAssetBundleOptions.ChunkBasedCompression,
                GetUnityBuildTarget(targetPlatform));
        }

        private bool PatchDifferent => !_packageInApp && _inAppVersionConfig != null;

        private void CopyAll()
        {
            if (Directory.Exists(_destPath))
            {
                Directory.Delete(_destPath, true);
            }

            Directory.CreateDirectory(_destPath);

            // copy bundles
            foreach (var bundle in _bundles)
            {
                var inputPath = Path.Combine(_poolPath, bundle.Name);
                
                MemoryStream outputStream = new MemoryStream(File.ReadAllBytes(inputPath));
                if (!string.IsNullOrEmpty(_config.Password) && bundle.Encrypted)
                {
                    // encrypted
                    outputStream = new MemoryStream(XXTEA.Encrypt(
                        outputStream.ToArray(),
                        _config.Password));
                }

                var outputData = outputStream.ToArray();
                bundle.Size = (uint)outputData.Length;
                bundle.MD5 = GetMD5(outputData);
                bundle.CRC = GetBundleCRC(inputPath);

                // var bundlePath = bundle.name + AssetBundleManager.Suffix;
                var outputPath = _packageInApp ?
                    Path.Combine(_destPath, bundle.Name + AssetBundleManager.Suffix) :
                    Path.Combine(_destPath, bundle.GetUpdatedBundleName() + AssetBundleManager.Suffix);

                // if generating asset bundle patch
                // skip asset bundles which are not changed
                if (PatchDifferent)
                {
                    var inAppBundle = _inAppVersionConfig.Bundles.FirstOrDefault(
                        b => b.Name == bundle.Name);
                    if (inAppBundle != null &&
                        bundle.MD5 == inAppBundle.MD5)
                        continue;
                }

                var parentFolder = Path.GetDirectoryName(bundle.Name);
                if (!string.IsNullOrEmpty(parentFolder))
                {
                    string childFolderPath = Path.Combine(_destPath, parentFolder);
                    if (!Directory.Exists(childFolderPath))
                    {
                        Directory.CreateDirectory(childFolderPath);
                    }
                }
                
                File.WriteAllBytes(outputPath, outputData);
            }
        }

        private string GetInAppVersionFilePath()
        {
            return Path.Combine(
                Application.dataPath, 
                "Resources", 
                _config.BundleRelativePath,
                _config.VersionFileName + AssetBundleManager.Suffix);
        }

        private void CreateVersionFile()
        {
            VersionConfig vc = new VersionConfig();
            vc.VersionNum = _config.VersionNumber;
//        vc.appVersion = PlayerSettings.bundleVersion;
            vc.BundleRelativePath = _config.BundleRelativePath;

            if (PatchDifferent)
            {
                var patchBundles = new List<BundleInfo>();
                foreach (var bundle in Bundles)
                {
                    var inAppBundle = _inAppVersionConfig.Bundles.FirstOrDefault(
                        b => b.Name == bundle.Name);
                    if (inAppBundle == null ||
                        bundle.MD5 != inAppBundle.MD5)
                        patchBundles.Add(bundle);
                }

                vc.Bundles = patchBundles;
            }
            else
                vc.Bundles = Bundles;
            
            vc.Bundles.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            
            JsonWriter writer = new JsonWriter()
            {
                PrettyPrint = true
            };
            JsonMapper.ToJson(vc, writer);
            string verJson = writer.ToString();
            string outputPath;
            if (_packageInApp)
            {
                // save in package version file into resources
                // it is different from asset bundle files 
                // which are saved in streaming assets
                // because read file directly from streaming assets is too complicated
                outputPath = GetInAppVersionFilePath();
                string folder = Path.GetDirectoryName(outputPath);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }

                Directory.CreateDirectory(folder);
            }
            else
            {
                outputPath = Path.Combine(
                    _destPath, 
                    _config.VersionFileName + AssetBundleManager.Suffix);
                string versionNumberFile = _destPath + "/../version.txt";
                if (File.Exists(versionNumberFile))
                {
                    File.Delete(versionNumberFile);
                }

                File.WriteAllText(versionNumberFile, vc.VersionNum.ToString());
            }
            
            File.WriteAllText(outputPath, verJson);
        }

        public void UpdateAll(bool updateEncryptList)
        {
            if (updateEncryptList)
            {
                UpdateEncryptList();
            }

            _bundles = BundleInfo.GetAllBundles();
            foreach (var info in _bundles)
            {
                if (_encryptedAssetBundles.Contains(info.Name))
                    info.Encrypted = true;
            }
        }

        private string GetFileMD5(string path)
        {
            var data = File.ReadAllBytes(path);
            return GetMD5(data);
        }

        private string GetMD5(byte[] data)
        {
            var md5 = BundleInfo.EncryptMD5(data);
            return md5;
        }

        private readonly List<string> _assetPaths = new List<string>();
        private uint GetBundleCRC(string assetBundleName)
        {
            // _assetPaths.Clear();
            // _assetPaths.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName));
            // _assetPaths.Sort();
            // var stringBuilder = new StringBuilder();
            // foreach (var path in _assetPaths)
            // {
            //     var crc = AssetDatabase.LoadAssetAtPath(path)
            //     stringBuilder.Append()
            // }
            if (BuildPipeline.GetCRCForAssetBundle(assetBundleName, out var crc))
                return crc;
            return 0;
        }

        public bool CheckTargetWithEditor()
        {
            return GetUnityBuildTarget(platform) == EditorUserBuildSettings.activeBuildTarget;
        }

        public void SaveConfig()
        {
            UpdateEncryptList();
            _config.EncryptedAssetBundles = _encryptedAssetBundles.ToArray();
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }

        private void UpdateEncryptList()
        {
            _encryptedAssetBundles.Clear();
            foreach (var bundle in _bundles)
            {
                if (bundle.Encrypted)
                {
                    _encryptedAssetBundles.Add(bundle.Name);
                }
            }
        }

        public static BuildTarget GetUnityBuildTarget(TargetPlatform targetPlatform)
        {
            BuildTarget bt = BuildTarget.StandaloneWindows;
            switch (targetPlatform)
            {
                case TargetPlatform.Standalone:
#if UNITY_EDITOR_OSX
                    bt = BuildTarget.StandaloneOSX;
                    break;
#else
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
                        bt = BuildTarget.StandaloneWindows64;
                    else
                        bt = BuildTarget.StandaloneWindows;
                    break;
#endif
                case TargetPlatform.iOS:
                    bt = BuildTarget.iOS;
                    break;
                case TargetPlatform.Android:
                    bt = BuildTarget.Android;
                    break;
            }

            return bt;
        }

        // public static TargetPlatform GetTargetPlatform(BuildTarget buildTarget)
        // {
        //     switch (buildTarget)
        //     {
        //         case BuildTarget.Android:
        //             return TargetPlatform.Android;
        //         case BuildTarget.iOS:
        //             return TargetPlatform.iOS;
        //         case BuildTarget.StandaloneOSXIntel:
        //             return TargetPlatform.StandaloneOSXIntel;
        //         default:
        //             return TargetPlatform.StandaloneWindows;
        //     }
        // }

        // used for debugging only
        public void BuildSpecificAssetBundles(
            TargetPlatform targetPlatform,
            IReadOnlyCollection<string> bundleNames)
        {
            _poolPath = GetPoolPath(targetPlatform);
            _destPath = GetDestinationPath(
                targetPlatform,
                true);

            var assetBundleBuilds = new List<AssetBundleBuild>(bundleNames.Count);
            assetBundleBuilds.AddRange(bundleNames.Select(name => new AssetBundleBuild
            {
                assetBundleName = name,
                assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name)
            }));

            BuildPipeline.BuildAssetBundles(
                GetPoolPath(targetPlatform),
                assetBundleBuilds.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression,
                GetUnityBuildTarget(targetPlatform));

            foreach (var name in bundleNames)
            {
                var childFolder = name.LastIndexOf("/");
                if (childFolder > 0)
                {
                    var childFolderPath = _destPath + "/" + name.Substring(0, childFolder);
                    if (!Directory.Exists(childFolderPath))
                    {
                        Directory.CreateDirectory(childFolderPath);
                    }
                }

                string inputPath = Path.Combine(_poolPath, name);
                string outputPath = Path.Combine(_destPath, name + AssetBundleManager.Suffix);

                MemoryStream outputStream = new MemoryStream(File.ReadAllBytes(inputPath));
                if (!string.IsNullOrEmpty(_config.Password) && _encryptedAssetBundles.Contains(name))
                {
                    // encrypted
                    outputStream = new MemoryStream(XXTEA.Encrypt(
                        outputStream.ToArray(),
                        _config.Password));
                }

                File.WriteAllBytes(outputPath, outputStream.ToArray());
            }
        }
    }
}



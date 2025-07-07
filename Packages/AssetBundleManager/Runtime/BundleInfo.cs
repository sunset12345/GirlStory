using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GSDev.AssetBundles
{
    [Preserve]
    public class BundleInfo
    {
        public uint CRC;
        public string[] Dependency;
        public bool Encrypted;
        public string MD5;
        public string Name;
        public uint Size;
        
        // default constructor used for json deserializer
        public BundleInfo() {}
        
        public string GetUpdatedBundleName()
        {
            var fileName = Path.GetFileName(Name);
            var dir = Path.GetDirectoryName(Name);
            return Path.Combine(dir, $"{MD5}_{fileName}");
        }

#if UNITY_EDITOR
        private string[] _dependentAssets;

        public string[] GetDependentAssets()
        {
            return _dependentAssets;
        }

        private BundleInfo(string name)
        {
            this.Name = name;
            Update();
        }

        public void Update()
        {
            UpdateDependency();
        }

        private void UpdateDependency()
        {
            _dependentAssets = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPathsFromAssetBundle(Name), true);
            var bundleHashSet = new HashSet<string>();
            foreach (var dependentAssetPath in _dependentAssets)
            {
                var assetBundleName = AssetDatabase.GetImplicitAssetBundleName(dependentAssetPath);
                if (string.IsNullOrEmpty(assetBundleName) ||
                    // split name by '.' to compare name without bundle variable
                    assetBundleName == Name.Split('.')[0])
                    continue;
                bundleHashSet.Add(assetBundleName);
            }

            Dependency = bundleHashSet.ToArray();
        }

        public static List<BundleInfo> GetAllBundles()
        {
            var list = new List<BundleInfo>();
            var unusedBundle = AssetDatabase.GetUnusedAssetBundleNames();
            var existBundleNames = AssetDatabase.GetAllAssetBundleNames();

            foreach (var bundleName in existBundleNames)
            {
                if (unusedBundle.Contains(bundleName)) continue;
                var bundle = new BundleInfo(bundleName);
                bundle.Update();
                list.Add(bundle);
            }

            return list;
        }
#endif
        
        public static string EncryptMD5(byte[] data)
        {
            MD5 md5Provider = new MD5CryptoServiceProvider();
            byte[] hash = md5Provider.ComputeHash(data);
            string md5 = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

            return md5;
        }
    }
}
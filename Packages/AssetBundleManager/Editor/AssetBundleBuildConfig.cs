using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GSDev.AssetBundles.Editor
{
    internal class AssetBundleBuildConfig : ScriptableObject
    {
        public string BundleRelativePath = "publish";
        public string VersionFileName = "vc";
        public string BundlePoolRelativePath = "AssetBundlePool";
        public string Password = "password";
        public int VersionNumber = 1;
    
        public string[] EncryptedAssetBundles;
    }
}

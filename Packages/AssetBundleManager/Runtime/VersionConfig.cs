using System.Collections.Generic;
using System.Text;
using UnityEngine.Scripting;
using LitJson;

namespace GSDev.AssetBundles
{
    [Preserve]
    public class VersionConfig
    {
        public int VersionNum;
        
        public string BundleRelativePath;
        public bool NeedFixBundles = false;
        public List<BundleInfo> Bundles;
        
        // default constructor used for json deserializer
        public VersionConfig() {}

        public Dictionary<string, BundleInfo> CreateDictionary()
        {
            Dictionary<string, BundleInfo> dic = new Dictionary<string, BundleInfo>();
            for (int i = 0; i < Bundles.Count; ++i)
            {
                dic.Add(Bundles[i].Name, Bundles[i]);
            }

            return dic;
        }
        
        public static VersionConfig ParseJson(byte[] data)
        {
            UTF8Encoding utf8 = new UTF8Encoding();

            return JsonMapper.ToObject<VersionConfig>(utf8.GetString(data));
        }
    }
}

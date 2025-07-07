using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSDev.AssetBundles.Editor
{
    public static class  BundleInfoExt
    {
        public static string GetNameWithoutVariaty(this BundleInfo info)
        {
            return info.Name.Split('.')[0];
        }
    }
    
    public static class AssetBundleAnalyser
    {
        private class BundleInfoNameCompare : IEqualityComparer<BundleInfo>
        {
            public bool Equals(BundleInfo x, BundleInfo y)
            {
                return x.GetNameWithoutVariaty() == y.GetNameWithoutVariaty();
            }

            public int GetHashCode(BundleInfo obj)
            {
                return obj.GetNameWithoutVariaty().GetHashCode();
            }
        }
        public static List<BundleInfo[]> CheckLoopDependency(IList<BundleInfo> bundleInfos)
        {
            var output = new List<BundleInfo[]>();
            var bundleInfoDictionary = bundleInfos.
                Distinct(new BundleInfoNameCompare()).
                ToDictionary(info => info.GetNameWithoutVariaty());
            var loadStack = new Stack<BundleInfo>();
            var loopedBundle = new HashSet<string>();
            
            void LoadCheck(BundleInfo info)
            {
                if (loopedBundle.Contains(info.Name))
                    return;
                
                var loadedBefore = loadStack.LastOrDefault(i => i.Name == info.Name) != null;
                loadStack.Push(info);
                
                if (loadStack.Count > 2 && loadedBefore)
                {
                    output.Add(loadStack.Reverse().ToArray());
                    loopedBundle.Add(info.Name);
                }
                else
                {
                    foreach (var dependentBundle in info.Dependency)
                    {
                        if (bundleInfoDictionary.TryGetValue(dependentBundle, out var dependentBundleInfo))
                        {
                            LoadCheck(dependentBundleInfo);
                        }
                        else
                        {
                            Debug.Log($"Bundle not found {dependentBundle}");
                        }
                    }
                }

                loadStack.Pop();
            }

            foreach (var info in bundleInfos)
            {
                LoadCheck(info);
            }

            return output;
        }
    }
}


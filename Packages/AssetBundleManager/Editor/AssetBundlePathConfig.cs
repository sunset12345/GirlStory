using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GSDev.AssetBundles.Editor
{
    public class AssetBundlePathConfig : ScriptableObject
    {
        [Header("配置规则列表")]
        public List<AssetBundlePathRuleParam> RuleList = new List<AssetBundlePathRuleParam>();

        public static void ApplyRule(AssetBundlePathRuleParam rule)
        {
            if (rule.RuleType == AssetBundleRuleType.Folder)
                AssetBundlePathSetter.UpdatePathByFolder(
                    rule.Path,
                    rule.Prefix,
                    rule.Depth,
                    rule.ExcludeFolderName);
            else
                AssetBundlePathSetter.UpdatePathByAsset(
                    rule.Path,
                    rule.Prefix,
                    rule.SearchPattern,
                    rule.IsRecursionFolder);
        }

        public void ApplyAllRules()
        {
            foreach (var rule in RuleList)
            {
                ApplyRule(rule);
            }
        }
    }

    public enum AssetBundleRuleType
    {
        Folder,
        Asset
    }


    [Serializable]
    public class AssetBundlePathRuleParam
    {
        public string RuleName;
        public AssetBundleRuleType RuleType;
        public string Path;
        public DefaultAsset Root;
        public string Prefix;
        public int Depth;
        public string ExcludeFolderName = "";
        public string SearchPattern = "*";
        public bool IsRecursionFolder = true;
    }
}
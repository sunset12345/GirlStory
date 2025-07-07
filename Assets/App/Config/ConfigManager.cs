using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GSDev.CSVConfig;
using GSDev.Singleton;
using UnityEngine;
using System.Text;
using GSDev.UserLabel;
using GSDev.AssetBundles;
using System.Reflection;
using System.Linq;

namespace App.Config
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private const string Suffix = ".csv";

        private static readonly string TableConfigFileNameProperty = "ConfigName";
        private static readonly string DefaultTableBundle = "config/tables/";
        private static readonly string DefaultConstBundle = "config/consts/";
        
        private static readonly StringBuilder StringBuilder = new (128);
        private readonly Dictionary<Type, ConfigBase> _configs = new ();
        private readonly Dictionary<Type, IConfigRef> _refs = new ();
        
        public async static UniTask<ConfigBase> LoadConfigAsync(
            Type type,
            string path,
            bool mergeReplace = false)
        {
            if (!type.IsSubclassOf(typeof(ConfigBase)))
            {
                Debug.LogError($"Config loading failed! Type {type} is not subclass of ConfigBase");
                return null;
            }
            var replaced = UserLabelManager.Instance.GetReplacement(path);
            if (!replaced.ResolveAssetPath(out var bundle, out var asset))
                return null;
            var loader = AssetBundleManager.Instance.LoadAssetAsync<TextAsset>(
                bundle,
                asset);
            await loader.ToUniTask();
            if (loader.asset == null)
                return null;
            
            var config = Activator.CreateInstance(type) as ConfigBase;
            if (config == null)
            {
                Debug.LogError($"Config loading failed! Type {type} is not subclass of ConfigBase");
                return null;
            }
            if (mergeReplace &&
                replaced != path &&
                path.ResolveAssetPath(out bundle, out asset))
            {
                // 常量表支持部分数据替换
                // 此处针对常量表，先加载原始常量，再加载分层替换后的数据对原表进行内容替换
                // 这样分层后的表即使遗漏字段也不会出现问题，有原表的数据来补充
                var originTextAsset = AssetBundleManager.Instance.LoadAsset<TextAsset>(
                    bundle,
                    asset);
                if (originTextAsset != null)
                    config.ParseConfigure(originTextAsset.text);
            }
            
            config.ParseConfigure(loader.asset.text);
            return config;
        }

        private async UniTask LoadCSVTableAsync(Type type, string bundle = null)
        {
            var fileNameProperty = type.GetProperty(
                TableConfigFileNameProperty,
                BindingFlags.Static | BindingFlags.Public);
            var fileName = fileNameProperty?.GetValue(null, null) as string;
            Debug.Assert(!string.IsNullOrEmpty(fileName));
            if (string.IsNullOrEmpty(bundle))
                bundle = DefaultTableBundle;
            var path = StringBuilder
                .Clear()
                .Append(bundle)
                .Append(fileName)
                .ToString();
            _configs[type] = await LoadConfigAsync(type, path);
        }

        private async UniTask LoadCSVConstAsync(Type type, string bundle = null)
        {
            var path = StringBuilder
                .Clear()
                .Append(DefaultConstBundle)
                .Append(type.Name)
                .ToString();
            _configs[type] = await LoadConfigAsync(
                type, 
                path, 
                true);
        }

        public T GetConfig<T>() where T : ConfigBase
        {
            if (_configs.TryGetValue(typeof(T), out var config)) 
                return config as T;
            return null;
        }
        
        public ConfigBase GetConfig(Type type)
        {
            if (_configs.TryGetValue(type, out var config)) 
                return config;
            return null;
        }
        
        public T GetRef<T>() where T : class, IConfigRef
        {
            if (_refs.TryGetValue(typeof(T), out var config)) 
                return config as T;
            return null;
        }
        
        public IConfigRef GetRef(Type type)
        {
            if (_refs.TryGetValue(type, out var config)) 
                return config;
            return null;
        }
        
        public async UniTask LoadAll()
        {
            if (_configs.Count > 0)
                _configs.Clear();
            if (_refs.Count > 0)
                _refs.Clear();
            
            GC.Collect();
            
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            
            // get all types inherit from ConfigBase
            var baseType = typeof(ConfigBase);
            var types = allTypes
                .Where(t => t.IsSubclassOf(baseType))
                .Except(_autoLoadIgnore);

            var tasks = new List<UniTask>();
            foreach (var type in types)
            {
                tasks.Add(type.Name.Contains("const", StringComparison.InvariantCultureIgnoreCase)
                    ? LoadCSVConstAsync(type)
                    : LoadCSVTableAsync(type));
            }

            await UniTask.WhenAll(tasks);
            
            // get all types implemented IConfigRef
            var refType = typeof(IConfigRef);
            types = allTypes
                .Where(t => t.IsClass && refType.IsAssignableFrom(t));
            foreach (var configRefType in types)
            {
                var configRef = Activator.CreateInstance(configRefType) as IConfigRef;
                if (configRef == null)
                    continue;
                configRef.Init(this);
                _refs[configRefType] = configRef;
            }
            
            foreach (var configRef in _refs.Values)
            {
                configRef.PostProcessData(this);
            }
        }
        
        private static readonly List<Type> _autoLoadIgnore = new ()
        {
        };
    }
}
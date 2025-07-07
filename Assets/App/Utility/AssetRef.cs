using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GSDev.AssetBundles;
using UnityEngine;

public class AssetRef
{
    private static Dictionary<string, AssetRef> _cache = new Dictionary<string, AssetRef>();

    private string _assetBundlePath;
    public string AssetBundlePath => _assetBundlePath;
    private string _assetName;
    public string AssetName => _assetName;

    private UnityEngine.Object _asset;
    private UnityEngine.Object[] _subAssets;

    private int _refCounter = 0;
    public int RefCounter => _refCounter;

    public static AssetRef Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;
        if (_cache.TryGetValue(content, out var assetRef))
            return assetRef;
        
        var separator = content.LastIndexOf('/');
        if (separator < 0)
            return null;

        string assetName;
        string bundleName;
        if (content[content.Length - 1] == '*')
        {
            // 路径结尾为*，表明最底层bundle包名同资源名
            assetName = content.Substring(separator + 1, content.Length - separator - 2);
            bundleName = content.Substring(0, content.Length - 1);
        }
        else
        { 
            assetName = content.Substring(separator + 1, content.Length - separator - 1);
            bundleName = content.Substring(0, separator);
        }
        assetRef = new AssetRef()
        {
            _assetBundlePath = bundleName,
            _assetName = assetName
        };
        _cache.Add(content, assetRef);
        return assetRef;
    }

    private bool TryGetCache<T>(out T output) where T : UnityEngine.Object
    {
        output = default;
        if (_asset == null)
            return false;
        if (_asset is T asset)
        {
            output = asset;
            return true;
        }
        Debug.unityLogger.Log(LogType.Error,$"AssetRef load type error! Bundle:{_assetBundlePath}, Asset:{_assetName}, Type:{typeof(T).Name}");
        return false;
    }

    public T Load<T>() where T : UnityEngine.Object
    {
        ++_refCounter;
        
        if (TryGetCache<T>(out var output))
            return output;
        _asset = AssetBundleManager.Instance.LoadAsset<T>(_assetBundlePath, _assetName);
        return (T)_asset;
    }
    
    public async UniTask<T> LoadAsync<T>() where T : UnityEngine.Object
    {
        ++_refCounter;
        
        if (TryGetCache<T>(out var output))
            return output;
        var loader = AssetBundleManager.Instance.LoadAssetAsync<T>(_assetBundlePath, _assetName);
        await loader.ToUniTask();
        _asset = loader.asset;
        return (T)_asset;
    }

    public UnityEngine.Object Load(Type type)
    {
        ++_refCounter;
        if (_asset != null)
        {
            if (_asset.GetType() == type)
                return _asset;
            else
            {
                Debug.LogError($"AssetRef load type error! Bundle:{_assetBundlePath}, Asset:{_assetName}, Type:{type.Name}");
                return null;
            }
        }

        _asset = AssetBundleManager.Instance.LoadAsset(_assetBundlePath, _assetName, type);
        return _asset;
    }

    public T[] LoadAllSubAssets<T>() where T : UnityEngine.Object
    {
        ++_refCounter;
        if (_subAssets != null)
        {
            if (_subAssets is T[] output)
                return output;
            else
            {
                Debug.LogError($"AssetRef load type error! Bundle:{_assetBundlePath}, Asset:{_assetName}, Type:{typeof(T).Name}");
                return null;
            }
        }

        _subAssets = AssetBundleManager.Instance.LoadAssetWithSubAssets<T>(_assetBundlePath, _assetName);
        return (T[])_subAssets;
    }

    public void Release(bool force = false)
    {
        if (force)
            _refCounter = 0;
        else
        {
            _refCounter = Mathf.Max(0, _refCounter - 1);
        }

        if (_refCounter == 0)
        {
            AssetBundleManager.Instance.UnloadAssetBundle(_assetBundlePath);
            if (_asset != null)
            {
                Resources.UnloadAsset(_asset);
                _asset = null;
            }

            if (_subAssets != null)
            {
                foreach (var subAsset in _subAssets)
                {
                    Resources.UnloadAsset(subAsset);
                    _subAssets = null;
                }
            }
        }
    }

    public void LoadScene(bool additive = false)
    {
        ++_refCounter;
        AssetBundleManager.Instance.LoadScene(
            _assetBundlePath,
            _assetName,
            additive);
    }

    public LoadSceneOperation LoadSceneAsync(bool additive = false)
    {
        ++_refCounter;
        return AssetBundleManager.Instance.LoadSceneAsync(
            _assetBundlePath,
            _assetName,
            additive);
    }

    public static void ReleaseAll()
    {
        foreach (var v in _cache.Values)
            v.Release(true);
    }
}

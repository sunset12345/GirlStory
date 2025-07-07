using UnityEngine;
using System.Collections;

namespace GSDev.AssetBundles
{
	public abstract class AssetBundleLoadOperation : IEnumerator
	{
		public object Current
		{
			get
			{
				return null;
			}
		}
		public bool MoveNext()
		{
			return !IsDone();
		}
		
		public void Reset()
		{
		}
		
		abstract public bool IsDone ();
	}

    public class LoadAssetBundleOperation : AssetBundleLoadOperation
    {
        protected readonly string _bundleName;
        public string bundleName => _bundleName;
        
        private AssetBundle _bundle;
        public AssetBundle bundle
        {
            get { return _bundle; }
            set { _bundle = value; }
        }
        
        private bool _forceDone = false;

        public bool persistent { get; }

        public LoadAssetBundleOperation(
            string bundleName,
            bool persistent)
        {
            _bundleName = bundleName;
            this.persistent = persistent;
        }

        public void ForceDone()
        {
            _forceDone = true;
        }

        public override bool IsDone()
        {
            return _bundle != null ||
                _forceDone;
        }
    }


    public class LoadAssetOperation<T> : AssetBundleLoadOperation
        where T : UnityEngine.Object
	{
        public bool persistent { get; }
        protected string _bundleName;
        public string bundleName => _bundleName;
        
        protected string _assetName;
        public string assetName => _assetName;
        
        protected AssetBundle _bundle;
        public AssetBundle bundle
        {
            get { return _bundle; }
            set { _bundle = value; }
        }

        private T _asset;
        public T asset
        {
            get => _asset;
            set => _asset = value;
        }
        
        public LoadAssetOperation() {}

        public LoadAssetOperation(string bundleName,
            string assetName, bool persistent = false)
        {
            this.persistent = persistent;
            Setup(bundleName, assetName);
        }

        public void Setup(string bundleName, string assetName)
        {
            _bundleName = bundleName;
            _assetName = assetName;
            _asset = null;
            _bundle = null;
        }

        public T GetAsset()
        {
            return _asset;
        }

        public override bool IsDone()
        {
            return _asset != null;
        }
    }

    public class LoadMultiAssetsOperation<T> : AssetBundleLoadOperation
        where T : UnityEngine.Object
    {
        protected readonly string _bundleName;
        public string bundleName => _bundleName;
        
        protected readonly string _assetName;
        public string assetName => _assetName;
        
        public bool persistent { get; }

        public LoadMultiAssetsOperation(
            string bundleName,
            string assetName,
            bool persistent = false)
        {
            _bundleName = bundleName;
            _assetName = assetName;
            this.persistent = persistent;
        }
        
        protected AssetBundle _bundle;
        public AssetBundle bundle
        {
            get { return _bundle; }
            set { _bundle = value; }
        }

        private UnityEngine.Object[] _assets;

        public UnityEngine.Object[] assets
        {
            get => _assets;
            internal set => _assets = value;
        }

        public int count => _assets?.Length ?? 0;

        public override bool IsDone()
        {
            return _assets != null;
        }
    }

    public class LoadSceneOperation : AssetBundleLoadOperation
    {
        public string bundleName
        {
            get;
            private set;
        }

        public string levelName
        {
            get;
            private set;
        }

        public bool additive
        {
            get;
            private set;
        }

        private AsyncOperation _sceneLoadOperation;
        public AsyncOperation sceneLoadOperation
        {
            get { return _sceneLoadOperation; }
            set { _sceneLoadOperation = value; }
        }

        public LoadSceneOperation(
            string bundleName,
            string levelName,
            bool additive)
        {
            this.bundleName = bundleName;
            this.levelName = levelName;
            this.additive = additive;
        }

        override public bool IsDone()
        {
            return _sceneLoadOperation != null && _sceneLoadOperation.isDone;
        }

        public float progress
        {
            get { return _sceneLoadOperation != null ? _sceneLoadOperation.progress : 0.0f; }
        }
    }
    
}

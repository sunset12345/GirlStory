using UnityEngine;

namespace GSDev.Singleton
{
    public abstract class MonoSingleton<T>
        : MonoBehaviour
        where T : MonoSingleton<T>
    {
        private const string ROOT_NAME = "SingletonRoot";
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).Name);
                    gameObject.transform.SetParent(Root.transform);
                    _instance = gameObject.AddComponent<T>();
                }

                return _instance;
            }
        }

        private static GameObject Root
        {
            get
            {
                GameObject singletonRoot = GameObject.Find(ROOT_NAME);
                if (singletonRoot == null)
                {
                    singletonRoot = new GameObject(ROOT_NAME);
                    DontDestroyOnLoad(singletonRoot);
                }

                return singletonRoot;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
                transform.SetParent(Root.transform);
                Init();
            }
            else if (_instance != this)
            {
                Debug.LogErrorFormat("Singleton duplicated class:{0}", GetType().Name);
            }
        }

        protected virtual void Init()
        {
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
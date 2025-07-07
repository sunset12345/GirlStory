using System;

namespace GSDev.Singleton
{
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance => _instance ??= new T();

        public virtual void Destroy()
        {
            _instance = null;
        }
    }
}

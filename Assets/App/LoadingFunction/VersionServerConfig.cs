namespace App.LoadingFunction
{
    public static class VersionServerConfig
    {
#if ENABLE_GM
        public const string Root = "";
#else
        public const string Root = "";
#endif
        
#if UNITY_ANDROID
        public const string Platform = "android";
#elif UNITY_IOS
        public const string Platform = "ios";
#else
        public const string Platform = "android";
#endif
    }
}
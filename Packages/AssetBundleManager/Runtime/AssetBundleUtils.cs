namespace GSDev.AssetBundles
{
    public static class AssetBundleUtils
    {
        public const char PathSeparator = '/';

        public static bool ResolveAssetPath(
            this string path,
            out string bundle,
            out string asset)
        {
            bundle = null;
            asset = null;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var separator = path.LastIndexOf(PathSeparator);
            if (separator <= 0)
                return false;
        
            bundle = path.Substring(0, separator);
            asset = path.Substring(
                separator + 1, 
                path.Length - separator - 1);
            return true;
        }
    }
}
using System.Text.RegularExpressions;

namespace GSDev.CSVConfig.Editor
{
    internal static class TypeMatchRegex
    {
        public static readonly Regex SbyteRegex = new Regex("^sbyte$");
        public static readonly Regex ByteRegex  = new Regex("^byte$");
        public static readonly Regex ShortRegex = new Regex("^short$");
        public static readonly Regex UshortRegex = new Regex("^ushort$");
        public static readonly Regex IntRegex  = new Regex("^int$");
        public static readonly Regex UintRegex = new Regex("^uint$");
        public static readonly Regex LongRegex = new Regex("^long$");
        public static readonly Regex UlongRegex = new Regex("^ulong$");
        public static readonly Regex FloatRegex = new Regex("^float");
        public static readonly Regex BoolRegex = new Regex("^bool$");
        public static readonly Regex StringRegex = new Regex("^string$");
        public static readonly Regex EnumRegex = new Regex("^enum@.+$");
        public static readonly Regex ClassRegex = new Regex("^class@.+$");
        public static readonly Regex DictRegex = new Regex("^Dictionary<[^&]+&.+>$");
        public static readonly Regex DictListRegex = new Regex("^Dictionary<[^&]+&List<.+>>$");
        public static readonly Regex ListRegex = new Regex("^List<.+>$");
    }
}
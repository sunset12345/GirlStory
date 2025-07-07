namespace GSDev.CSVConfig.Editor
{
    internal static class FlagChar
    {
        public static readonly string[] LineSeparators = new[] {"\n"};
        public const char CommonHeader = '#';
        public const char TitleHeader = '%';
        public const char PropertyHeader = '$';
        public const char ListSeparator = '|';
        public const char DictionarySeparator = '|';
        public const char KeyValueSeparator = ':';
        public static readonly string[] FieldSeparator = new[] {","};
    }
}
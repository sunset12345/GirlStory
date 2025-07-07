using System;

namespace GSDev.CSVConfig
{
    public abstract class ConfigBase
    {
        public static Action<string> ErrorLogger;

        public abstract void ParseConfigure(string text);

        protected void LogError(string errorInfo)
        {
            ErrorLogger?.Invoke(errorInfo);
        }

        protected void LogError(string errorInfo, params object[] objects)
        {
            var info = string.Format(errorInfo, objects);
            ErrorLogger?.Invoke(info);
        }
    }
}
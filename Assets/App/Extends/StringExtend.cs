using System;
using System.Text;

public static class StringExtend
{
    private static readonly StringBuilder _stringBuilder = new StringBuilder(256);
    public static StringBuilder Builder => _stringBuilder.Clear();

    public static StringBuilder Append(this string self, string arg)
    {
        return _stringBuilder.Clear().Append(self).Append(arg);
    }

    public static StringBuilder Append(this string self, int arg)
    {
        return _stringBuilder.Clear().Append(self).Append(arg);
    }

    public static float ToFloat(this string self)
    {
        return Convert.ToSingle(self, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static int ToInt(this string self)
    {
        return Convert.ToInt32(self);
    }
}

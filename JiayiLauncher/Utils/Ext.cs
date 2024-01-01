using System;

namespace JiayiLauncher.Utils
{
    public static class Ext
    {
        public static string Truncate(this string value, int length)
        {
            return value[..Math.Min(length, value.Length)];
        }
    }
}

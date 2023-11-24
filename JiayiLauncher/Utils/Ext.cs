using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

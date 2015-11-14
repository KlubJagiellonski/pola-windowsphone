using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class SystemExtensions
    {
        public static string ToEanString(this string value)
        {
            if (value.Length == 13)
                return string.Format("{0} {1} {2}", value.Substring(0, 1), value.Substring(1, 6), value.Substring(7, 6));
            else if (value.Length == 8)
                return value.Insert(4, " ");
            return value;
        }
    }
}

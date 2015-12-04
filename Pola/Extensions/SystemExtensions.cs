
namespace System
{
    public static class SystemExtensions
    {
        /// <summary>
        /// Returns a new string with white spaces between groups of numbers in a barcode string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToEanString(this string value)
        {
            if (value.Length == 13)
                return string.Format("{0} {1} {2}", value.Substring(0, 1), value.Substring(1, 6), value.Substring(7, 6));
            if (value.Length == 12)
                return string.Format("{0} {1} {2} {3}", value.Substring(0, 1), value.Substring(1, 5), value.Substring(6, 5), value.Substring(11, 1));
            if (value.Length == 8)
                return value.Insert(4, " ");
            return value;
        }
    }
}

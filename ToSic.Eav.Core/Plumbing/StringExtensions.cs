using System;
using System.Linq;
using static System.StringComparison;

namespace ToSic.Eav.Plumbing
{
    public static class StringExtensions
    {
        /// <summary>
        /// Check if a string is null or empty. Whitespace is treated as not-empty, use <see cref="IsEmptyOrWs"/> for that.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string value) => string.IsNullOrEmpty(value);
        public static bool IsEmptyOrWs(this string value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Has real value, so neither null, empty or white space.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasValue(this string value) => !string.IsNullOrWhiteSpace(value);

        public static string UseFallbackIfNoValue(this string value, string fallback) => value.HasValue() ? value : fallback;

        public static string NullIfNoValue(this string value) => value.HasValue() ? value : null;

        public static string[] SplitNewLine(this string value) 
            => value?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        public static string[] TrimmedAndWithoutEmpty(this string[] value) 
            => value?
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

        public static string NeverNull(this string value) => value ?? "";

        /// <summary>
        /// Null-safe string-equals method. If both are null, it's equal.
        /// </summary>
        /// <param name="a">The initial string, can also be null.</param>
        /// <param name="b">The compare string, can also be null</param>
        /// <returns></returns>
        public static bool EqualsInsensitive(this string a, string b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Equals(b, InvariantCultureIgnoreCase);
        }

        public static bool ContainsInsensitive(this string a, string b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.IndexOf(b, 0, CurrentCultureIgnoreCase) != -1;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (value == null) return null;
            if (maxLength == 0) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }


        /// <summary>
        /// Make sure a string can be used as key - the core mission is to ensure that null-values are not the same as empty strings.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsKey(this string value) 
            => value ?? NullKey;

        private const string NullKey = "\0";


        // https://stackoverflow.com/questions/2571716/find-nth-occurrence-of-a-character-in-a-string
        public static int GetNthIndex(this string s, char t, int n)
        {
            if (s.IsEmpty()) return -1;
            var count = 0;
            for (var i = 0; i < s.Length; i++)
                if (s[i] == t)
                    if (++count == n)
                        return i;
            return -1;
        }
    }
}

using System;
using static System.StringComparison;

namespace ToSic.Eav.Plumbing
{
    /// <summary>
    /// IMPORTANT
    /// This code is duplicate with RazorBlades implementation
    /// ATM we leave it here, because we don't want a reference to RazorBlade in the EAV project
    /// but some day in future we should probably decide to optimize
    /// </summary>

    public static class SubstringExtensions
    {
        /// <summary>
        /// Get string value after the (first occurrence) key.
        /// Case insensitive by default. 
        /// Will return null in various cases which don't make sense or if not found.
        /// Very safe and robust, fully tested. 
        /// </summary>
        /// <param name="value">The initial string. If null, will always return null or the specified fallback.</param>
        /// <param name="key">The key to find. If null, will always return null or the specified fallback</param>
        /// <param name="caseSensitive">Set to true if you need case-sensitive compare</param>
        /// <returns></returns>
        public static string After(this string value, string key, bool caseSensitive = false)
            => value.AfterInternal(key, caseSensitive ? Ordinal : InvariantCultureIgnoreCase);

        /// <summary>
        /// Get string value after the last occurrence of a key.
        /// Will return null in various cases which don't make sense or if not found.
        /// Case insensitive by default. 
        /// Very safe and robust, fully tested.
        /// </summary>
        /// <param name="value">The initial string. If null, will always return null or the specified fallback.</param>
        /// <param name="key">The key to find. If null, will always return null or the specified fallback</param>
        /// <param name="caseSensitive">Set to true if you need case-sensitive compare</param>
        /// <returns></returns>
        public static string AfterLast(this string value, string key, bool caseSensitive = false)
            => value.AfterInternal(key, caseSensitive ? Ordinal : InvariantCultureIgnoreCase, true);

        private static string AfterInternal(this string value,
            string key, 
            StringComparison comparison,
            bool findLast = false, 
            string fallback = null)
        {
            if (value == null || key == null) return fallback;
            var posA = findLast ? value.LastIndexOf(key, comparison) : value.IndexOf(key, comparison);
            if (posA == -1) return fallback;
            var adjustedPosA = posA + key.Length;
            // If found but nothing left, return "" which is correct
            if (adjustedPosA >= value.Length) return "";
            return value.Substring(adjustedPosA);
        }

        /// <summary>
        /// Get string value before the key.
        /// Case insensitive by default. 
        /// Will return null in various cases which don't make sense or if not found.
        /// Very safe and robust, fully tested.
        /// </summary>
        /// <param name="value">The initial string. If null, will always return null or the specified fallback.</param>
        /// <param name="key">The key to find. If null, will always return null or the specified fallback</param>
        /// <param name="caseSensitive">Set to true if you need case-sensitive compare</param>
        /// <returns></returns>
        public static string Before(this string value, string key, bool caseSensitive = false)
            => value.BeforeInternal(key, caseSensitive ? Ordinal : InvariantCultureIgnoreCase);

        /// <summary>
        /// Get string value before the last occurrence of a key.
        /// Case insensitive by default. 
        /// Will return null in various cases which don't make sense or if not found.
        /// Very safe and robust, fully tested.
        /// </summary>
        /// <param name="value">The initial string. If null, will always return null or the specified fallback.</param>
        /// <param name="key">The key to find. If null, will always return null or the specified fallback</param>
        /// <param name="caseSensitive">Set to true if you need case-sensitive compare</param>
        /// <returns></returns>
        public static string BeforeLast(this string value, string key, bool caseSensitive = false)
            => value.BeforeInternal(key, caseSensitive ? Ordinal : InvariantCultureIgnoreCase, true);
        
        private static string BeforeInternal(this string value, string key, StringComparison comparison, bool findLast = false, string fallback = null)
        {
            if (value == null || key == null) return fallback;
            var posA = findLast ? value.LastIndexOf(key, comparison) : value.IndexOf(key, comparison);
            if (posA == -1) return fallback;
            return value.Substring(0, posA);
        }


        public static string Between(this string value, string before, string after, bool goToEndIfEndNotFound = false, bool caseSensitive = false)
        => value.BetweenInternal(before, after, goToEndIfEndNotFound, null, caseSensitive ? Ordinal : InvariantCultureIgnoreCase);


        /// <summary>
        /// Get string value between [first] a and [last] b.
        /// </summary>
        private static string BetweenInternal(
            this string value,
            string before, 
            string after,
            bool goToEndIfEndNotFound = false,
            string fallback = null,
            StringComparison comparison = InvariantCultureIgnoreCase
        )
        {
            if (value == null || before == null || after == null) return fallback;

            var trimStart = value.AfterInternal(before, comparison);
            if (trimStart == null) return fallback;
            var result = trimStart.BeforeInternal(after, comparison);
            if (result == null && goToEndIfEndNotFound) return trimStart;
            return result;
        }
    }
}

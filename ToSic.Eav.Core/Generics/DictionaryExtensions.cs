using System;
using System.Collections.Generic;

namespace ToSic.Eav.Generics
{
    // ReSharper disable once InconsistentNaming
    public static class DictionaryExtensions
    {
        public static IDictionary<string, T> ToInvariant<T>(this IDictionary<string, T> original)
        {
            // Bypass if it's already doing this - can only be checked on "real" dictionaries
            if (original is Dictionary<string, T> originalDic)
                return originalDic.ToInvariant();
            return new Dictionary<string, T>(original, StringComparer.InvariantCultureIgnoreCase);
        }

        public static Dictionary<string, T> ToInvariant<T>(this Dictionary<string, T> original)
        {
            // Bypass if it's already doing this
            if (Equals(original.Comparer, StringComparer.InvariantCultureIgnoreCase) || Equals(original.Comparer, StringComparer.OrdinalIgnoreCase))
                return original;
            return new Dictionary<string, T>(original, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}

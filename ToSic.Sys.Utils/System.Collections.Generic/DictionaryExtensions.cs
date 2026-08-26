// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

// Polyfill for .NET Framework, which doesn't have this method
// Added 2026-08-26 2dm
// Should stay in here till we drop .NET Framework support or till we can use .net standard 2.1 or similar

#if NETFRAMEWORK

public static class DictionaryExtensions
{
    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        /// <summary>
        /// Gets the value associated with the specified key, or the default value if the key is not present.
        /// </summary>
        public TValue? GetValueOrDefault(TKey key)
        {
            // Reads dictionary value safely with default fallback.
            return dictionary!.GetValueOrDefault(key, default);
        }

        /// <summary>
        /// Gets the value associated with the specified key, or the specified default value if the key is not present.
        /// </summary>
        public TValue GetValueOrDefault(TKey key, TValue defaultValue)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}

#endif
namespace ToSic.Sys.Utils.DictionaryExtensions;

internal static class DictionaryExtensionsTac
{
    extension<T>(IDictionary<string, T> original)
    {
        internal IEqualityComparer<string> GetComparerTac()
            => original.GetComparer();

        internal bool IsIgnoreCaseTac()
            => original.IsIgnoreCase();

        public IDictionary<string, T> FilterOutKeysTac(IEnumerable<string> keysToRemove)
            => original.FilterOutKeys(keysToRemove);
    }
}

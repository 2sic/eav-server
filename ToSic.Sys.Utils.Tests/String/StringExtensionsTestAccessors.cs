using ToSic.Sys.Utils;

namespace ToSic.Eav.Plumbing;

internal static class StringExtensionsTestAccessors
{
    internal static string AfterTac(this string v, string key, bool caseSensitive = false) =>
        v.After(key, caseSensitive);
    //internal static string TestBefore(this string v, string key, bool caseSensitive = false) =>
    //    v.Before(key, caseSensitive);
    internal static string BetweenTac(this string value, string before, string after,
        bool goToEndIfEndNotFound = false, bool caseSensitive = false)
        => value.Between(before, after, goToEndIfEndNotFound, caseSensitive);
}
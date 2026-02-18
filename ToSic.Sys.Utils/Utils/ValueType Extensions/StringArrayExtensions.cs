using System.Collections.Immutable;

namespace ToSic.Sys.Utils;

public static class StringArrayExtensions
{
    /// <summary>
    /// Convert a string array containing "key=value" pairs into an invariant immutable dictionary.
    /// Empty or null entries will be removed, and if there are no valid entries, the result will be null.
    /// </summary>
    /// <param name="valuePairs"></param>
    /// <returns></returns>
    public static ImmutableDictionary<string, string>? ValuePairsToDicImInv(this string[]? valuePairs, bool preferNullToEmpty = false)
    {
        var cleaned = valuePairs
                          ?.Where(v => v.HasValue())
                          .ToList()
                      ?? [];
        
        if (cleaned.SafeNone())
            return preferNullToEmpty
                ? null
                : new Dictionary<string, string>().ToImmutableDictionary();

        var valDic = cleaned
            .Select(v => v.Split('='))
            .Where(pair => pair.Length == 2)
            .ToImmutableDictionary(pair => pair[0], pair => pair[1], StringComparer.InvariantCultureIgnoreCase);

        return valDic.Count > 0 || !preferNullToEmpty
            ? valDic
            : null;
    }

}
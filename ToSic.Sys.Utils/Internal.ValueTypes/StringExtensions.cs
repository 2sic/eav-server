using System.Diagnostics.CodeAnalysis;
using static System.StringComparison;

namespace ToSic.Eav.Plumbing;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StringExtensions
{
    /// <summary>
    /// Check if a string is null or empty. Whitespace is treated as not-empty, use <see cref="IsEmptyOrWs"/> for that.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsEmpty(this string value) => string.IsNullOrEmpty(value);
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsEmptyOrWs(this string value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Has real value, so neither null, empty or white space.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool HasValue( /* [NotNullWhen(true)] // can't use when not .net 8 */ this string value)
        => !string.IsNullOrWhiteSpace(value);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string UseFallbackIfNoValue(this string value, string fallback)
        => !string.IsNullOrWhiteSpace(value) ? value : fallback;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string NullIfNoValue(this string value) => value.HasValue() ? value : null;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string[] SplitNewLine(this string value) 
        => value?.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

    /// <summary>
    /// Split a CSV string into an array of string. Empty entries are removed.
    /// Null-strings will return empty array;
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    // TODO: @2dm - changed a lot of places to use this 2024-01-23. If no errors appear, remove commented code in each location ca. 2024-Q2
    public static string[] CsvToArrayWithoutEmpty(this string original) => original?.Split(',').TrimmedAndWithoutEmpty() ?? [];

    public static string[] LinesToArrayWithoutEmpty(this string original) => original?.SplitNewLine().TrimmedAndWithoutEmpty() ?? [];


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string[] TrimmedAndWithoutEmpty(this string[] value) 
        => value?
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string NeverNull(this string value) => value ?? "";

    /// <summary>
    /// Null-safe string-equals method. If both are null, it's equal.
    /// </summary>
    /// <param name="a">The initial string, can also be null.</param>
    /// <param name="b">The compare string, can also be null</param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool EqualsInsensitive(this string a, string b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b, InvariantCultureIgnoreCase);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool ContainsInsensitive(this string a, string b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.IndexOf(b, 0, CurrentCultureIgnoreCase) != -1;
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string AsKey(this string value) 
        => value ?? NullKey;

    private const string NullKey = "\0";


    // https://stackoverflow.com/questions/2571716/find-nth-occurrence-of-a-character-in-a-string
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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

    /// <summary>
    /// Replace all characters in a string.
    /// https://stackoverflow.com/questions/7265315/replace-multiple-characters-in-a-c-sharp-string
    /// </summary>
    /// <param name="s"></param>
    /// <param name="separators"></param>
    /// <param name="newVal"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string RemoveAll(this string s, params char[] separators)
    {
        // 2dm: took
        var temp = s.Split(separators /*StringSplitOptions.RemoveEmptyEntries*/);
        return string.Join("", temp);
    }

}
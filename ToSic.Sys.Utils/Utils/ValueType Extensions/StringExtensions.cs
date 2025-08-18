﻿using static System.StringComparison;

namespace ToSic.Sys.Utils;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StringExtensions
{
    /// <summary>
    /// Check if a string is null or empty. Whitespace is treated as not-empty, use <see cref="IsEmptyOrWs"/> for that.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsEmpty([NotNullWhen(false)] this string? value)
        => string.IsNullOrEmpty(value);
    
    public static bool IsEmptyOrWs([NotNullWhen(false)] this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Has real value, so neither null, empty or white space.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool HasValue([NotNullWhen(true)] this string? value)
        => !string.IsNullOrWhiteSpace(value);

    // ReSharper disable once RedundantNullableFlowAttribute
    [return: NotNullIfNotNull(nameof(fallback))]
    public static string? UseFallbackIfNoValue(this string? value, string? fallback)
        => !string.IsNullOrWhiteSpace(value) ? value : fallback;

    public static string? NullIfNoValue(this string? value)
        => value.HasValue() ? value : null;

    [ShowApiWhenReleased(ShowApiMode.Never)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string[]? SplitNewLine(this string? value) 
        => value?.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

    /// <summary>
    /// Split a CSV string into an array of string. Empty entries are removed.
    /// Null-strings will return empty array;
    /// </summary>
    /// <param name="original">string containing csv. If null/empty will return []</param>
    /// <returns></returns>
    public static string[] CsvToArrayWithoutEmpty(this string? original)
        => original?.Split(',').TrimmedAndWithoutEmpty() ?? [];

    public static string[] LinesToArrayWithoutEmpty(this string? original)
        => original?.SplitNewLine().TrimmedAndWithoutEmpty() ?? [];


    [ShowApiWhenReleased(ShowApiMode.Never)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string[]? TrimmedAndWithoutEmpty(this string[]? value) 
        => value?
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string NeverNull(this string? value) => value ?? "";

    /// <summary>
    /// Null-safe string-equals method. If both are null, it's equal.
    /// </summary>
    /// <param name="a">The initial string, can also be null.</param>
    /// <param name="b">The compare string, can also be null</param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool EqualsInsensitive(this string? a, string? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b, InvariantCultureIgnoreCase);
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool ContainsInsensitive(this string? a, string? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.IndexOf(b, 0, CurrentCultureIgnoreCase) != -1;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string? Truncate(this string? value, int maxLength)
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
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string AsKey(this string? value) 
        => value ?? NullKey;

    private const string NullKey = "\0";


    // https://stackoverflow.com/questions/2571716/find-nth-occurrence-of-a-character-in-a-string
    [ShowApiWhenReleased(ShowApiMode.Never)]
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
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string RemoveAll(this string s, params char[] separators)
    {
        // 2dm: took
        var temp = s.Split(separators /*StringSplitOptions.RemoveEmptyEntries*/);
        return string.Join("", temp);
    }


    /// <summary>
    /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.
    /// This is to be used in .NET Framework or .netstandard 2.0 because .NET 5+ already has this string.Replace() method
    /// https://stackoverflow.com/a/36317315
    /// </summary>
    /// <param name="value">The string performing the replace method.</param>
    /// <param name="oldValue">The string to be replaced.</param>
    /// <param name="newValue">The string replace all occurrences of oldValue.</param>
    /// <param name="comparisonType">Type of the comparison.</param>
    /// <returns></returns>
    public static string ReplaceIgnoreCase(this string value, string oldValue, string newValue, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        oldValue ??= string.Empty;
        newValue ??= string.Empty;
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, comparisonType))
            return value;

        int foundAt;
        while ((foundAt = value.IndexOf(oldValue, 0, comparisonType)) != -1)
            value = value.Remove(foundAt, oldValue.Length).Insert(foundAt, newValue);

        return value;
    }

}
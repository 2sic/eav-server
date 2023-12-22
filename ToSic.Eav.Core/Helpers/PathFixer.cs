using System;

namespace ToSic.Eav.Helpers;

public static class PathFixer
{
    /// <summary>
    /// Convert all "/" characters to "\" characters - usually to change url-style paths to folder style paths
    /// </summary>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string Backslash(this string original)
        => original?.Replace("/", "\\");
    // Additional replace too risky, breaks network paths like \\srv-xyz\
    // .Replace("\\\\", "\\");


    /// <summary>
    /// Convert all "\" characters to "/" characters
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string ForwardSlash(this string original)
        => original?.Replace("\\", "/");
    // could break https:// links etc.
    // .Replace("//", "/").Replace("//", "/");

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string PrefixSlash(this string original)
    {
        if (original == null) return "/";
        if (original.StartsWith("/")) return original;
        if (original.StartsWith("\\")) original = original.TrimStart('\\');
        return "/" + original;
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string SuffixSlash(this string original)
    {
        if (original == null) return "/";
        if (original.EndsWith("/")) return original;
        if (original.EndsWith("\\")) original = original.TrimEnd('\\');
        return original + "/";
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string TrimLastSlash(this string original)
        => original?.TrimEnd('/').TrimEnd('\\');

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string TrimPrefixSlash(this string original)
        => original?.TrimStart('/').TrimStart('\\');

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string FlattenMultipleForwardSlashes(this string path) 
        => path.Replace("//", "/").Replace("//", "/").Replace("//", "/");

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string ToAbsolutePathForwardSlash(this string path)
    {
        if (path.Contains(":"))
            throw new ArgumentException("Path to clean cannot have ':' characters", nameof(path));
        // remove base character which often has a ~ to mark site root
        path = path.TrimStart('~');
        return path.ForwardSlash().PrefixSlash().FlattenMultipleForwardSlashes();
    }
}
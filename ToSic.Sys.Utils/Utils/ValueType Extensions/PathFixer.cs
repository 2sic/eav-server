using System.Text.RegularExpressions;

namespace ToSic.Sys.Utils;

public static class PathFixer
{
    public const string PathTraversal = "..";
    public const string PathTraversalMayNotContainMessage = "may not contain '..' or '../' or similar path traversal.";

    // Check if a path contains a path traversal
    public static bool ContainsPathTraversal(this string? path)
        => path?.Contains(PathTraversal) == true;

    /// <summary>
    /// Convert all "/" characters to "\" characters - usually to change url-style paths to folder style paths
    /// </summary>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string? Backslash(this string? original)
        => original?.Replace("/", "\\");
    // Additional replace too risky, breaks network paths like \\srv-xyz\
    // .Replace("\\\\", "\\");


    /// <summary>
    /// Convert all "\" characters to "/" characters
    /// </summary>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    [return: NotNullIfNotNull(nameof(original))]
    public static string? ForwardSlash(this string? original)
        => original?.Replace("\\", "/");
    // could break https:// links etc.
    // .Replace("//", "/").Replace("//", "/");

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string PrefixSlash(this string? original)
    {
        if (original == null)
            return "/";
        if (original.StartsWith("/"))
            return original;
        if (original.StartsWith("\\"))
            original = original.TrimStart('\\');
        return "/" + original;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string SuffixSlash(this string? original)
    {
        if (original == null)
            return "/";
        if (original.EndsWith("/"))
            return original;
        if (original.EndsWith("\\"))
            original = original.TrimEnd('\\');
        return original + "/";
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string? TrimLastSlash(this string? original)
        => original?.TrimEnd('/').TrimEnd('\\');

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string? TrimPrefixSlash(this string? original)
        => original?.TrimStart('/').TrimStart('\\');

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string FlattenMultipleForwardSlashes(this string path) 
        => path.Replace("//", "/").Replace("//", "/").Replace("//", "/");

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string FlattenMultipleBackslashes(this string path)
        => Regex.Replace(path, "\\+", "\\");

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string FlattenSlashes(this string path)
        => path.FlattenMultipleForwardSlashes().FlattenMultipleBackslashes();

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAbsolutePathForwardSlash(this string path)
    {
        if (path.Contains(":"))
            throw new ArgumentException("Path to clean cannot have ':' characters", nameof(path));
        // remove base character which often has a ~ to mark site root
        path = path.TrimStart('~');
        return path.ForwardSlash().PrefixSlash().FlattenMultipleForwardSlashes();
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsPathWithDrive(this string path)
        => Regex.IsMatch(path, @"^[a-zA-Z]:\\");

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsPathWithDriveOrNetwork(this string path)
        => Regex.IsMatch(path, @"^\\\\|^[a-zA-Z]:\\");
}
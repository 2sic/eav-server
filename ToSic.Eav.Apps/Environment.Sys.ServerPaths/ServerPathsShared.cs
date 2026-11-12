using ToSic.Eav.Data.Sys.ValueConverter;

namespace ToSic.Eav.Environment.Sys.ServerPaths;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class ServerPathsShared
{
    [return: NotNullIfNotNull(nameof(fileReference))]
    public static string? FullPathOfReference(string? fileReference, Func<int, string?> fullPathOfReference)
    {
        if (string.IsNullOrWhiteSpace(fileReference))
            return fileReference;

        var parts = new LinkParts(fileReference!);
        if (parts.IsPage)
            return fileReference;

        return fullPathOfReference(parts.Id)
            ?? fileReference;
    }
}
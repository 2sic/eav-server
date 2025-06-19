namespace ToSic.Sys.Utils;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ExtensionToListOfOne
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static List<T>? ToListOfOneOrNull<T>(this T? original)
        => original == null
            ? null
            : [original];

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static List<T> ToListOfOneOrNone<T>(this T? original)
        => original == null
            ? []
            : [original];
}
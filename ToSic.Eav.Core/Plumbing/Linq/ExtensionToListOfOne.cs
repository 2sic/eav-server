namespace ToSic.Eav.Plumbing;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ExtensionToListOfOne
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static List<T> ToListOfOne<T>(this T original) => [original];

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static List<T> ToListOfOneOrNone<T>(this T original) => original == null ? [] : [original];
}
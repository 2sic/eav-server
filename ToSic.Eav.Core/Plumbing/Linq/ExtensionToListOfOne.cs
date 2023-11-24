using System.Collections.Generic;

namespace ToSic.Eav.Plumbing;

public static class ExtensionToListOfOne
{
    public static List<T> ToListOfOne<T>(this T original) => new() { original };
}
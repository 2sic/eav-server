using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.LookUp;

public static class LookUpExtensions
{
    public static bool HasSource(this IReadOnlyCollection<ILookUp> list, string name) 
        => list?.Any(s => s.Name.EqualsInsensitive(name)) == true;

    public static ILookUp GetSource(this IReadOnlyCollection<ILookUp> list, string name)
        => list?.FirstOrDefault(s => s.Name.EqualsInsensitive(name));
}
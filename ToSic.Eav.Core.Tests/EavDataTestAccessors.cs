using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Core.Tests;

public static class EavDataTestAccessors
{
    public static IEnumerable<IValue> TacValues(this IAttribute attribute)
        => attribute.Values;
}
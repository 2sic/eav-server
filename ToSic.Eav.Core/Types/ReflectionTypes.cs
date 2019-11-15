using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.Types
{
    public class ReflectionTypes
    {
        public static readonly ImmutableDictionary<string, IContentType> FakeCache =
            new Dictionary<string, IContentType>()
                .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }
}

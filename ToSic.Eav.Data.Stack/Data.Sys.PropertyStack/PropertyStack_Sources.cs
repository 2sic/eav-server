﻿using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data.PropertyStack.Sys;

public partial class PropertyStack
{
    public IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources
        => _sources ?? throw new($"Can't access {nameof(IPropertyStack)}.{nameof(Sources)} as it hasn't been initialized yet.");
    private IImmutableList<KeyValuePair<string, IPropertyLookup>>? _sources;

    public IImmutableList<KeyValuePair<string, IPropertyLookup>> SourcesReal => _sourcesReal.Get(GeneratorSourcesReal)!;
    private readonly GetOnce<IImmutableList<KeyValuePair<string, IPropertyLookup>>> _sourcesReal = new();

    private IImmutableList<KeyValuePair<string, IPropertyLookup>> GeneratorSourcesReal()
    {
        var real = Sources
            .Where(ep => ep.Value != null! /* paranoid */)
            // Must de-duplicate sources. EG AppSystem and AppAncestorSystem could be the same entity
            // And in that case future lookups could result in endless loops
            .DistinctBy(src => src.Value)
            .ToImmutableOpt();
        return real!;
    }


    public IPropertyLookup? GetSource(string name)
    {
        var found = Sources
            .Where(s => s.Key.EqualsInsensitive(name))
            .ToListOpt();
        return found.Any() ? found[0].Value : null;
    }
}
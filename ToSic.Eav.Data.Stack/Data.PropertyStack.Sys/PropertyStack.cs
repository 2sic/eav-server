﻿using ToSic.Eav.Data.Sys;
using ToSic.Eav.Sys;
using ToSic.Lib.Data;

namespace ToSic.Eav.Data.PropertyStack.Sys;

[PrivateApi("Hide implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class PropertyStack: IPropertyStack, IHasIdentityNameId
{
    public PropertyStack Init(string name, IEnumerable<IPropertyLookup> sources)
        => Init(name, sources
            .Select(s => new KeyValuePair<string, IPropertyLookup>((s as IHasIdentityNameId)?.NameId ?? "unknown", s))
            .Where(kvp => kvp.Key != null! && kvp.Value != null!)
            .ToArray()
        );

    public PropertyStack Init(string name, IReadOnlyCollection<KeyValuePair<string, IPropertyLookup>> sources)
        => Init(name, sources.ToArray());

    public PropertyStack Init(string name, params KeyValuePair<string, IPropertyLookup>[] sources)
    {
        NameId = name;
        var pairCount = 0;

        _sources = sources
            .Select(selector: ep =>
            {
                var key = !string.IsNullOrWhiteSpace(ep.Key)
                    ? ep.Key
                    : $"auto-named-{++pairCount}";
                return new KeyValuePair<string, IPropertyLookup?>(key, ep.Value);
            })
            .ToImmutableOpt();

        return this;
    }

    public string NameId { get; private set; } = "awaiting-init";


    public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
        => GetNextInStack(specs, 0, path);

    public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path)
    {
        specs = specs.SubLog(EavLogs.Eav + ".PStack");
        var l = specs.LogOrNull.Fn<PropReqResult>($"{specs.Dump()}, {nameof(startAtSource)}: {startAtSource}");
        // Start with empty result, may be filled in later on
        var result = PropReqResult.Null(path);
        for (var sourceIndex = startAtSource; sourceIndex < SourcesReal.Count; sourceIndex++)
        {
            var source = SourcesReal[sourceIndex];
            l.A($"Testing source #{sourceIndex} : {source.Key}");

            path = path.Add($"PropertyStack[{sourceIndex}]", source.Key, specs.Field);
            var propInfo = source.Value.FindPropertyInternal(specs, path);
            if (propInfo?.Result == null)
                continue;

            result = propInfo.MarkAsFinalOrNot(source.Key, sourceIndex, specs.LogOrNull, specs.TreatEmptyAsDefault);
            if (result.IsFinal != true)
                continue;

            var parentRefForNewChildren = new StackAddress(this, specs.Field, sourceIndex, null);
            var wrapper = new StackReWrapper(parentRefForNewChildren, specs.LogOrNull);
            result = wrapper.ReWrapIfPossible(result);
            return l.Return(result,
                result.ResultOriginal == null ? "simple value, final" : "re-wrapped with stack nav");
        }

        // All loops completed, maybe one got a temporary result, return that
        return l.Return(result, "not-final");
    }

    // #DropUseOfDumpProperties
    //[PrivateApi("Internal")]
    //public List<PropertyDumpItem> _DumpNameWipDroppingMostCases(PropReqSpecs specs, string path)
    //    => new PropertyStackDump().Dump(this, specs, path, null);
}
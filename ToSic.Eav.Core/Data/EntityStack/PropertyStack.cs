using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    [PrivateApi("Hide implementation")]
    public partial class PropertyStack: IPropertyStack, IHasIdentityNameId
    {
        public PropertyStack Init(string name, IEnumerable<IPropertyLookup> sources)
            => Init(name,
                sources?.Select(s => new KeyValuePair<string, IPropertyLookup>((s as IHasIdentityNameId)?.NameId, s)).ToArray()
                ?? Array.Empty<KeyValuePair<string, IPropertyLookup>>());

        public PropertyStack Init(string name, IReadOnlyCollection<KeyValuePair<string, IPropertyLookup>> sources)
            => Init(name, sources.ToArray());

        public PropertyStack Init(string name, params KeyValuePair<string, IPropertyLookup>[] sources)
        {
            NameId = name;
            var pairCount = 0;

            _sources = sources
                .Select(selector: ep =>
                {
                    var key = !string.IsNullOrWhiteSpace(ep.Key) ? ep.Key : $"auto-named-{++pairCount}";
                    return new KeyValuePair<string, IPropertyLookup>(key, ep.Value);
                })
                .ToImmutableArray();

            return this;
        }

        public string NameId { get; private set; }


        public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
            => GetNextInStack(specs, 0, path);

        public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path)
        {
            specs = specs.SubLog(LogNames.Eav + ".PStack");
            var l = specs.LogOrNull.Fn<PropReqResult>($"{specs.Dump()}, {nameof(startAtSource)}: {startAtSource}");
            // Start with empty result, may be filled in later on
            var result = PropReqResult.Null(path);
            for (var sourceIndex = startAtSource; sourceIndex < SourcesReal.Count; sourceIndex++)
            {
                var source = SourcesReal[sourceIndex];
                l.A($"Testing source #{sourceIndex} : {source.Key}");

                path = path.Add($"PropertyStack[{sourceIndex}]", source.Key, specs.Field);
                var propInfo = source.Value.FindPropertyInternal(specs, path);
                if (propInfo?.Result == null) continue;

                result = propInfo.MarkAsFinalOrNot(source.Key, sourceIndex, specs.LogOrNull, specs.TreatEmptyAsDefault);
                if (!result.IsFinal) continue;

                var parentRefForNewChildren = new StackAddress(this, specs.Field, sourceIndex, null);
                var wrapper = new StackReWrapper(parentRefForNewChildren, specs.LogOrNull);
                result = wrapper.ReWrapIfPossible(result);
                return l.Return(result,
                    result.ResultOriginal == null ? "simple value, final" : "re-wrapped with stack nav");
            }

            // All loops completed, maybe one got a temporary result, return that
            return l.Return(result, "not-final");
        }
        
    }
}

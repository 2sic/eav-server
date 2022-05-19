using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Plumbing.Linq;

namespace ToSic.Eav.Data
{
    [PrivateApi("Hide implementation")]
    public partial class PropertyStack: IPropertyStack
    {
        public void Init(string name, params KeyValuePair<string, IPropertyLookup>[] entities)
        {
            Name = name;
            var pairCount = 0;

            _sources = entities
                .Select(selector: ep =>
                {
                    var key = !string.IsNullOrWhiteSpace(ep.Key) ? ep.Key : $"auto-named-{++pairCount}";
                    return new KeyValuePair<string, IPropertyLookup>(key, ep.Value);
                })
                .ToImmutableArray();
        }

        public string Name { get; private set; }

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources 
            => _sources ?? throw new Exception($"Can't access {nameof(IPropertyStack)}.{nameof(Sources)} as it hasn't been initialized yet.");
        private IImmutableList<KeyValuePair<string, IPropertyLookup>> _sources;

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> SourcesReal => _sourcesReal.Get(GeneratorSourcesReal);
        private readonly ValueGetOnce<IImmutableList<KeyValuePair<string, IPropertyLookup>>> _sourcesReal = new ValueGetOnce<IImmutableList<KeyValuePair<string, IPropertyLookup>>>();

        private IImmutableList<KeyValuePair<string, IPropertyLookup>> GeneratorSourcesReal()
        {
            var real = _sources.Where(ep => ep.Value != null)
                // Must de-duplicate sources. EG AppSystem and AppAncestorSystem could be the same entity
                // And in that case future lookups could result in endless loops
                .DistinctBy(src => src.Value)
                .ToImmutableArray();
            return real;
        }


        public IPropertyLookup GetSource(string name)
        {
            var found = Sources.Where(s => s.Key.EqualsInsensitive(name)).ToArray();
            return found.Any() ? found[0].Value : null;
        }

        public IPropertyStack GetStack(params string[] names) => GetStack(null, names);

        public IPropertyStack GetStack(ILog log, params string[] names)
        {
            var wrapLog = log.Call2<IPropertyStack>();
            // Get all required names in the order they were requested
            var newSources = new List<KeyValuePair<string, IPropertyLookup>>();
            foreach (var name in names)
            {
                var s = GetSource(name);
                wrapLog.A($"Add stack {name}, found: {s != null}");
                if (s != null) newSources.Add(new KeyValuePair<string, IPropertyLookup>(name, s));
            }

            var newStack = new PropertyStack();
            newStack.Init("New", newSources.ToArray());
            return wrapLog.Return(newStack, newSources.Count.ToString());
        }

        [PrivateApi("Internal")]
        public PropertyRequest FindPropertyInternal(string field, string[] dimensions, ILog parentLogOrNull, PropertyLookupPath path)
            => PropertyInStack(field, dimensions, 0, true, parentLogOrNull, path);

        public PropertyRequest PropertyInStack(string field, string[] dimensions, int startAtSource, bool treatEmptyAsDefault, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".PStack");
            var wrapLog = logOrNull.Call2<PropertyRequest>($"{nameof(field)}: {field}, {nameof(startAtSource)}: {startAtSource}");
            // Start with empty result, may be filled in later on
            var result = new PropertyRequest();
            for (var sourceIndex = startAtSource; sourceIndex < SourcesReal.Count; sourceIndex++)
            {
                var source = SourcesReal[sourceIndex];
                wrapLog.A($"Testing source #{sourceIndex} : {source.Key}");

                path = path.Add($"PropertyStack[{sourceIndex}]", source.Key, field);
                var propInfo = source.Value.FindPropertyInternal(field, dimensions, logOrNull, path);
                if (propInfo?.Result == null) continue;

                result = propInfo.MarkAsFinalOrNot(source.Key, sourceIndex, logOrNull, treatEmptyAsDefault);
                if (!result.IsFinal) continue;
                
                if (!(result.Result is IEnumerable<IEntity> entityChildren))
                    return wrapLog.Return(result, "simple value, final");

                var navigationWrapped = entityChildren.Select(e =>
                    new EntityWithStackNavigation(e, this, field, result.SourceIndex, 0)).ToList();
                result.Result = navigationWrapped;

                return wrapLog.Return(result, "wrapped as Entity-Stack, final");
            }

            // All loops completed, maybe one got a temporary result, return that
            return wrapLog.Return(result, "not-final");
        }
        
    }
}

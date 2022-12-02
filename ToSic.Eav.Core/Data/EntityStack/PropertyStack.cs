using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Plumbing.Linq;

namespace ToSic.Eav.Data
{
    [PrivateApi("Hide implementation")]
    public partial class PropertyStack: IPropertyStack
    {
        public PropertyStack Init(string name, IEnumerable<IPropertyLookup> sources)
            => Init(name,
                sources?.Select(s => new KeyValuePair<string, IPropertyLookup>((s as IHasIdentityNameId)?.NameId, s)).ToArray()
                ?? Array.Empty<KeyValuePair<string, IPropertyLookup>>());

        public PropertyStack Init(string name, params KeyValuePair<string, IPropertyLookup>[] sources)
        {
            Name = name;
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

        public string Name { get; private set; }

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources 
            => _sources ?? throw new Exception($"Can't access {nameof(IPropertyStack)}.{nameof(Sources)} as it hasn't been initialized yet.");
        private IImmutableList<KeyValuePair<string, IPropertyLookup>> _sources;

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> SourcesReal => _sourcesReal.Get(GeneratorSourcesReal);
        private readonly GetOnce<IImmutableList<KeyValuePair<string, IPropertyLookup>>> _sourcesReal = new GetOnce<IImmutableList<KeyValuePair<string, IPropertyLookup>>>();

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
            var wrapLog = log.Fn<IPropertyStack>();
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
            var wrapLog = logOrNull.Fn<PropertyRequest>($"{nameof(field)}: {field}, {nameof(startAtSource)}: {startAtSource}");
            // Start with empty result, may be filled in later on
            var result = new PropertyRequest(null, path);
            for (var sourceIndex = startAtSource; sourceIndex < SourcesReal.Count; sourceIndex++)
            {
                var source = SourcesReal[sourceIndex];
                wrapLog.A($"Testing source #{sourceIndex} : {source.Key}");

                path = path.Add($"PropertyStack[{sourceIndex}]", source.Key, field);
                var propInfo = source.Value.FindPropertyInternal(field, dimensions, logOrNull, path);
                if (propInfo?.Result == null) continue;

                result = propInfo.MarkAsFinalOrNot(source.Key, sourceIndex, logOrNull, treatEmptyAsDefault);
                if (!result.IsFinal) continue;

                // Note 2022-12-01 2dm - not sure if this is actually hit, or if it's handled at another level...?
                if (result.Result is IEnumerable<IEntity> entityChildren)
                {
                    var navigationWrapped = entityChildren.Select(e =>
                        new EntityWithStackNavigation(e, this, field, result.SourceIndex, 0)).ToList();
                    result.Result = navigationWrapped;

                    return wrapLog.Return(result, "wrapped as Entity-Stack, final");
                }

                // 2022-12-01 2dm - new type of result, mainly for testing ATM, shouldn't happen in production
                // It seems to be necessary as soon as the result is a field-property list
                // But the parent seems off...
                // I'm not using IEnumerable<IPropertyLookup> because that could have untested side-effects
                if (result.Result is IEnumerable<PropertyLookupDictionary> dicChildren)
                {
                    // First construct the parent navigator, which the children need first
                    if (!(result.Source is PropertyLookupDictionary currentSource))
                        throw new Exception("Test 2dm- this should always be a dictionary");

                    //var parentNavigator = new PropertyLookupWithStackNavigation(currentSource, this, field, result.SourceIndex, 0);

                    var navWrapped = dicChildren.Select(e =>
                        new PropertyLookupWithStackNavigation(e, this, field, result.SourceIndex, 0)).ToList();
                    result.Result = navWrapped;
                    return wrapLog.Return(result, "wrapped as dictionary prop stack, final");
                }

                return wrapLog.Return(result, "simple value, final");
            }

            // All loops completed, maybe one got a temporary result, return that
            return wrapLog.Return(result, "not-final");
        }
        
    }
}

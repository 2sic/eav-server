﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

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

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> SourcesReal 
            => _sourcesReal ?? (_sourcesReal = _sources.Where(ep => ep.Value != null).ToImmutableArray());

        private IImmutableList<KeyValuePair<string, IPropertyLookup>> _sourcesReal;

        public IPropertyLookup GetSource(string name)
        {
            var found = Sources.Where(s => s.Key.EqualsInsensitive(name)).ToArray();
            return found.Any() ? found[0].Value : null;
        }

        public IPropertyStack GetStack(params string[] names) => GetStack(null, names);

        public IPropertyStack GetStack(ILog log, params string[] names)
        {
            var wrapLog = log.SafeCall<IPropertyStack>();
            // Get all required names in the order they were requested
            var newSources = new List<KeyValuePair<string, IPropertyLookup>>();
            foreach (var name in names)
            {
                var s = GetSource(name);
                log.SafeAdd($"Add stack {name}, found: {s != null}");
                if (s != null) newSources.Add(new KeyValuePair<string, IPropertyLookup>(name, s));
            }

            var newStack = new PropertyStack();
            newStack.Init("New", newSources.ToArray());
            return wrapLog(newSources.Count.ToString(), newStack);
        }

        [PrivateApi("Internal")]
        public PropertyRequest FindPropertyInternal(string field, string[] dimensions, ILog parentLogOrNull, PropertyLookupPath path)
            => PropertyInStack(field, dimensions, 0, true, parentLogOrNull, path);

        public PropertyRequest PropertyInStack(string field, string[] dimensions, int startAtSource, bool treatEmptyAsDefault, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".PStack");
            var wrapLog = logOrNull.SafeCall<PropertyRequest>($"{nameof(field)}: {field}, {nameof(startAtSource)}: {startAtSource}");
            // Start with empty result, may be filled in later on
            var result = new PropertyRequest();
            for (var sourceIndex = startAtSource; sourceIndex < SourcesReal.Count; sourceIndex++)
            {
                var source = SourcesReal[sourceIndex];
                logOrNull.SafeAdd($"Testing source #{sourceIndex} : {source.Key}");

                path = path.Add("PropStack", source.Key, field);
                var propInfo = source.Value.FindPropertyInternal(field, dimensions, logOrNull, path);
                if (propInfo?.Result == null) continue;

                result = propInfo.MarkAsFinalOrNot(source.Key, sourceIndex, logOrNull, treatEmptyAsDefault);
                if (!result.IsFinal) continue;
                
                if (!(result.Result is IEnumerable<IEntity> entityChildren))
                    return wrapLog("simple value, final", result);

                var navigationWrapped = entityChildren.Select(e =>
                    new EntityWithStackNavigation(e, this, field, result.SourceIndex)).ToList();
                result.Result = navigationWrapped;

                return wrapLog("wrapped as Entity-Stack, final", result);
            }

            // All loops completed, maybe one got a temporary result, return that
            return wrapLog("not-final", result);
        }
        
    }
}

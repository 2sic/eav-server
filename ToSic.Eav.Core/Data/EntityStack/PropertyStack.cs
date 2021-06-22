using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data
{
    [PrivateApi("Still WIP")]
    public class PropertyStack: IPropertyStack
    {
        public void Init(params KeyValuePair<string, IPropertyLookup>[] entities)
        {
            var pairCount = 0;

            _sources = entities
                .Select(selector: ep =>
                {
                    var key = !string.IsNullOrWhiteSpace(ep.Key) ? ep.Key : $"auto-named-{++pairCount}";
                    return new KeyValuePair<string, IPropertyLookup>(key, ep.Value);
                })
                .ToImmutableArray();
        }

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources => _sources ?? throw new Exception($"Can't access {nameof(IPropertyStack)}.{nameof(Sources)} as it hasn't been initialized yet.");
        private IImmutableList<KeyValuePair<string, IPropertyLookup>> _sources;

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> SourcesReal => _sourcesReal ?? (_sourcesReal = _sources.Where(ep => ep.Value != null).ToImmutableArray());
        private IImmutableList<KeyValuePair<string, IPropertyLookup>> _sourcesReal;

        public IPropertyLookup GetSource(string name)
        {
            var found = Sources.Where(s => s.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return found.Any() ? found[0].Value : null;
        }

        [PrivateApi("Internal")]
        public PropertyRequest FindPropertyInternal(string field, string[] dimensions, ILog parentLogOrNull)
            => PropertyInStack(field, dimensions, 0, true, parentLogOrNull);

        public PropertyRequest PropertyInStack(string field, string[] dimensions, int startAtSource, bool treatEmptyAsDefault, ILog parentLogOrNull)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".PStack");
            var wrapLog = logOrNull.SafeCall<PropertyRequest>($"{nameof(field)}: {field}, {nameof(startAtSource)}: {startAtSource}");
            // Start with empty result, may be filled in later on
            var result = new PropertyRequest();
            for (var sourceIndex = startAtSource; sourceIndex < SourcesReal.Count; sourceIndex++)
            {
                var source = SourcesReal[sourceIndex];
                logOrNull.SafeAdd($"Testing source #{sourceIndex} : {source.Key}");

                var propInfo = source.Value.FindPropertyInternal(field, dimensions, logOrNull);
                
                if (propInfo?.Result == null) continue;

                result = MarkAsFinalOrNot(propInfo, source.Key, sourceIndex, logOrNull, treatEmptyAsDefault);
                
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

        /// <summary>
        /// Review the result and mark as final if it is final.
        /// Also optionally log the decision process. 
        /// </summary>
        /// <returns></returns>
        public static PropertyRequest MarkAsFinalOrNot(PropertyRequest propInfo, string sourceName, int sourceIndex, ILog logOrNull, bool treatEmptyAsDefault)
        {
            var safeWrap = logOrNull.SafeCall<PropertyRequest>();
            // Check nulls and prevent multiple executions
            if (propInfo == null || propInfo.IsFinal) 
                return safeWrap("null or final", propInfo);
            
            propInfo.Name = sourceName;

            // Preserve, in case we won't find another but don't necessarily return now
            var result = propInfo;

            // if any non-null is ok, use that.
            if (!treatEmptyAsDefault)
                return safeWrap("empty is ok", result.AsFinal(sourceIndex));

            // this may set a null, but may also set an empty string or empty array
            if (result.Result.IsNullOrDefault(treatFalseAsDefault: false)) 
                return safeWrap("NullOrDefault - not final", result);

            if (result.Result is string foundString)
                return string.IsNullOrEmpty(foundString)
                    ? safeWrap("empty string, not final", result)
                    : safeWrap("string, non-empty - final", result.AsFinal(sourceIndex));

            // Return entity-list if it has elements, otherwise continue searching
            if (result.Result is IEnumerable<IEntity> entityList)
                return !entityList.Any()
                    ? safeWrap("empty list, not final", result)
                    : safeWrap("list, non empty, final", result.AsFinal(sourceIndex));

            // not sure if this will ever hit
            if (result.Result is ICollection list)
                return list.Count == 0
                    ? safeWrap("empty collection, not final", result)
                    : safeWrap("list, non-empty, final",  result.AsFinal(sourceIndex));

            // All seems ok, special checks passed, return result
            return safeWrap("all ok/final", result.AsFinal(sourceIndex));
        }
    }
}

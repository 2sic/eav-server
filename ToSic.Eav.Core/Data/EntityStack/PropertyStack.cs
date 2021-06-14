﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Documentation;
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
                .Where(ep => ep.Value != null)
                .Select(selector: ep =>
                {
                    var key = !string.IsNullOrWhiteSpace(ep.Key) ? ep.Key : $"auto-named-{++pairCount}";
                    return new KeyValuePair<string, IPropertyLookup>(key, ep.Value);
                })
                .ToImmutableArray();
        }

        public IImmutableList<KeyValuePair<string, IPropertyLookup>> Sources => _sources ?? throw new Exception($"Can't access {nameof(IPropertyStack)}.{nameof(Sources)} as it hasn't been initialized yet.");
        private IImmutableList<KeyValuePair<string, IPropertyLookup>> _sources;

        public IPropertyLookup GetSource(string name)
        {
            var found = Sources.Where(s => s.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return found.Any() ? found[0].Value : null;
        }

        [PrivateApi("Internal")]
        public PropertyRequest FindPropertyInternal(string fieldName, string[] dimensions)
            => PropertyInStack(fieldName, dimensions, 0, true);

        public PropertyRequest PropertyInStack(string fieldName, string[] dimensions, int startAtSource, bool treatEmptyAsDefault)
        {
            // Start with empty result, may be filled in later on
            var result = new PropertyRequest();
            for (var sourceIndex = startAtSource; sourceIndex < Sources.Count; sourceIndex++)
            {
                var source = Sources[sourceIndex];

                var propInfo = source.Value.FindPropertyInternal(fieldName, dimensions);
                
                if (propInfo?.Result == null) continue;

                result = MarkAsFinalOrNot(propInfo, source.Key, sourceIndex, treatEmptyAsDefault);
                if (result.IsFinal) return result;
            }

            // All loops completed, maybe one got a temporary result, return that
            return result;
        }

        public static PropertyRequest MarkAsFinalOrNot(PropertyRequest propInfo, string sourceName, int sourceIndex, bool treatEmptyAsDefault = true)
        {
            // Check nulls and prevent multiple executions
            if (propInfo == null || propInfo.IsFinal) return propInfo;
            
            propInfo.Name = sourceName;

            // Preserve, in case we won't find another but don't necessarily return now
            var result = propInfo;

            // if any non-null is ok, use that.
            if (!treatEmptyAsDefault) return result.AsFinal(sourceIndex);

            // this may set a null, but may also set an empty string or empty array
            if (result.Result.IsNullOrDefault(treatFalseAsDefault: false)) return result;

            if (result.Result is string foundString)
            {
                if (string.IsNullOrEmpty(foundString)) return result;
                return result.AsFinal(sourceIndex);
            }

            // Return entity-list if it has elements, otherwise continue searching
            if (result.Result is IEnumerable<IEntity> entityList)
            {
                if (!entityList.Any()) return result;
                return result.AsFinal(sourceIndex);
            }

            // not sure if this will ever hit
            if (result.Result is ICollection list)
            {
                if (list.Count == 0) return result;
                return result.AsFinal(sourceIndex);
            }

            // All seems ok, special checks passed, return result
            return result.AsFinal(sourceIndex);
        }
    }
}

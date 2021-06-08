using System;
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
            => FindPropertyInternal(fieldName, dimensions, true);

        public PropertyRequest FindPropertyInternal(string fieldName, string[] dimensions, bool treatEmptyAsDefault)
        {
            // Start with empty result, may be filled in later on
            var result = new PropertyRequest();
            foreach (var stackItem in Sources)
            {
                // Check if the entity even has this field
                // if (!stackItem.Value.Attributes.ContainsKey(fieldName)) continue;

                var propInfo = stackItem.Value.FindPropertyInternal(fieldName, dimensions);
                if (propInfo?.Result == null) continue;
                propInfo.Name = stackItem.Key;
                
                // Preserve, in case we won't find another but don't necessarily return now
                result = propInfo;

                // if any non-null is ok, use that.
                if (!treatEmptyAsDefault) return result;

                // this may set a null, but may also set an empty string or empty array
                if (result.Result.IsNullOrDefault(treatFalseAsDefault: false)) continue;

                if (result.Result is string foundString)
                {
                    if(string.IsNullOrEmpty(foundString)) continue;
                    return result;
                }

                // Return entity-list if it has elements, otherwise continue searching
                if (result.Result is IEnumerable<IEntity> entityList)
                {
                    if(!entityList.Any()) continue;
                    return result;
                }
                
                // not sure if this will ever hit
                if (result.Result is ICollection list)
                {
                    if(list.Count == 0) continue;
                    return result;
                }
                
                // All seems ok, special checks passed, return result
                return result;
            }

            // All loops completed, maybe one got a temporary result, return that
            return result;
        }
    }
}

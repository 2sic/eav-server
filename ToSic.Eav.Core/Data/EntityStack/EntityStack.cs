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
    public class EntityStack: IEntityStack
    {
        public void Init(string[] languages, params IEntity[] entities)
        {
            Languages = languages;
            _stack = entities.Where(e => e != null).ToImmutableArray();
        }

        public string[] Languages = {};

        public IImmutableList<IEntity> Stack => _stack ?? throw new Exception($"Can't access {nameof(IEntityStack)}.{nameof(Stack)} as it hasn't been initialized yet.");
        private IImmutableList<IEntity> _stack;

        public object Value(string fieldName, bool treatEmptyAsDefault = true)
        {
            object result = null;
            foreach (var entity in Stack)
            {
                // Check if the entity even has this field
                if (!entity.Attributes.ContainsKey(fieldName)) continue;

                var found = entity.GetBestValue(fieldName, Languages);
                if(found == null) continue;
                
                // if any non-null is ok, use that.
                if (!treatEmptyAsDefault) return found;
                
                // Preserve, in case we won't find another but don't necessarily return now
                // this may set a null, but may also set an empty string or empty array
                result = found;
                if (found.IsNullOrDefault()) continue;
                
                if (found is string foundString && !string.IsNullOrEmpty(foundString)) return found;

                // if (found is ICollection list && list.Count != 0) return found;
                return found;
            }

            return result;
        }

        public T Value<T>(string fieldName) => Value<T>(fieldName, true);

        public T Value<T>(string fieldName, bool treatEmptyAsDefault)
        {
            foreach (var entity in Stack)
            {
                // Check if the entity even has this field
                if (!entity.Attributes.ContainsKey(fieldName)) continue;
                
                var found = entity.GetBestValue<T>(fieldName, Languages);
                if (EqualityComparer<T>.Default.Equals(found)) continue;

                // if any non-null is ok, use that.
                if (!treatEmptyAsDefault) return found;
                
                if (found is string foundString && !string.IsNullOrEmpty(foundString)) return found;

                if (found is ICollection list && list.Count != 0) return found;
                    return found;
            }

            return default;
        }
    }
}

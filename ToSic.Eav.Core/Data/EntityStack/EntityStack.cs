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
        public void Init(params IEntity[] entities)
        {
            _stack = entities.Where(e => e != null).ToImmutableArray();
        }

        public IImmutableList<IEntity> Stack => _stack ?? throw new Exception($"Can't access {nameof(IEntityStack)}.{nameof(Stack)} as it hasn't been initialized yet.");
        private IImmutableList<IEntity> _stack;


        public Tuple<object, string, IEntity> ValueAndMore(string fieldName, string[] dimensions, bool treatEmptyAsDefault = true)
        {
            var result = new Tuple<object, string, IEntity>(null, null, null);
            foreach (var entity in Stack)
            {
                // Check if the entity even has this field
                if (!entity.Attributes.ContainsKey(fieldName)) continue;

                var foundSet = entity.ValueAndType(fieldName, dimensions);
                if (foundSet?.Item1 == null) continue;

                // Preserve, in case we won't find another but don't necessarily return now
                result = new Tuple<object, string, IEntity>(foundSet.Item1, foundSet.Item2, entity);
                
                // if any non-null is ok, use that.
                if (!treatEmptyAsDefault) return result;

                // this may set a null, but may also set an empty string or empty array
                if (result.Item1.IsNullOrDefault()) continue;

                if (result.Item1 is string foundString && !string.IsNullOrEmpty(foundString)) return result;

                // Return entity-list if it has elements, otherwise continue searching
                if (result.Item1 is IEnumerable<IEntity> entityList && entityList.Any()) return result;
                
                // not sure if this will ever hit
                if (result.Item1 is ICollection list && list.Count != 0) return result;
            }

            return result;
        }
    }
}

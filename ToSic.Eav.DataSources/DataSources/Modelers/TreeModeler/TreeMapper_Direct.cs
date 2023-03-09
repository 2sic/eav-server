using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Generics;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    public partial class TreeMapper
    {

        private List<EntityPair<TNewEntity>> AddRelationshipField<TNewEntity, TKey>(string newField, List<(EntityPair<TNewEntity> set, List<TKey> NeedsIds)> list, ILookup<TKey, IEntity> lookup = null)
        {
            var useNumber = typeof(TKey).IsNumeric();
            var result = list.Select(setNeedsBundle =>
            {
                var target = setNeedsBundle.set.Entity;
                var attributes = target.Attributes.ToEditable();
                attributes = AddRelationships(attributes, newField, lookup, setNeedsBundle.NeedsIds,
                    $"Entity: {target.EntityId}/{target.EntityGuid}");
                return new EntityPair<TNewEntity>(_builder.Entity.Clone(target, attributes: _builder.Attribute.Create(attributes)), setNeedsBundle.set.Partner);
            });
            return result.ToList();
        }

        private IDictionary<string, IAttribute> AddRelationships<TKey>(
            IDictionary<string, IAttribute> attributes,
            string newFieldName,
            ILookup<TKey, IEntity> lookup,
            List<TKey> lookupIds,
            string debug
        ) => Log.Func($"{debug} {newFieldName} pointing to {lookupIds}", () =>
        {
            // Find referencing entities (children or parents) - but only if we have a valid reference
            var related = lookupIds
                .SelectMany(lookupId => lookup[lookupId])
                .ToImmutableList();

            var relAttr = _builder.Attribute.CreateOneWayRelationship(newFieldName, related);
            attributes = _builder.Attribute.Replace(attributes, relAttr);

            // Log
            return (attributes, $"added {related.Count} items");
        });
    }
}

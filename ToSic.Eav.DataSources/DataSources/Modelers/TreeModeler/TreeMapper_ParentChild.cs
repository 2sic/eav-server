using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    public partial class TreeMapper
    {
        public IImmutableList<IEntity> AddParentChild(
            IEnumerable<IEntity> originals,
            string parentIdField,
            string childToParentRefField,
            string newChildrenField = default,
            string newParentField = default,
            LazyLookup<object, IEntity> lookup = default) => Log.Func(l =>
        {
            // Make sure we have field names in case they were not provided & full-clone entities/relationships
            newParentField = newParentField ?? DefaultParentFieldName;
            newChildrenField = newChildrenField ?? DefaultChildrenFieldName;
            lookup = lookup ?? new LazyLookup<object, IEntity>();
            var list = originals.ToList();

            // Prepare - figure out the parent IDs and Reference to Parent ID
            var withKeys = list
                .Select(e =>
                {
                    var ownId = GetObjKeyOrDefault(e, parentIdField);
                    var relatedId = GetObjKeyOrDefault(e, childToParentRefField);
                    return new EntityPair<(object OwnId, object RelatedId)>(e, (ownId, relatedId));
                })
                .ToList();

            // Create list of Entities with their new Attributes
            var parentNeedsChildren = withKeys.Select(pair => new EntityPair<List<IAttribute>>(pair.Entity, 
                new List<IAttribute>
                {
                    _builder.Attribute.CreateOneWayRelationship(newParentField, new List<object> { pair.Partner.RelatedId }, lookup),
                    _builder.Attribute.CreateOneWayRelationship(newChildrenField, new List<object> { "Needs:" + pair.Partner.OwnId }, lookup)
                }
                )).ToList();
            var result = AddRelationshipFieldNew(parentNeedsChildren);

            // Add lookup to own id
            lookup.Add(withKeys.Select(pair => new KeyValuePair<object, IEntity>(pair.Partner.OwnId, pair.Entity)));
            // Add list of "Needs:ParentId" so that the parents can find it
            lookup.Add(withKeys.Select(pair => new KeyValuePair<object, IEntity>("Needs:" + pair.Partner.RelatedId, pair.Entity)));

            return result.ToImmutableList();
        });

        private IEnumerable<IEntity> AddRelationshipFieldNew(IEnumerable<EntityPair<List<IAttribute>>> list) =>
            list.Select(pair =>
            {
                var attributes = _builder.Attribute.Replace(pair.Entity.Attributes, pair.Partner);
                return _builder.Entity.Clone(pair.Entity, attributes: _builder.Attribute.Create(attributes));
            }).ToList();


        private object GetObjKeyOrDefault(IEntity e, string attribute) => Log.Func(enabled: Debug, func: l =>
        {
            try
            {
                var val = e.GetBestValue(attribute, Array.Empty<string>());
                l.A(Debug, $"Entity: {e.EntityId}[{attribute}]={val} ({val.GetType().Name})");
                return val.ToString();
            }
            catch (Exception ex)
            {
                l.Ex(ex);
                return default;
            }
        });
    }
}

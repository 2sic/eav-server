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

            // Prepare - figure out the parent IDs and Reference to Parent ID
            var withKeys = originals
                .Select(e =>
                {
                    var ownId = GetKey(e, parentIdField);
                    var relatedId = GetKey(e, childToParentRefField);
                    return new EntityPair<(object OwnId, object RelatedId)>(e, (ownId, relatedId));
                })
                .ToList();

            var result = withKeys.Select(pair =>
            {
                var newAttributes = new List<IAttribute>
                {
                    _builder.Attribute.CreateRelationship(newParentField, new List<object> { pair.Partner.RelatedId }, lookup),
                    _builder.Attribute.CreateRelationship(newChildrenField, new List<object> { "Needs:" + pair.Partner.OwnId }, lookup)
                };
                var attributes = _builder.Attribute.Replace(pair.Entity.Attributes, newAttributes);
                return _builder.Entity.Clone(pair.Entity, attributes: _builder.Attribute.Create(attributes));
            }).ToList();

            // Add lookup to own id
            lookup.Add(withKeys.Select(pair => new KeyValuePair<object, IEntity>(pair.Partner.OwnId, pair.Entity)));
            // Add list of "Needs:ParentId" so that the parents can find it
            lookup.Add(withKeys.Select(pair => new KeyValuePair<object, IEntity>("Needs:" + pair.Partner.RelatedId, pair.Entity)));

            return result.ToImmutableList();
        });

        /// <summary>
        /// Gets the key for the specified field.
        /// It converts it to a `string` because that ensures that we'll always find it no matter how it was converted / extended with prefixes.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private object GetKey(IEntity e, string attribute) => Log.Func(enabled: Debug, func: l =>
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

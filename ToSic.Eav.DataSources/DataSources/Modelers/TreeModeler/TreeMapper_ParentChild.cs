using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    public partial class TreeMapper
    {
        public IImmutableList<IEntity> AddParentChild<TKey>(
            IEnumerable<IEntity> originals,
            string parentIdField,
            string childToParentRefField,
            string newChildrenField = default,
            string newParentField = default) => Log.Func(l =>
        {
            // Make sure we have field names in case they were not provided & full-clone entities/relationships
            newParentField = newParentField ?? DefaultParentFieldName;
            newChildrenField = newChildrenField ?? DefaultChildrenFieldName;
            var clones = originals.ToList();

            // Prepare - figure out the parent IDs and Reference to Parent ID
            var withKeys = clones
                .Select(e =>
                {
                    var ownId = GetTypedKeyOrDefault<TKey>(e, parentIdField);
                    var relatedId = GetTypedKeyOrDefault<TKey>(e, childToParentRefField);
                    return new EntityPair<(TKey OwnId, TKey RelatedId)>(e, (ownId, relatedId));
                })
                .ToList();

            // Assign parents to children
            withKeys = AddRelationshipField(newParentField,
                withKeys.Select(s => (s, new List<TKey> { s.Partner.RelatedId })).ToList(),
                withKeys.ToLookup(s => s.Partner.OwnId, s => s.Entity));

            withKeys = AddRelationshipField(newChildrenField,
                withKeys.Select(s => (s, new List<TKey> { s.Partner.OwnId })).ToList(),
                withKeys.ToLookup(s => s.Partner.RelatedId, s => s.Entity));


            var result = withKeys.Select(set => set.Entity);

            return result.ToImmutableList();
        });

        private TRelationshipKey GetTypedKeyOrDefault<TRelationshipKey>(IEntity e, string attribute) => Log.Func(enabled: Debug, func: l =>
        {
            try
            {
                var val = e.GetBestValue(attribute, Array.Empty<string>());

                l.A(Debug, $"Entity: {e.EntityId}[{attribute}]={val} ({val.GetType().Name})");

                if (val is TRelationshipKey val1)
                    return val1;
                if (typeof(TRelationshipKey) == typeof(Guid) && Guid.TryParse(val.ToString(), out var guid))
                    return (TRelationshipKey)(object)guid;

                if (typeof(TRelationshipKey).IsNumeric() && val.IsNumeric())
                    return val.TryConvert<TRelationshipKey>(true).Value;

                // Fallback, hope for the best
                return val.TryConvert<TRelationshipKey>().Value;
            }
            catch (Exception ex)
            {
                l.Ex(ex);
                return default;
            }
        });
    }
}

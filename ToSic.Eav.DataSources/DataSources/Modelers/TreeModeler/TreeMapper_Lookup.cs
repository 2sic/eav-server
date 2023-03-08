using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Generics;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    public partial class TreeMapper
    {
        //public List<EntityPair<TPartner>> AddRelationships<TPartner, TKey>(
        //    ) where TPartner : AppFileDataRawBase
        //{
        //    return null;
        //}

        public List<EntitySet<TPartner, List<TKey>>> AddOneRelationshipWIP<TPartner, TKey>(
            string fieldName,
            IList<EntityPair<TPartner>> needs,
            ILookup<TKey, IEntity> lookup,
            Func<EntityPair<TPartner>, IEnumerable<TKey>> keysFinder
        )
        {
            var needs2 = needs.Select(pair => pair.Extend(keysFinder?.Invoke(pair).ToList())).ToList();
            return AddOneRelationshipWIP(fieldName, needs2, lookup);
        }
        public List<EntitySet<TPartner, List<TKey>>> AddOneRelationshipWIP<TPartner, TKey>(
            string fieldName,
            List<EntitySet<TPartner, List<TKey>>> needs,
            ILookup<TKey, IEntity> lookup,
            Func<EntityPair<TPartner>, IEnumerable<TKey>> keysFinder = null)
        {
            return AddRelationshipFieldWIP(fieldName, needs, lookup);
        }
        private List<EntitySet<TPartner, List<TKey>>> AddRelationshipFieldWIP<TPartner, TKey>(string newField, IList<EntitySet<TPartner, List<TKey>>> list, ILookup<TKey, IEntity> lookup = null)
        {
            var result = list.Select(set =>
            {
                var target = set.Entity;
                var attributes = target.Attributes.ToEditable();
                attributes = AddRelationshipsWIP(attributes, newField, lookup, set.Assistant,
                    $"Entity: {target.EntityId}/{target.EntityGuid}");
                return set.Clone(entity: _builder.Entity.Clone(target, attributes: _builder.Attribute.Create(attributes)));
            });
            return result.ToList();
        }


        private List<EntityPair<TNewEntity>> AddRelationshipFieldWIP<TNewEntity, TKey>(string newField, List<(EntityPair<TNewEntity> set, List<TKey> NeedsIds)> list, ILookup<TKey, IEntity> lookup = null)
        {
            var result = list.Select(setNeedsBundle =>
            {
                var target = setNeedsBundle.set.Entity;
                var attributes = target.Attributes.ToEditable();
                attributes = AddRelationshipsWIP(attributes, newField, lookup, setNeedsBundle.NeedsIds,
                    $"Entity: {target.EntityId}/{target.EntityGuid}");
                return new EntityPair<TNewEntity>(_builder.Entity.Clone(target, attributes: _builder.Attribute.Create(attributes)), setNeedsBundle.set.Partner);
            });
            return result.ToList();
        }


        private IDictionary<string, IAttribute> AddRelationshipsWIP<TKey>(
            IDictionary<string, IAttribute> attributes,
            string newFieldName,
            ILookup<TKey, IEntity> lookup,
            IReadOnlyCollection<TKey> lookupIds,
            string debug
        ) => Log.Func($"{debug} {newFieldName} pointing to {lookupIds}", () =>
        {
            var lookupSource = new LookUpEntitiesSource<TKey>(lookupIds.ToImmutableList(), lookup);

            var relAttr = _builder.Attribute.CreateOneWayRelationship(newFieldName, lookupSource);
            attributes = _builder.Attribute.Replace(attributes, relAttr);

            // Log
            return (attributes, "ok");
        });
    }
}

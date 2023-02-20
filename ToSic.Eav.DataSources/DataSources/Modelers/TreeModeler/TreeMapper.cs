using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public class TreeMapper : ServiceBase, ITreeMapper, ICanDebug
    {
        public const string DefaultParentFieldName = "Parent";
        public const string DefaultChildrenFieldName = "Children";

        #region Constructor / DI



        #endregion
        private readonly MultiBuilder _builder;

        /// <summary>
        /// Constructor for DI
        /// </summary>
        /// <param name="builder"></param>
        public TreeMapper(MultiBuilder builder): base("DS.TreeMp")
        {
            _builder = builder;
            Debug = false;
        }

        public List<IEntity> AddSomeRelationshipsWIP<TKey>(
            string fieldName,
            List<(IEntity Entity, List<TKey> Ids)> needs,
            List<(IEntity Entity, TKey Id)> lookup,
            bool cloneFirst = true
        )
        {
            // WIP - for now the clone is very important, because it changes the attribute-model on generated entities from light to not-light
            // otherwise adding relationship attributes fails for now
            if (cloneFirst)
                needs = needs.Select(n => (_builder.FullClone(n.Entity) as IEntity, n.Ids)).ToList();
            
            var properLookup = lookup.ToLookup(i => i.Id, i => i.Entity);

            return AddRelationshipField(fieldName, needs, properLookup).ToList();
        }

        public IImmutableList<IEntity> AddRelationships<TKey>(
            IEnumerable<IEntity> originals,
            string parentIdField,
            string childToParentRefField,
            string newChildrenField = default,
            string newParentField = default) => Log.Func(l =>
        {
            // Make sure we have field names in case they were not provided & full-clone entities/relationships
            newParentField = newParentField ?? DefaultParentFieldName;
            newChildrenField = newChildrenField ?? DefaultChildrenFieldName;
            var clones = originals.Select(e => _builder.FullClone(e)).ToList();

            // Prepare - figure out the parent IDs and Reference to Parent ID
            var withKeys = clones
                .Select(e => new
                {
                    Entity = e as IEntity,
                    OwnId = GetTypedKeyOrDefault<TKey>(e, parentIdField),
                    RelatedId = GetTypedKeyOrDefault<TKey>(e, childToParentRefField),
                })
                .ToList();

            // Assign parents to children
            AddRelationshipField(newParentField, 
                withKeys.Select(set => (set.Entity, new List<TKey> { set.RelatedId })).ToList(), 
                withKeys.ToLookup(s => s.OwnId,  s => s.Entity as IEntity));

            AddRelationshipField(newChildrenField, 
                withKeys.Select(set => (set.Entity, new List<TKey> { set.OwnId })).ToList(),
                withKeys.ToLookup(s => s.RelatedId, s => s.Entity as IEntity));


            var result = withKeys.Select(set => set.Entity).Cast<IEntity>();
            
            return result.ToImmutableArray();
        });
        

        private List<IEntity> AddRelationshipField<TKey>(string newField, List<(IEntity Entity, List<TKey> NeedsIds)> list, ILookup<TKey, IEntity> lookup = null)
        {
            var useNumber = typeof(TKey).IsNumeric();
            foreach (var item in list)
                AddRelationships(item.Entity, newField, lookup, item.NeedsIds, useNumber);
            return list.Select(i => i.Entity).ToList();
        }

        private void AddRelationships<TKey>(IEntity target, string newFieldName, ILookup<TKey, IEntity> lookup, List<TKey> lookupIds, bool keyIsNumeric
        ) => Log.Do($"Entity: {target.EntityId}/{target.EntityGuid} {newFieldName} pointing to {lookupIds}", () =>
        {
            // Find referencing entities (children or parents) - but only if we have a valid reference
            var related = lookupIds.SelectMany(lookupId => lookup[lookupId]).ToList();

            // Create Guid List of children (note 2dm - not sure why..., as the guids may be Guid.Empty)
            // but changing it if numeric actually fails, maybe a guid is numeric?
            var childGuids = keyIsNumeric // typeof(TKey).IsNumeric()
                ? related.Select(e => e.EntityId).ToList() as object
                : related.Select(e => e.EntityGuid).ToList();
            _builder.Attribute.AddValue(target.Attributes, newFieldName, childGuids, DataTypes.Entity,
                null, false, false, new DirectEntitiesSource(related));

            // Log
            return $"added {related.Count} items";
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

        public bool Debug { get; set; }
    }


}
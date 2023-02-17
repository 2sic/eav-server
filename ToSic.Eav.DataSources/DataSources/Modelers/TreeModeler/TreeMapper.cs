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



        public IImmutableList<IEntity> AddRelationships<TRel>(
            IEnumerable<IEntity> originals,
            string parentIdField,
            string childToParentRefField,
            string newChildrenField = default,
            string newParentField = default) => Log.Func(l =>
        {
            // Make sure we have field names in case they were not provided
            newParentField = newParentField ?? DefaultParentFieldName;
            newChildrenField = newChildrenField ?? DefaultChildrenFieldName;

            // Copy all entities to prevent modification of original
            var clones = originals
                .Select(e => _builder.Entity.Clone(e,
                    _builder.Attribute.Clone(e.Attributes),
                    ((RelationshipManager)e.Relationships).AllRelationships)
                )
                .ToList();

            // Prepare - figure out the parent IDs and Reference to Parent ID
            var entitiesWithKeys = clones
                .Select(e => new
                {
                    Entity = e,
                    OwnId = GetTypedKeyOrDefault<TRel>(e, parentIdField),
                    ParentId = GetTypedKeyOrDefault<TRel>(e, childToParentRefField),
                })
                .ToList();

            // Convert list to lookup of "parent" guids/ids
            var childrenLookup = entitiesWithKeys.ToLookup(set => set.ParentId, set => set.Entity);

            var parentLookup = entitiesWithKeys.ToLookup(set => set.OwnId, set => set.Entity);


            // Assign children to parents
            var result = new List<IEntity>();
            foreach (var item in entitiesWithKeys)
            {
                var entity = item.Entity;

                // When Entity is Parent: Find and assign all children
                var children = AddRelationship(entity, item.OwnId, childrenLookup, newChildrenField);

                var parents = AddRelationship(entity, item.ParentId, parentLookup, newParentField);

                l.A($"Adding to Entity {entity.EntityId}/{entity.EntityGuid}: Children {children.Count}; Parent: {parents.Count}; OwnId: {item.OwnId}; ParentRef: {item.ParentId}");

                result.Add(entity);
            }
            
            return result.ToImmutableArray();
        });

        private List<Entity> AddRelationship<TKey>(Entity parent, TKey lookupId, ILookup<TKey, Entity> lookup, string newFieldName)
        {
            // Find referencing entities (children or parents) - but only if we have a valid reference
            var related = !EqualityComparer<TKey>.Default.Equals(lookupId, default)
                ? lookup[lookupId].ToList()
                : new List<Entity>();

            // Create Guid List of children (note 2dm - not sure why..., as the guids may be Guid.Empty)
            // but changing it if numeric actually fails, maybe a guid is numeric?
            var childGuids = // typeof(TKey).IsNumeric()
                //? related.Select(e => e.EntityId)
                /*:*/ related.Select(e => e.EntityGuid).ToList() as object;
            _builder.Attribute.AddValue(parent.Attributes, newFieldName, childGuids, DataTypes.Entity,
                null, false, false, new DirectEntitiesSource(related));
            return related; // for tracing/debug on caller
        }

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
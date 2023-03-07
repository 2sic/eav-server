﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Process;
using ToSic.Eav.Generics;
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
        private readonly DataBuilder _builder;

        /// <summary>
        /// Constructor for DI
        /// </summary>
        /// <param name="builder"></param>
        public TreeMapper(DataBuilder builder): base("DS.TreeMp")
        {
            _builder = builder;
            Debug = false;
        }

        public IList<EntityPair<TRaw>> AddOneRelationship<TRaw, TKey>(
            string fieldName,
            List<(EntityPair<TRaw> Set, List<TKey> Ids)> needs,
            List<(IEntity Entity, TKey Id)> lookup
        )
        {
            var properLookup = lookup.ToLookup(i => i.Id, i => i.Entity);

            return AddRelationshipField(fieldName, needs, properLookup);
        }

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
                    return new EntityPair<(TKey OwnId, TKey RelatedId)>((ownId, relatedId), e);
                })
                .ToList();

            // Assign parents to children
            withKeys = AddRelationshipField(newParentField, 
                withKeys.Select(s => (s, new List<TKey> { s.Partner.RelatedId })).ToList(), 
                withKeys.ToLookup(s => s.Partner.OwnId,  s => s.Entity));

            withKeys = AddRelationshipField(newChildrenField, 
                withKeys.Select(s => (s, new List<TKey> { s.Partner.OwnId })).ToList(),
                withKeys.ToLookup(s => s.Partner.RelatedId, s => s.Entity));


            var result = withKeys.Select(set => set.Entity);
            
            return result.ToImmutableList();
        });


        private List<EntityPair<TNewEntity>> AddRelationshipField<TNewEntity, TKey>(string newField, List<(EntityPair<TNewEntity> set, List<TKey> NeedsIds)> list, ILookup<TKey, IEntity> lookup = null)
        {
            var useNumber = typeof(TKey).IsNumeric();
            var result = list.Select(setNeedsBundle =>
            {
                var target = setNeedsBundle.set.Entity;
                var attributes = target.Attributes.ToEditable();
                attributes = AddRelationships(attributes, newField, lookup, setNeedsBundle.NeedsIds,
                    $"Entity: {target.EntityId}/{target.EntityGuid}");
                return new EntityPair<TNewEntity>(setNeedsBundle.set.Partner,
                    _builder.Entity.Clone(target, attributes: _builder.Attribute.Create(attributes)));
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
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
        public const string DefaultParentAttribute = "Parent";
        public const string DefaultChildrenAttribute = "Children";

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
            string parentIdentifierAttribute,
            string childParentAttribute,
            string targetChildrenAttribute = default,
            string targetParentAttribute = default) where TRel: struct => Log.Func(l =>
        {

            targetParentAttribute = targetParentAttribute ?? DefaultParentAttribute;
            targetChildrenAttribute = targetChildrenAttribute ?? DefaultChildrenAttribute;

            // Copy all entities to prevent modification of original
            var clones = originals.Select(e => _builder.Entity
                    .Clone(e,
                        _builder.Attribute.Clone(e.Attributes),
                        ((RelationshipManager)e.Relationships).AllRelationships
                    )
                )
                .ToList();

            // Convert list to lookup of "parent" guids
            var childrenByParentIdentifier = clones.ToLookup(e => GetTypedValueOrNull<TRel>(e, childParentAttribute), e => e);

            var identifiers = clones.ToDictionary(e => GetTypedValueOrNull<TRel>(e, parentIdentifierAttribute), e => e);

            // Assign children to parents
            var result = new List<IEntity>();
            foreach (var item in identifiers)
            {
                var entity = item.Value;

                // Find and assign children
                var children = childrenByParentIdentifier[item.Key].ToList();
                _builder.Attribute.AddValue(entity.Attributes, targetChildrenAttribute,
                    children.Select(e => e.EntityGuid).ToList(), "Entity",
                    null, false, false,
                    new DirectEntitiesSource(children));

                // Find and assign parent
                var parentIdentifier = GetTypedValueOrNull<TRel>(entity, childParentAttribute);
                var parents = new List<IEntity>();
                if (parentIdentifier.HasValue && identifiers.ContainsKey(parentIdentifier))
                    parents.Add(identifiers[parentIdentifier]);
                _builder.Attribute.AddValue(entity.Attributes, targetParentAttribute,
                    parents.Select(e => e.EntityGuid).ToList(), "Entity",
                    null, false, false,
                    new DirectEntitiesSource(parents));

                l.A($"Adding to Entity {entity.EntityId}/{entity.EntityGuid}: Children {children.Count}; Parent: {parents.Count} - {parentIdentifier}");

                result.Add(entity);
            }
            
            return result.ToImmutableArray();
        });

        private TRelationshipKey? GetTypedValueOrNull<TRelationshipKey>(IEntity e, string attribute) where TRelationshipKey: struct 
            => Log.Func<TRelationshipKey?>(enabled: Debug, func: l =>
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
                return null;
            }
        });

        public bool Debug { get; set; }
    }


}
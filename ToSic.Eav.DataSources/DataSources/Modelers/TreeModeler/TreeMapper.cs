using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;

namespace ToSic.Eav.DataSources
{
    internal class TreeMapper<T> : ITreeMapper where T : struct
    {
        // TODO: PROBABLY make DI
        public TreeMapper(AttributeBuilder attributeBuilder, EntityBuilder entityBuilder)
        {
            _attributeBuilder = attributeBuilder;
            _entityBuilder = entityBuilder;
        }
        private readonly AttributeBuilder _attributeBuilder;
        private readonly EntityBuilder _entityBuilder;


        public IImmutableList<IEntity> GetEntitiesWithRelationships(IEnumerable<IEntity> originals, string parentIdentifierAttribute, string childParentAttribute, string targetChildrenAttribute, string targetParentAttribute)
        {
            var result = new List<IEntity>();

            // Copy all entities to prevent modification
            var clones = originals.Select(e => _entityBuilder.FullClone(e, e.Attributes.Copy(), ((RelationshipManager)e.Relationships).AllRelationships)).ToList();

            // Convert list to lookup of "parent" guids
            var childrenByParentIdentifier = clones.ToLookup(e => GetTypedValueOrNull(e, childParentAttribute), e => e);

            var identifiers = clones.ToDictionary(e => GetTypedValueOrNull(e, parentIdentifierAttribute), e => e);

            // Assign children to parents
            foreach (var item in identifiers)
            {
                var entity = item.Value;

                // Find and assign children
                var children = childrenByParentIdentifier[item.Key].ToList();
                _attributeBuilder.AddValue(entity.Attributes, targetChildrenAttribute, children.Select(e => e.EntityGuid).ToList(), "Entity", null, false, false, new DirectEntitiesSource(children));
                //entity.Attributes.AddValue(targetChildrenAttribute, children.Select(e => e.EntityGuid).ToList(), "Entity", null, false, false, new DirectEntitiesSource(children));

                // Find and assign parent
                var parentIdentifier = GetTypedValueOrNull(entity, childParentAttribute);
                var parents = new List<IEntity>();
                if (parentIdentifier.HasValue && identifiers.ContainsKey(parentIdentifier)) {
                    parents.Add(identifiers[parentIdentifier]);
                }
                _attributeBuilder.AddValue(entity.Attributes, targetParentAttribute, parents.Select(e => e.EntityGuid).ToList(), "Entity", null, false, false, new DirectEntitiesSource(parents));
                //entity.Attributes.AddValue(targetParentAttribute, parents.Select(e => e.EntityGuid).ToList(), "Entity", null, false, false, new DirectEntitiesSource(parents));

                result.Add(entity);
            }
            
            return result.ToImmutableArray();
        }

        private T? GetTypedValueOrNull(IEntity e, string attribute)
        {
            try
            {
                var val = e.GetBestValue(attribute, new string[0]);
                if (val is T val1) return val1;
                if(typeof(T) == typeof(Guid))
                {
                    if (Guid.TryParse(val.ToString(), out Guid guid))
                        return (T)(object)guid;
                }
                return (T)val;
            } catch
            {
                return null;
            }
        }
    }


}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Represents Relationships to Child Entities
    /// </summary>
    public class EntityRelationship : IEnumerable<IEntity>
    {
        /// <summary>
        /// Blank value, just for marking the list as empty
        /// </summary>
        private static readonly int?[] EntityIdsEmpty = new int?[0];

        private readonly bool _useGuid;

        /// <summary>
        /// List of Child EntityIds
        /// </summary>
        public IEnumerable<int?> EntityIds 
            => _entityIds ?? (_entityIds = this.Select(e => e?.EntityId).ToList());

        private IEnumerable<int?> _entityIds;

        private List<Guid?> Guids { get; }

        private readonly IDeferredEntitiesList _fullEntityList;
        private List<IEntity> _entities;

        /// <summary>
        /// Initializes a new instance of the EntityRelationship class.
        /// </summary>
        /// <param name="allEntities">DataSource to retrieve child entities</param>
        /// <param name="entityIds">List of IDs to initialize with</param>
        public EntityRelationship(IDeferredEntitiesList allEntities, IEnumerable<int?> entityIds)
        {
            _useGuid = false;
            _entityIds = entityIds ?? EntityIdsEmpty;
            _fullEntityList = allEntities; 
        }

        /// <summary>
        /// Initializes a new instance of the EntityRelationship class.
        /// </summary>
        /// <param name="allEntities">DataSource to retrieve child entities</param>
        /// <param name="entityGuids">List of IDs to initialize with</param>
        public EntityRelationship(IDeferredEntitiesList allEntities, List<Guid?> entityGuids)
        {
            _useGuid = true;
            Guids = entityGuids;
            _fullEntityList = allEntities;
        }


        public override string ToString() => EntityIds == null ? string.Empty : string.Join(",", EntityIds.Select(e => e));

        public IEnumerator<IEntity> GetEnumerator()
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (_entities == null)
                LoadEntities();

            return new EntityEnumerator(_entities);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void LoadEntities()
        {
            _entities = _fullEntityList == null
                ? new List<IEntity>()
                : (_useGuid
                    ? Guids.Select(l => !l.HasValue
                        ? null
                        // special: in some cases, the entity cannot be found because it has been deleted or something
                        : _fullEntityList.LightList.FirstOrDefault(e => e.EntityGuid == l))
                    : EntityIds.Select(l => l.HasValue
                        ? (_fullEntityList.List.ContainsKey(l.Value) ? _fullEntityList.List[l.Value] : null)
                        // special: in some cases, the entity cannot be found because it has been deleted or something
                        : null)).ToList();
        }

    }
}
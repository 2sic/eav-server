using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Query;
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
        private static readonly List<int?> EntityIdsEmpty = new List<int?>();

        private readonly bool _useGuid;

        /// <summary>
        /// List of Child EntityIds
        /// </summary>
        public List<int?> EntityIds 
            => _entityIds ?? (_entityIds = this.Select(e => e?.EntityId).ToList());

        /// <summary>
        /// Identifiers
        /// </summary>
        public IList Identifiers
            => _useGuid ? Guids.ToList() as IList : EntityIds.ToList();

        private List<int?> _entityIds;

        internal List<Guid?> Guids { get; }

        private readonly IDeferredEntitiesList _lookupList;
        private List<IEntity> _entities;


        /// <summary>
        /// Initializes a new instance of the EntityRelationship class.
        /// </summary>
        /// <param name="allEntities">DataSource to retrieve child entities</param>
        /// <param name="identifiers">List of IDs to initialize with</param>
        public EntityRelationship(IDeferredEntitiesList allEntities, IList identifiers)
        {
            _lookupList = allEntities;
            switch (identifiers)
            {
                case null:
                    _useGuid = false;
                    _entityIds = EntityIdsEmpty;
                    break;
                case List<int?> _:
                    _useGuid = false;
                    _entityIds = (List<int?>) identifiers;
                    break;
                case List<Guid?> _:
                    _useGuid = true;
                    Guids = (List<Guid?>) identifiers;
                    break;
                default:
                    throw new Exception("relationship identifiers must be int? or guid?, anything else won't work");
            }

        }

        ///// <summary>
        ///// Initializes a new instance of the EntityRelationship class.
        ///// </summary>
        ///// <param name="allEntities">DataSource to retrieve child entities</param>
        ///// <param name="entityGuids">List of IDs to initialize with</param>
        //public EntityRelationship(IDeferredEntitiesList allEntities, List<Guid?> entityGuids)
        //{
        //    _useGuid = true;
        //    Guids = entityGuids;
        //    _lookupList = allEntities;
        //}


        // todo: unclear when this is actually needed / used?
        public override string ToString()
        {
            return !_useGuid
                ? (EntityIds != null ? string.Join(",", EntityIds) : string.Empty)
                : (Guids != null ? string.Join(",", Guids) : string.Empty);

        }

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
            _entities = _lookupList == null
                ? new List<IEntity>()
                : (_useGuid
                    ? Guids.Select(l => !l.HasValue
                        ? null
                        // special: in some cases, the entity cannot be found because it has been deleted or something
                        : _lookupList.List.One(l.Value))
                    : EntityIds.Select(l => l.HasValue
                        ? _lookupList.List.FindRepoId(l.Value)// (_lookupList.List.ContainsKey(l.Value) ? _lookupList.List[l.Value] : null)
                        // special: in some cases, the entity cannot be found because it has been deleted or something
                        : null)).ToList();
        }

    }
}
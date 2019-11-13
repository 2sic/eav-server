using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data.Query;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Delivers entities which are needed. <br/>
    /// It's lazy, because on initialization it only knows the Ids (int/guid) of the items to pick up,
    /// and only retrieves them when needed. <br/>
    /// - Once retrieved, it will cache the result, until the up-stream reports changes.
    /// - An additional optimization: in special cases where we just need the IDs for saving,
    ///   it won't actually query the cache if the IDs are already known.
    /// </summary>
    [PublicApi]
    public partial class LazyEntities : IEnumerable<IEntity>, ICacheDependent
    {
        /// <summary>
        /// Blank value, just for marking the list as empty
        /// </summary>
        private static readonly List<int?> EntityIdsEmpty = new List<int?>();

        private readonly bool _preferGuid;

        /// <summary>
        /// List of Child EntityIds - int-based.
        /// </summary>
        /// <remarks>
        /// Note that only the EntityIds <em>or</em> the Guids should be populated.
        /// </remarks>
        internal List<int?> EntityIds 
            => _entityIds ?? (_entityIds = this.Select(e => e?.EntityId).ToList());

        /// <summary>
        /// Identifiers of the items in the list. Build with either the Guids or the Ids, depending on what was used.
        /// Special mechanism to get identifiers to DB storage, without loading the entities themselves
        /// </summary>
        public IList Identifiers => _preferGuid ? Guids as IList : EntityIds;


        private List<int?> _entityIds;

        /// <summary>
        /// List of Child EntityIds - int-based.
        /// </summary>
        /// <remarks>
        /// Note that only the EntityIds <em>or</em> the Guids should be populated.
        /// </remarks>
        internal List<Guid?> Guids { get; }

        /// <summary>
        /// Lookup the guids of all relationships
        /// Either because the guids were stored - and are the primary key
        /// or because the IDs were stored, and the guids were then looked up
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is important for serializing to json, because there we need the guids, 
        /// and the serializer shouldn't have know about the internals of relationship management
        /// </remarks>
        [PrivateApi]
        public List<Guid?> ResolveGuids()
        {
            if (_preferGuid) return Guids;

            // if we have number-IDs, but no lookup system, we'll have to use this as lookup system
            if (_entityIds != null && _entityIds.Count > 0 && _lookupList == null) // not set yet
                throw new Exception("trying to resolve guids for this relationship, but can't, because the lookupList is not available");

            return this.Select(e => e?.EntityGuid).ToList();
        }

        [PrivateApi]
        internal void AttachLookupList(IEntitiesSource lookupList)
        {
            _lookupList = lookupList
                ?? throw new ArgumentNullException(nameof(lookupList), "Trying to resolve relationship guids, which requires a full list of the app-items, but didn't receive it.");
            _entities = null; // reset possibly cached list of entities from before, so it will be rebuilt
        }

        private IEntitiesSource _lookupList;
        private List<IEntity> _entities;


        /// <summary>
        /// Initializes a new instance of the EntityRelationship class.
        /// </summary>
        /// <param name="allEntities">DataSource to retrieve child entities</param>
        /// <param name="identifiers">List of IDs to initialize with</param>
        [PrivateApi]
        internal LazyEntities(IEntitiesSource allEntities, IList identifiers)
        {
            _lookupList = allEntities;
            switch (identifiers)
            {
                case null:
                    _preferGuid = false;
                    _entityIds = EntityIdsEmpty;
                    break;
                case List<int?> intList:
                    _preferGuid = false;
                    _entityIds = intList;
                    break;
                case List<Guid?> guids:
                    _preferGuid = true;
                    Guids = guids;
                    break;
                default:
                    throw new Exception("relationship identifiers must be int? or guid?, anything else won't work");
            }

        }

        // todo: unclear when this is actually needed / used?
        [PrivateApi]
        public override string ToString()
        {
            return !_preferGuid
                ? (EntityIds != null ? string.Join(",", EntityIds) : string.Empty)
                : (Guids != null ? string.Join(",", Guids) : string.Empty);

        }

        [PrivateApi]
        public IEnumerator<IEntity> GetEnumerator()
        {
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            if (_entities == null || CacheChanged())
                _entities = LoadEntities();

            return new EntityEnumerator(_entities);
        }

        [PrivateApi]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private List<IEntity> LoadEntities()
        {
            var result = _lookupList == null
                ? new List<IEntity>()
                : (_preferGuid
                    ? Guids.Select(l => !l.HasValue
                        ? null
                        // special: in some cases, the entity cannot be found because it has been deleted or something
                        : _lookupList.List.One(l.Value))
                    : EntityIds.Select(l => l.HasValue
                        ? _lookupList.List.FindRepoId(l.Value)
                        // special: in some cases, the entity cannot be found because it has been deleted or something
                        : null)).ToList();

            CacheTimestamp = _lookupList?.CacheTimestamp ?? 0;
            return result;
        }

        /// <inheritdoc />
        public long CacheTimestamp { get; private set; }

        /// <inheritdoc />
        public bool CacheChanged() => _lookupList?.CacheChanged(CacheTimestamp) ?? false;
    }
}
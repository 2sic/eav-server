using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    [PrivateApi("internal use only, don't publish this object")]
    internal class AppMetadataManager: HasLog, IMetadataSource
    {
        #region cache value objects: Types, _guid, _number, _string
        /// <summary>
        /// Type-map for id/name of types
        /// </summary>
        private ImmutableDictionary<int, string> Types { get; }

        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
        /// </summary>
        private Dictionary<int, Dictionary<Guid, List<IEntity>>> _guid;

        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
        /// </summary>
        private Dictionary<int, Dictionary<int, List<IEntity>>> _number;

        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
        /// </summary>
        private Dictionary<int, Dictionary<string, List<IEntity>>> _string;

        private readonly AppState _app;

        #endregion

        public AppMetadataManager(AppState app, ImmutableDictionary<int, string> metadataTypes, ILog parentLog) 
            : base("App.MDMan", parentLog, "initialize")
        {
            _app = app;

            Types = metadataTypes;

            // make sure the lists have a sub-list for each known relationship type
            Reset();
        }

        /// <summary>
        /// Reset all indexes
        /// </summary>
        internal void Reset()
        {
            _guid = new Dictionary<int, Dictionary<Guid, List<IEntity>>>();
            _number = new Dictionary<int, Dictionary<int, List<IEntity>>>();
            _string = new Dictionary<int, Dictionary<string, List<IEntity>>>();

            foreach (var mdt in Types)
            {
                _guid.Add(mdt.Key, new Dictionary<Guid, List<IEntity>>());
                _number.Add(mdt.Key, new Dictionary<int, List<IEntity>>());
                _string.Add(mdt.Key, new Dictionary<string, List<IEntity>>());
            }
        }

        #region Cache Timestamp & Invalidation

        public long CacheTimestamp => _app.CacheTimestamp;
        public bool CacheChanged(long newCacheTimeStamp) => _app.CacheChanged(newCacheTimeStamp);

        #endregion

        /// <summary>
        /// Register an entity to the metadata manager
        /// This ensures that any request for metadata would include this entity, if it's metadata
        /// </summary>
        /// <param name="entity"></param>
        internal void Register(Entity entity)
        {
            var md = entity.MetadataFor;
            if (!md.IsMetadata) return;

            // Try guid first. Note that an item can be assigned to both a guid, string and an int if necessary, though not commonly used
            if (md.KeyGuid.HasValue)
                AddToMetaDic(_guid, md.TargetType, md.KeyGuid.Value, entity);
            if (md.KeyNumber.HasValue)
                AddToMetaDic(_number, md.TargetType, md.KeyNumber.Value, entity);
            if (!string.IsNullOrEmpty(md.KeyString))
                AddToMetaDic(_string, md.TargetType, md.KeyString, entity);
        }

        
        private static void AddToMetaDic<T>(IReadOnlyDictionary<int, Dictionary<T, List<IEntity>>> metadataIndex, int mdTargetType, T mdValue, IEntity newEntity)
        {
            // get the index of the target type (like 4 = Entity target)
            var indexOfType = metadataIndex[mdTargetType];
            // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) exists, otherwise create empty list
            var list = indexOfType.ContainsKey(mdValue) ? indexOfType[mdValue] : indexOfType[mdValue] = new List<IEntity>();

            // in case it was already in this index, remove first
            var found = list.One(newEntity.EntityId);
            if (found != null)
                list.Remove(found);

            // Now all containers must exist, add this item
            list.Add(newEntity);
        }

        // FYI: disabled 2021-11-19, was deprecated since v11.11 #cleanup EOY 2021
        ///// <inheritdoc />
        //public IEnumerable<IEntity> Get<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null)
        //    => GetMetadata(targetType, key, contentTypeName);

        /// <inheritdoc />
        public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null)
        {
            if(key == null) return new IEntity[0];
            var type = typeof(TMetadataKey);
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(Guid))
                return Lookup(_guid, targetType, key as Guid? ?? Guid.Empty, contentTypeName);

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                    return Lookup(_number, targetType, key as int? ?? 0, contentTypeName);
                case TypeCode.String:
                    return Lookup(_string, targetType, key as string, contentTypeName);
                default:
                    return Lookup(_string, targetType, key.ToString(), contentTypeName);
            }
        }

        private static IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, List<IEntity>>> list, int targetType, T key, string contentTypeName)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            if (list.TryGetValue(targetType, out var keyDict)
                && keyDict.TryGetValue(key, out var entities))
                return contentTypeName == null
                    ? entities
                    : entities.Where(e => e.Type.Is(contentTypeName));
            return new List<IEntity>();
        }
    }
}

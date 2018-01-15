using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    internal class AppMetadataManager: IMetadataProvider
    {
        /// <summary>
        /// Type-map for id/name of types
        /// </summary>
        private ImmutableDictionary<int, string> Types { get; }

        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
        /// </summary>
        private readonly Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> _guid
            = new Dictionary<int, Dictionary<Guid, IEnumerable<IEntity>>>();

        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, IEnumerable<IEntity>>> _number
            = new Dictionary<int, Dictionary<int, IEnumerable<IEntity>>>();

        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
        /// </summary>
        private readonly Dictionary<int, Dictionary<string, IEnumerable<IEntity>>> _string
            = new Dictionary<int, Dictionary<string, IEnumerable<IEntity>>>();

        public AppMetadataManager(ImmutableDictionary<int, string> metadataTypes)
        {
            Types = metadataTypes;

            foreach (var mdt in Types)
            {
                _guid.Add(mdt.Key, new Dictionary<Guid, IEnumerable<IEntity>>());
                _number.Add(mdt.Key, new Dictionary<int, IEnumerable<IEntity>>());
                _string.Add(mdt.Key, new Dictionary<string, IEnumerable<IEntity>>());
            }
        }

        /// <summary>
        /// Register an entity to the metadata manager
        /// This ensures that any request for metadata would include this entity, if it's metadata
        /// </summary>
        /// <param name="entity"></param>
        public void Add(Entity entity)
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
        
        private static void AddToMetaDic<T>(Dictionary<int, Dictionary<T, IEnumerable<IEntity>>> metadataIndex, int mdTargetType, T mdValue, Entity newEntity)
        {
            // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) has an empty list of items
            if (!metadataIndex[mdTargetType].ContainsKey(mdValue)) // ensure target list exists
                metadataIndex[mdTargetType][mdValue] = new List<IEntity>();

            // Now all containers must exist, add this item
            ((List<IEntity>)metadataIndex[mdTargetType][mdValue]).Add(newEntity);
        }



        /// <summary>
        /// Get metadata-items of something
        /// </summary>
        /// <typeparam name="TMetadataKey">Type - guid, int or string</typeparam>
        /// <param name="targetType">target-type is a number from 1-4 which says if it's metadata of an entity, of an attribute, etc.</param>
        /// <param name="key">the (int/guid/string) key we're looking for</param>
        /// <param name="contentTypeName">an optional type name, if we only want the items of a specific type</param>
        /// <returns></returns>
        public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null)
        {
            if (typeof(TMetadataKey) == typeof(Guid))
                return Lookup(_guid, targetType, key as Guid? ?? Guid.Empty, contentTypeName);

            switch (Type.GetTypeCode(typeof(TMetadataKey)))
            {
                case TypeCode.Int32:
                    return Lookup(_number, targetType, key as int? ?? 0, contentTypeName);
                case TypeCode.String:
                    return Lookup(_string, targetType, key as string, contentTypeName);
                default:
                    return Lookup(_string, targetType, key.ToString(), contentTypeName);
            }
        }

        private static IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, IEnumerable<IEntity>>> list, int targetType, T key, string contentTypeName)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            if (list.TryGetValue(targetType, out Dictionary<T, IEnumerable<IEntity>> keyDict))
                if (keyDict.TryGetValue(key, out IEnumerable<IEntity> entities))
                    return contentTypeName == null
                        ? entities
                        : entities.Where(e => e.Type.StaticName == contentTypeName);
            return new List<IEntity>();
        }
    }
}

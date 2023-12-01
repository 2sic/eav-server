using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.State;

[PrivateApi("internal use only")]
internal class AppMetadataManager: IMetadataSource
{
    #region cache value objects: Types, _guid, _number, _string

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

    #endregion

    #region IAppIdentity

    private readonly IAppIdentity _appIdentity;
    public int ZoneId => _appIdentity.ZoneId;

    public int AppId => _appIdentity.AppId;


    #endregion

    public AppMetadataManager(IAppIdentity appIdentity, ICacheExpiring cacheExpiry)
    {
        _appIdentity = appIdentity;
        _appCacheInfo = cacheExpiry;
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
    }

    #region Cache Timestamp & Invalidation

    private readonly ICacheExpiring _appCacheInfo;

    public long CacheTimestamp => _appCacheInfo.CacheTimestamp;
    public bool CacheChanged(long dependentTimeStamp) => _appCacheInfo.CacheChanged(dependentTimeStamp);

    #endregion

    /// <summary>
    /// Register an entity to the metadata manager
    /// This ensures that any request for metadata would include this entity, if it's metadata
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="add">false - removes entity from list</param>
    internal void Register(IEntity entity, bool add)
    {
        var md = entity.MetadataFor;
        if (!md.IsMetadata) return;

        // Try guid first. Note that an item can be assigned to both a guid, string and an int if necessary, though not commonly used
        if (md.KeyGuid.HasValue)
            AddToMetaDic(_guid, md.TargetType, md.KeyGuid.Value, entity, add);
        if (md.KeyNumber.HasValue)
            AddToMetaDic(_number, md.TargetType, md.KeyNumber.Value, entity, add);
        if (!string.IsNullOrEmpty(md.KeyString))
            AddToMetaDic(_string, md.TargetType, md.KeyString, entity, add);
    }

        
    private static void AddToMetaDic<T>(IDictionary<int, Dictionary<T, List<IEntity>>> metadataIndex, int mdTargetType, T mdValue, IEntity entity, bool add)
    {
        // Get or create the metadata index on this targetType
        if (!metadataIndex.TryGetValue(mdTargetType, out var indexOfType))
            indexOfType = metadataIndex[mdTargetType] = new Dictionary<T, List<IEntity>>();

        // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) exists, otherwise create empty list
        var list = indexOfType.TryGetValue(mdValue, out var lst) ? lst : indexOfType[mdValue] = new List<IEntity>();

        // in case it was already in this index, remove first
        var found = list.One(entity.EntityId);
        if (found != null)
            list.Remove(found);

        // Now all containers must exist, add this item
        if (add) 
            list.Add(entity);
    }

    /// <inheritdoc />
    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null)
    {
        if(key == null) return Array.Empty<IEntity>();
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

    public IEnumerable<IEntity> GetMetadata<T>(TargetTypes targetType, T key, string contentTypeName = null) 
        => GetMetadata((int)targetType, key, contentTypeName);

    private static IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, List<IEntity>>> list, int targetType, T key, string contentTypeName)
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        if (list.TryGetValue(targetType, out var keyDict)
            && keyDict.TryGetValue(key, out var entities))
            return contentTypeName == null
                ? entities
                : entities.Where(e => e.Type.Is(contentTypeName));
        return Array.Empty<IEntity>();
    }
}
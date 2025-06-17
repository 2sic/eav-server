﻿using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;
using ToSic.Sys.Caching;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Sys.State.Managers;

[PrivateApi("internal use only")]
internal class AppMetadataManager(IAppIdentity appIdentity, ICacheExpiring cacheExpiry)
    : IMetadataSource, IHasMetadataSourceAndExpiring
{
    #region cache value objects: Types, _guid, _number, _string

    /// <summary>
    /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
    /// </summary>
    private Dictionary<int, Dictionary<Guid, List<IEntity>>> _guid = [];

    /// <summary>
    /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
    /// </summary>
    private Dictionary<int, Dictionary<int, List<IEntity>>> _number = [];

    /// <summary>
    /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
    /// </summary>
    private Dictionary<int, Dictionary<string, List<IEntity>>> _string = [];

    #endregion

    #region IAppIdentity

    public int ZoneId => appIdentity.ZoneId;

    public int AppId => appIdentity.AppId;


    #endregion

    /// <summary>
    /// Reset all indexes
    /// </summary>
    internal void Reset()
    {
        _guid = [];
        _number = [];
        _string = [];
    }

    #region Cache Timestamp & Invalidation

    public long CacheTimestamp => cacheExpiry.CacheTimestamp;
    public bool CacheChanged(long dependentTimeStamp) => cacheExpiry.CacheChanged(dependentTimeStamp);

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
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            AddToMetaDic(_string, md.TargetType, md.KeyString, entity, add);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }


    private static void AddToMetaDic<T>(IDictionary<int, Dictionary<T, List<IEntity>>> metadataIndex, int mdTargetType, T mdValue, IEntity entity, bool add)
        where T : notnull
    {
        // Get or create the metadata index on this targetType
        if (!metadataIndex.TryGetValue(mdTargetType, out var indexOfType))
            indexOfType = metadataIndex[mdTargetType] = new();

        // Ensure that the assignment type (like 4) the target guid (like a350320-3502-afg0-...) exists, otherwise create empty list
        var list = indexOfType.TryGetValue(mdValue, out var lst) ? lst : indexOfType[mdValue] = [];

        // in case it was already in this index, remove first
        var found = list.One(entity.EntityId);
        if (found != null)
            list.Remove(found);

        // Now all containers must exist, add this item
        if (add) 
            list.Add(entity);
    }

    /// <inheritdoc />
    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string? contentTypeName = null)
    {
        if(key == null)
            return [];
        var type = typeof(TMetadataKey);
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(Guid))
            return Lookup(_guid, targetType, key as Guid? ?? Guid.Empty, contentTypeName);

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Int32 => Lookup(_number, targetType, key as int? ?? 0, contentTypeName),
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            TypeCode.String => Lookup(_string, targetType, key as string, contentTypeName),
            _ => Lookup(_string, targetType, key.ToString(), contentTypeName)
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        };
    }

    public IEnumerable<IEntity> GetMetadata<T>(TargetTypes targetType, T key, string? contentTypeName = null) 
        => GetMetadata((int)targetType, key, contentTypeName);

    private static IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, List<IEntity>>> list, int targetType, T key, string? contentTypeName)
        where T : notnull
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        if (list.TryGetValue(targetType, out var keyDict)
            && keyDict.TryGetValue(key, out var entities))
            return contentTypeName == null
                ? entities
                : entities.Where(e => e.Type.Is(contentTypeName));
        return [];
    }

    public IMetadataSource MetadataSource => this;

    public IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, NoParamOrder protector, string? title = null)
        => new MetadataOf<T>((int)targetType, key, title ?? "title not known", appSource: this);
}
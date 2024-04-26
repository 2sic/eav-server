using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

[PrivateApi]
// ReSharper disable once InconsistentNaming
public static partial class IEntityExtensions
{
#if DEBUG
    private static int countOneId;
    private static int countOneGuid;
    private static int countOneHas;
    private static int countOneRepo;
    private static int countOneOfContentType;

    internal static int CountOneIdOpt;
    internal static int CountOneRepoOpt;
    internal static int CountOneHasOpt;
    internal static int CountOneGuidOpt;
    internal static int countOneOfContentTypeOpt;
#endif
    /// <summary>
    /// Get an entity with an entity-id - or null if not found
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity One(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        countOneId++;
#endif
        return list is ImmutableSmartList fastList
            ? fastList.Fast.Get(id)
            : list.FirstOrDefault(e => e.EntityId == id);
    }

    /// <summary>
    /// get an entity based on the guid - or null if not found
    /// </summary>
    /// <param name="list"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity One(this IEnumerable<IEntity> list, Guid guid)
    {
#if DEBUG
        countOneGuid++;
#endif
        return list is ImmutableSmartList fastList
            ? fastList.Fast.Get(guid)
            : list.FirstOrDefault(e => e.EntityGuid == guid);
    }


    /// <summary>
    /// Get an entity based on the repo-id - mainly used for internal APIs and saving/versioning
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity FindRepoId(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        countOneRepo++;
#endif
        return list is ImmutableSmartList fastList
            ? fastList.Fast.GetRepo(id)
            : list.FirstOrDefault(e => e.RepositoryId == id);
    }

    /// <summary>
    /// Check if an entity is available. 
    /// Mainly used in special cases where published/unpublished are hidden/visible
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool Has(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        countOneHas++;
#endif
        return list is ImmutableSmartList fastList
            ? fastList.Fast.Has(id)
            : list.Any(e => e.EntityId == id || e.RepositoryId == id);
    }


    // Todo #OptimizeOfType

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEnumerable<IEntity> OfType(this IEnumerable<IEntity> list, IContentType type)
        => list.Where(e => type.Equals(e.Type));

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEnumerable<IEntity> OfType(this IEnumerable<IEntity> list, string typeName)
    {
#if DEBUG
        countOneOfContentType++;
#endif            
        return list is ImmutableSmartList fastList
            ? fastList.Fast.OfType(typeName)
            : list.Where(e => e.Type.Is(typeName));
    }

    public static IEntity IfOfType(this IEntity entity, string typeName) 
        => entity.Type.Is(typeName) ? entity : null;


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Dictionary<string, object> AsDictionary(this IEntity entity)
    {
        var attributes = entity.Attributes.ToDictionary(k => k.Value.Name, v => v.Value[0]);
        attributes.Add(Attributes.EntityIdPascalCase, entity.EntityId);
        attributes.Add(Attributes.EntityGuidPascalCase, entity.EntityGuid);
        return attributes;
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity KeepOrThrowIfInvalid(this IEntity item, string contentType, object identifier)
    {
        if (item == null || contentType != null && !item.Type.Is(contentType))
            throw new KeyNotFoundException($"Can't find {identifier} of type '{contentType}'");
        return item;
    }
}
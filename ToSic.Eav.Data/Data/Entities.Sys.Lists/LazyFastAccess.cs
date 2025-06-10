﻿using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace ToSic.Eav.Data.Entities.Sys.Lists;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyFastAccess(IImmutableList<IEntity> list)
{
    public IEntity Get(int id)
    {
#if DEBUG
        IEntityExtensions.CountOneIdOpt++;
#endif
        // Check if ID was already cached
        if (_byInt.TryGetValue(id, out var result))
            return result;

        // If not, search in the list and cache the result
        result = _list.FirstOrDefault(e => e.EntityId == id);
        _byInt.TryAdd(id, result);
        return result;
    }
    public IEntity GetRepo(int id)
    {
#if DEBUG
        IEntityExtensions.CountOneRepoOpt++;
#endif
        // Check if RepositoryId was already cached
        if (_byRepoId.TryGetValue(id, out var result))
            return result;
        // If not, search in the list and cache the result
        result = _list.FirstOrDefault(e => e.RepositoryId == id);
        _byRepoId.TryAdd(id, result);
        return result;
    }

    public bool Has(int id)
    {
#if DEBUG
        IEntityExtensions.CountOneHasOpt++;
#endif
        if (_has.TryGetValue(id, out var result))
            return result;
        var found = Get(id) ?? GetRepo(id);
        var status = found != null;
        _has.TryAdd(id, status);
        return status;
    }

    public IEntity Get(Guid id)
    {
#if DEBUG
        IEntityExtensions.CountOneGuidOpt++;
#endif
        if (_byGuid.TryGetValue(id, out var result))
            return result;
        result = _list.FirstOrDefault(e => e.EntityGuid == id);
        _byGuid.TryAdd(id, result);
        return result;
    }

    public IImmutableList<IEntity> OfType(string name)
    {
#if DEBUG
        IEntityExtensions.CountOneOfContentTypeOpt++;
#endif
        if (_ofType.TryGetValue(name, out var found))
            return found;

        var newEntry = _list
            .Where(e => e.Type.Is(name))
            .ToImmutableOpt();
        _ofType.TryAdd(name, newEntry);
        return newEntry;
    }

    private readonly IEnumerable<IEntity> _list = list;

    private readonly ConcurrentDictionary<int, IEntity> _byInt = new();
    private readonly ConcurrentDictionary<int, IEntity> _byRepoId = new();
    private readonly ConcurrentDictionary<Guid, IEntity> _byGuid = new();
    private readonly ConcurrentDictionary<int, bool> _has = new();

    private readonly ConcurrentDictionary<string, IImmutableList<IEntity>> _ofType =
        new(StringComparer.InvariantCultureIgnoreCase);
}
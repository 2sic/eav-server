using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ToSic.Eav.Data
{
    public class LazyFastAccess
    {
        public LazyFastAccess(IImmutableList<IEntity> list) => _list = list;

        public IEntity Get(int id)
        {
#if DEBUG
            IEntityExtensions.CountOneIdOpt++;
#endif
            if (_byInt.TryGetValue(id, out var result)) return result;
            result = _list.FirstOrDefault(e => e.EntityId == id);
            _byInt.TryAdd(id, result);
            return result;
        }
        public IEntity GetRepo(int id)
        {
#if DEBUG
            IEntityExtensions.CountOneRepoOpt++;
#endif
            if (_byRepoId.TryGetValue(id, out var result)) return result;
            result = _list.FirstOrDefault(e => e.RepositoryId == id);
            _byRepoId.TryAdd(id, result);
            return result;
        }

        public bool Has(int id)
        {
#if DEBUG
            IEntityExtensions.CountOneHasOpt++;
#endif
            if (_has.TryGetValue(id, out var result)) return result;
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
            if (_byGuid.TryGetValue(id, out var result)) return result;
            result = _list.FirstOrDefault(e => e.EntityGuid == id);
            _byGuid.TryAdd(id, result);
            return result;
        }

        public IImmutableList<IEntity> OfType(string name)
        {
#if DEBUG
            IEntityExtensions.countOneOfContentTypeOpt++;
#endif
            if (_ofType.TryGetValue(name, out var found)) return found;

            var newEntry = _list.Where(e => e.Type.Is(name)).ToImmutableList();
            _ofType.TryAdd(name, newEntry);
            return newEntry;
        }

        private readonly IEnumerable<IEntity> _list;

        private readonly ConcurrentDictionary<int, IEntity> _byInt = new();
        private readonly ConcurrentDictionary<int, IEntity> _byRepoId = new();
        private readonly ConcurrentDictionary<Guid, IEntity> _byGuid = new();
        private readonly ConcurrentDictionary<int, bool> _has = new();

        private readonly ConcurrentDictionary<string, IImmutableList<IEntity>> _ofType =
            new(StringComparer.InvariantCultureIgnoreCase);
    }
}

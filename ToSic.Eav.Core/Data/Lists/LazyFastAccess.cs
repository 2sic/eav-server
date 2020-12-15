using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data
{
    public class LazyFastAccess
    {
        public LazyFastAccess(IEnumerable<IEntity> list) => _list = list;

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

        private readonly IEnumerable<IEntity> _list;

        private readonly ConcurrentDictionary<int, IEntity> _byInt = new ConcurrentDictionary<int, IEntity>();
        private readonly ConcurrentDictionary<int, IEntity> _byRepoId = new ConcurrentDictionary<int, IEntity>();
        private readonly ConcurrentDictionary<Guid, IEntity> _byGuid = new ConcurrentDictionary<Guid, IEntity>();
        private readonly ConcurrentDictionary<int, bool> _has = new ConcurrentDictionary<int, bool>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data
{
    public class LazyFastAccess
    {
        public LazyFastAccess(IEnumerable<IEntity> list) => _list = list;

        public IEntity Get(int id)
        {
            if (_byInt.TryGetValue(id, out var result)) return result;
            result = _list.FirstOrDefault(e => e.EntityId == id);
            _byInt.Add(id, result);
            return result;
        }
        public IEntity GetRepo(int id)
        {
            if (_byRepoId.TryGetValue(id, out var result)) return result;
            result = _list.FirstOrDefault(e => e.RepositoryId == id);
            _byRepoId.Add(id, result);
            return result;
        }

        public bool Has(int id)
        {
            if (_has.TryGetValue(id, out var result)) return result;
            var found = Get(id) ?? GetRepo(id);
            var status = found != null;
            _has.Add(id, status);
            return status;
        }

        public IEntity Get(Guid id)
        {
            if (_byGuid.TryGetValue(id, out var result)) return result;
            result = _list.FirstOrDefault(e => e.EntityGuid == id);
            _byGuid.Add(id, result);
            return result;
        }

        private readonly IEnumerable<IEntity> _list;

        private readonly Dictionary<int, IEntity> _byInt = new Dictionary<int, IEntity>();
        private readonly Dictionary<int, IEntity> _byRepoId = new Dictionary<int, IEntity>();
        private readonly Dictionary<Guid, IEntity> _byGuid = new Dictionary<Guid, IEntity>();
        private readonly Dictionary<int, bool> _has = new Dictionary<int, bool>();
    }
}

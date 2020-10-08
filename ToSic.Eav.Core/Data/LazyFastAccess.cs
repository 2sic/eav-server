using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    public class LazyFastAccess
    {
        public LazyFastAccess(IEnumerable<IEntity> list)
        {
            _list = list;
        }

        public IEntity Get(int id)
        {
            if (_dictionary.TryGetValue(id, out var result))
                return result;
            result = _list.One(id);
            _dictionary.Add(id, result);
            return result;
        }

        private readonly IEnumerable<IEntity> _list;

        private readonly Dictionary<int, IEntity> _dictionary = new Dictionary<int, IEntity>();
    }
}

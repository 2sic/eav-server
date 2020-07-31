using System.Collections.Generic;
using System.Linq;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ItemListActions
{
    public class Move : IItemListAction
    {
        private int _indexFrom, _indexTo;
        public Move(int from, int to)
        {
            _indexFrom = from;
            _indexTo = to;
        }
        public List<IEntity> Change(List<IEntity> ids)
        {
            if (_indexFrom >= ids.Count) // this is if you set cut after the last item
                _indexFrom = ids.Count - 1;
            if (_indexTo >= ids.Count)
                _indexTo = ids.Count;
            if (_indexFrom == _indexTo)
                return null;// false;

            // first copy, so we don't touch the original object
            ids = ids.ToList();

            // do actualy re-ordering
            var oldId = ids[_indexFrom];
            ids.RemoveAt(_indexFrom);
            if (_indexTo > _indexFrom) _indexTo--; // the actual index could have shifted due to the removal
            ids.Insert(_indexTo, oldId);
            return ids;// true;

        }
    }
}

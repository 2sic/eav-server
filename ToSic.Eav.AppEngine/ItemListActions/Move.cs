using System.Collections.Generic;
using ToSic.Eav.Apps.Interfaces;

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
        public bool Change(List<int?> ids)
        {
            if (_indexFrom >= ids.Count) // this is if you set cut after the last item
                _indexFrom = ids.Count - 1;
            if (_indexTo >= ids.Count)
                _indexTo = ids.Count;
            if (_indexFrom == _indexTo)
                return false;

            // do actualy re-ordering
            var oldId = ids[_indexFrom];
            ids.RemoveAt(_indexFrom);
            if (_indexTo > _indexFrom) _indexTo--; // the actual index could have shifted due to the removal
            ids.Insert(_indexTo, oldId);
            return true;

        }
    }
}

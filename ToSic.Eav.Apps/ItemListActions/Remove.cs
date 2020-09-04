using System.Collections.Generic;
using System.Linq;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ItemListActions
{
    public class Remove: IItemListAction
    {
        private readonly int _index;
        public Remove(int index)
        {
            _index = index;
        }
        public List<IEntity> Change(List<IEntity> ids)
        {
            // don't allow rmove outside of index
            if (_index < 0 || _index >= ids.Count)
                return null;// false;

            // first copy, so we don't touch the original object
            ids = ids.ToList();

            // do actualy re-ordering
            ids.RemoveAt(_index);
            return ids;// true;

        }
    }
}

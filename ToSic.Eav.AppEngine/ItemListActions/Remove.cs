using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.ItemListActions
{
    public class Remove: IItemListAction
    {
        private readonly int _index;
        public Remove(int index)
        {
            _index = index;
        }
        public bool Change(List<int?> ids)
        {
            // don't allow rmove outside of index
            if (_index < 0 || _index >= ids.Count)
                return false;

            // do actualy re-ordering
            ids.RemoveAt(_index);
            return true;

        }
    }
}

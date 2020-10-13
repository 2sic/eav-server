using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.Caching
{
    public class ImmutableSmartList: IImmutableList<IEntity> // , IImmutableListWithFastFind
    {
        public IImmutableList<IEntity> Contents;

        public ImmutableSmartList(IImmutableList<IEntity> contents)
            => Contents = contents;

        #region Smart bits / performance

        public IEntity One(int id) => LazyFastAccess.Get(id);
        public IEntity One(Guid id) => LazyFastAccess.Get(id);

        protected LazyFastAccess LazyFastAccess => _lfa ?? (_lfa = new LazyFastAccess(Contents));
        private LazyFastAccess _lfa;


        #endregion

        #region General interface support for IList<T> / IImmutableList<T>




        public IEnumerator<IEntity> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Contents).GetEnumerator();
        }

        public int Count => Contents.Count;

        public IEntity this[int index] => Contents[index];

        public IImmutableList<IEntity> Clear()
        {
            return Contents.Clear();
        }

        public int IndexOf(IEntity item, int index, int count, IEqualityComparer<IEntity> equalityComparer)
        {
            return Contents.IndexOf(item, index, count, equalityComparer);
        }

        public int LastIndexOf(IEntity item, int index, int count, IEqualityComparer<IEntity> equalityComparer)
        {
            return Contents.LastIndexOf(item, index, count, equalityComparer);
        }

        public IImmutableList<IEntity> Add(IEntity value)
        {
            return Contents.Add(value);
        }

        public IImmutableList<IEntity> AddRange(IEnumerable<IEntity> items)
        {
            return Contents.AddRange(items);
        }

        public IImmutableList<IEntity> Insert(int index, IEntity element)
        {
            return Contents.Insert(index, element);
        }

        public IImmutableList<IEntity> InsertRange(int index, IEnumerable<IEntity> items)
        {
            return Contents.InsertRange(index, items);
        }

        public IImmutableList<IEntity> Remove(IEntity value, IEqualityComparer<IEntity> equalityComparer)
        {
            return Contents.Remove(value, equalityComparer);
        }

        public IImmutableList<IEntity> RemoveAll(Predicate<IEntity> match)
        {
            return Contents.RemoveAll(match);
        }

        public IImmutableList<IEntity> RemoveRange(IEnumerable<IEntity> items, IEqualityComparer<IEntity> equalityComparer)
        {
            return Contents.RemoveRange(items, equalityComparer);
        }

        public IImmutableList<IEntity> RemoveRange(int index, int count)
        {
            return Contents.RemoveRange(index, count);
        }

        public IImmutableList<IEntity> RemoveAt(int index)
        {
            return Contents.RemoveAt(index);
        }

        public IImmutableList<IEntity> SetItem(int index, IEntity value)
        {
            return Contents.SetItem(index, value);
        }

        public IImmutableList<IEntity> Replace(IEntity oldValue, IEntity newValue, IEqualityComparer<IEntity> equalityComparer)
        {
            return Contents.Replace(oldValue, newValue, equalityComparer);
        }
        #endregion

    }
}

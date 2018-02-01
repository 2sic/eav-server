using System;
using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Environment
{
    public abstract class InstanceInfo<T>: IInstanceInfo
    {
        public T Info { get; }

        protected InstanceInfo(T item)
        {
            Info = item;
        }

        public abstract int Id { get; }

        public abstract int PageId { get; }
    }
}

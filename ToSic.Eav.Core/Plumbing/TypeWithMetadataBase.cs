using System;
using System.Linq;

namespace ToSic.Eav.Plumbing
{
    public abstract class TypeWithMedataBase<T> where T: class
    {
        public TypeWithMedataBase(Type dsType)
        {
            Type = dsType;

            // must put this in a try/catch, in case other DLLs have incompatible attributes
            try
            {
                TypeMetadata = Type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            }
            catch {  /*ignore */ }
        }

        public abstract string Name { get; }

        public Type Type { get; }

        public T TypeMetadata { get; }

    }
}

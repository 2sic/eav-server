using System;

namespace ToSic.Eav.Plumbing;

public abstract class TypeWithMetadataBase<T> where T: class
{
    protected TypeWithMetadataBase(Type dsType)
    {
        Type = dsType;

        // must put this in a try/catch, in case other DLLs have incompatible attributes
        try
        {
            TypeMetadata = dsType.GetDirectlyAttachedAttribute<T>();
        }
        catch {  /*ignore */ }
    }

    public abstract string Name { get; }

    public Type Type { get; }

    public T TypeMetadata { get; }

}
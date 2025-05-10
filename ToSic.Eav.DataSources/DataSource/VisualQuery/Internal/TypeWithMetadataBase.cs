using ToSic.Eav.Plumbing;
using ToSic.Lib.Data;

namespace ToSic.Eav.DataSource.VisualQuery.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class TypeWithMetadataBase<T> : IHasIdentityNameId
    where T: class
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

    public abstract string NameId { get; }

    public Type Type { get; }

    public T TypeMetadata { get; }

}
using ToSic.Eav.Models;

namespace ToSic.Eav.Metadata;

[PrivateApi]
public static class HasMetadataExtensions
{
    public static TModel? TryGetMetadata<TModel>(
        this IHasMetadata parent,
        NoParamOrder npo = default,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class
    {
        return parent.Metadata.First<TModel>(nullHandling: nullHandling);
    }
}

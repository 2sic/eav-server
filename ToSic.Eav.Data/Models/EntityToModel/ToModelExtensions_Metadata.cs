using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models;

[WorkInProgressApi("beta v21")]
public static partial class ToModelExtensions
{
    /// <summary>
    /// Get a typed metadata from an object which has metadata. Will return `null` if no data found.
    /// </summary>
    /// <typeparam name="TModel">Any model object or interface.</typeparam>
    /// <param name="parent">An object which has metadata.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <returns></returns>
    public static TModel? GetMetadata<TModel>(
        this IHasMetadata parent,
        NoParamOrder npo = default
        //ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelFromEntity
    {
        return parent.Metadata.First<TModel>(/*nullHandling: nullHandling*/);
    }
}

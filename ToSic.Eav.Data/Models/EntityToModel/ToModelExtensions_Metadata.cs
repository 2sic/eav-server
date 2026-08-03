using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models;

[WorkInProgressApi("beta v21")]
public static partial class ToModelExtensions
{
    /// <summary>
    /// Get a typed metadata from an object which has metadata.
    /// Will return `null` if no data found.
    /// </summary>
    /// <typeparam name="TModel">Any model object or interface.</typeparam>
    /// <param name="parent">An object which has metadata.</param>
    /// <returns></returns>
    public static TModel? GetMetadataModel<TModel>(this IHasMetadata parent)
        where TModel : class, IModelFromEntity
        => parent.Metadata.FirstModel<TModel>();


    /// <summary>
    /// Get a typed metadata from an object which has metadata.
    /// Will return `null` if no data found, unless specified otherwise in the options.
    /// </summary>
    /// <typeparam name="TModel">Any model object or interface.</typeparam>
    /// <param name="parent">An object which has metadata.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static TModel? GetMetadataModel<TModel>(
        this IHasMetadata parent,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
        => parent.Metadata.FirstModel<TModel>(options: options);
}

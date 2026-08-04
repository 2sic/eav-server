using ToSic.Eav.Metadata;
using ToSic.Eav.Models.Factory;
// ReSharper disable MethodOverloadWithOptionalParameter

namespace ToSic.Eav.Models;

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
        => FirstModelInternal<TModel>(parent.Metadata, null, null);

    
    
    /// <summary>
    /// Get a typed metadata from an object which has metadata.
    /// Will return `null` if no data found, unless specified otherwise in the options.
    /// </summary>
    /// <typeparam name="TModel">Any model object or interface.</typeparam>
    /// <param name="parent">An object which has metadata.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns></returns>
    public static TModel? GetMetadataModel<TModel>(this IHasMetadata parent, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => FirstModelInternal<TModel>(parent.Metadata, options: options, factory: null);
    
    
    
    public static TModel? GetMetadataModel<TModel>(this IHasMetadata parent, IModelFactory factory, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => FirstModelInternal<TModel>(parent.Metadata, options: options, factory: AssertFactory(factory));
}

using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Models.Factory;

/// <summary>
/// Generic simple wrapper factory which uses Dependency Injection to create wrappers.
/// </summary>
internal class ModelFactory(IServiceProvider sp): IModelFactory
{
    [return: NotNullIfNotNull(nameof(item))]
    public TModel? Create<TSource, TModel>(TSource? item, ToModelOptions options)
        where TModel : class, IModelFromEntity
        => ModelFactoryShared.CreateStatic<TSource, TModel>(sp, item, options);


    [return: NotNullIfNotNull("item")]
    public TCustom? AsCustomFrom<TCustom, TData>(TData? item, ModelSettings? settings = default) where TCustom : class, IModelFromData
    {
        throw new NotImplementedException($"This is only available once you get into 2sxc; EAV does not support it.");
    }
}

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ModelFactoryShared
{
    [return: NotNullIfNotNull(nameof(item))]
    public static TModel? CreateStatic<TSource, TModel>(IServiceProvider sp, TSource? item, ToModelOptions options)
        where TModel : class, IModelFromEntity
    {
        var wrapper = sp.Build<TModel>();
        // New implementation with null checks
        var result = (wrapper as IModelSetup<TSource>)?.SetupWithNullChecks(item, options.NullHandling);
        return result as TModel;
    }
    
}
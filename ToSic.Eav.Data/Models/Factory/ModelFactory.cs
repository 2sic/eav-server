using ToSic.Eav.Models.Factory.Sys;
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
using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Models.Factory;

/// <summary>
/// Generic simple wrapper factory which uses Dependency Injection to create wrappers.
/// </summary>
internal class ModelFactory(IServiceProvider sp): IModelFactory
{
    [return: NotNullIfNotNull(nameof(source))]
    public TModel? Create<TSource, TModel>(TSource? source)
        where TModel : IModelFromEntity
    {
        var wrapper = sp.Build<TModel>();
        var ok = (wrapper as IModelSetup<TSource>)?.SetupModel(source) ?? false;
        return ok ? wrapper : default;
    }

    [return: NotNullIfNotNull("item")]
    public TCustom? AsCustomFrom<TCustom, TData>(TData? item, ModelSettings? settings = default) where TCustom : class, IModelFromData
    {
        throw new NotImplementedException($"This is only available once you get into 2sxc; EAV does not support it.");
    }
}

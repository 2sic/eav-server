namespace ToSic.Eav.Models.Factory;

/// <summary>
/// Generic simple wrapper factory which uses Dependency Injection to create wrappers.
/// </summary>
internal class ModelFactory(IServiceProvider sp): IModelFactory
{
    [return: NotNullIfNotNull(nameof(source))]
    public TModel? Create<TSource, TModel>(TSource? source)
        where TModel : IModelSetup<TSource>
    {
        var wrapper = sp.Build<TModel>();
        var ok = wrapper.SetupModel(source);
        return ok ? wrapper : default;
    }
}

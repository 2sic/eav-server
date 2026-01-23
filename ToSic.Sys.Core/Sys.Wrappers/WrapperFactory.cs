namespace ToSic.Sys.Wrappers;

/// <summary>
/// Generic simple wrapper factory which uses Dependency Injection to create wrappers.
/// </summary>
internal class WrapperFactory(IServiceProvider sp): IWrapperFactory
{
    [return: NotNullIfNotNull(nameof(source))]
    public TModel? Create<TSource, TModel>(TSource? source)
        where TModel : IWrapperSetup<TSource>
    {
        var wrapper = sp.Build<TModel>();
        var ok = wrapper.SetupContents(source);
        return ok ? wrapper : default;
    }
}

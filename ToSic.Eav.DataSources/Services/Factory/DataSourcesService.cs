using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Services;

internal class DataSourcesService(
    IServiceProvider serviceProvider,
    LazySvc<ILookUpEngineResolver> lookupResolveLazy)
    : ServiceBase($"{DataSourceConstants.LogPrefix}.Factry", connect: [/* never! serviceProvider,*/ lookupResolveLazy]),
        IDataSourcesService
{

    #region GetDataSource

    /// <inheritdoc />
    public IDataSource Create(Type type, IDataSourceLinkable attach = default, IDataSourceOptions options = default) => Log.Func(() =>
    {
        var newDs = serviceProvider.Build<IDataSource>(type, Log);
        newDs.Setup(options, attach);
        return newDs;
    });


    #endregion

    #region GetDataSource Typed

    /// <summary>
    /// Experimental 12.10+
    /// </summary>
    /// <typeparam name="TDataSource"></typeparam>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    [PrivateApi("internal, experimental, only used in tests ATM")]
    public TDataSource Create<TDataSource>(IDataStream stream, IDataSourceOptions options = default) where TDataSource : IDataSource
    {
        if (stream.Source == null)
            throw new("Unexpected source - stream without a real source. can't process; wip");
        var sourceDs = stream.Source;
        var ds = Create<TDataSource>(options: new DataSourceOptions.Converter().Create(new DataSourceOptions(appIdentity: sourceDs, lookUp: sourceDs.Configuration.LookUpEngine), options));
        ds.Attach(DataSourceConstants.StreamDefaultName, stream);
        return ds;
    }

    /// <inheritdoc />
    public TDataSource Create<TDataSource>(IDataSourceLinkable attach = default, IDataSourceOptions options = default) where TDataSource : IDataSource => Log.Func(() =>
    {
        var primarySource = attach?.Link?.DataSource;
        if (primarySource == null && options?.AppIdentityOrReader == null)
            throw new($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(attach)} and configuration.AppIdentity no not be null.");
        if (primarySource == null && options?.LookUp == null)
            options = OptionsWithLookUp(options);
        // throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(attach)} and configuration.LookUp no not be null.");

        var newDs = serviceProvider.Build<TDataSource>(Log);
        newDs.Setup(options, attach);
        return newDs;
    });

    #endregion

    #region Get Root Data Source with Publishing
        
    /// <inheritdoc />
    public IDataSource CreateDefault(IDataSourceOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (options.AppIdentityOrReader == null)
            throw new ArgumentNullException(nameof(IDataSourceOptions.AppIdentityOrReader));
        var l = Log.Fn<IDataSource>($"#{options.AppIdentityOrReader.Show()}, draft:{options.ShowDrafts}, lookUp:{options.LookUp != null}");

        options = OptionsWithLookUp(options);
        var appRoot = Create<IAppRoot>(options: options);
        var publishingFilter = Create<PublishingFilter>(attach: appRoot, options: options);

        if (options.ShowDrafts != null)
            publishingFilter.ShowDrafts = options.ShowDrafts;

        return l.Return(publishingFilter, "ok");
    }

    private IDataSourceOptions OptionsWithLookUp(IDataSourceOptions optionsOrNull)
    {
        return optionsOrNull?.LookUp != null 
            ? optionsOrNull 
            : new DataSourceOptions(optionsOrNull, lookUp: lookupResolveLazy.Value.GetLookUpEngine(0));
    }

    #endregion
}
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp.Sys.Engines;

namespace ToSic.Eav.Services;

internal class DataSourcesService(
    IServiceProvider serviceProvider,
    LazySvc<ILookUpEngineResolver> lookupResolveLazy)
    : ServiceBase($"{DataSourceConstantsInternal.LogPrefix}.Factry", connect: [/* never! serviceProvider,*/ lookupResolveLazy]),
        IDataSourcesService
{

    #region GetDataSource

    /// <inheritdoc />
    public IDataSource Create(Type type, IDataSourceOptions? options = default)
    {
        var l = Log.Fn<IDataSource>();
        var newDs = serviceProvider.Build<IDataSource>(type, Log);
        newDs.Setup(options ?? DataSourceOptions.Empty());
        return l.Return(newDs);
    }

    /// <inheritdoc />
    [Obsolete("making this obsolete in v21; believe it's almost never used outside of 2sxc/eav")]
    public IDataSource Create(Type type, IDataSourceLinkable? attach = default, IDataSourceOptions? options = default)
        => Create(type, options.WithAttach(attach));

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
    public TDataSource Create<TDataSource>(IDataStream stream, IDataSourceOptions? options = default)
        where TDataSource : IDataSource
    {
        if (stream.Source == null)
            throw new("Unexpected source - stream without a real source. can't process; wip");
        var sourceDs = stream.Source;
        var ds = Create<TDataSource>(options: new DataSourceOptionConverter().Create(new DataSourceOptions
        {
            AppIdentityOrReader = sourceDs,
            LookUp = sourceDs.Configuration.LookUpEngine,
        }, options));
        ds.Attach(DataSourceConstants.StreamDefaultName, stream);
        return ds;
    }

    /// <inheritdoc />
    public TDataSource Create<TDataSource>(IDataSourceOptions? options = default)
        where TDataSource : IDataSource
    {
        var l = Log.Fn<TDataSource>($"{typeof(TDataSource).Name}, attach:{options?.Attach?.GetLink()?.DataSource?.Show()}");

        var primarySource = options?.Attach?.GetLink()?.DataSource;

        if (primarySource == null && options?.AppIdentityOrReader == null)
            throw new($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(options.Attach)} and configuration.AppIdentity no not be null.");
        if (primarySource == null && options?.LookUp == null)
            options = OptionsWithLookUp(options);

        var newDs = serviceProvider.Build<TDataSource>(Log);
        newDs.Setup(options ?? DataSourceOptions.Empty());
        return l.Return(newDs);
    }

    /// <inheritdoc />
    [Obsolete("v21")]
    public TDataSource Create<TDataSource>(IDataSourceLinkable? attach = default, IDataSourceOptions? options = default)
        where TDataSource : IDataSource =>
        Create<TDataSource>(options.WithAttach(attach));

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

        var optionsSafe = OptionsWithLookUp(options);
        var appRoot = Create<IAppRoot>(options: optionsSafe);
        var publishingFilter = Create<PublishingFilter>(/*attach: appRoot,*/ options: optionsSafe with { Attach = appRoot });

        if (optionsSafe.ShowDrafts != null)
            publishingFilter.ShowDrafts = optionsSafe.ShowDrafts;

        return l.Return(publishingFilter, "ok");
    }

    private DataSourceOptions OptionsWithLookUp(IDataSourceOptions? optionsOrNull) =>
        optionsOrNull is not DataSourceOptions typed
            ? new()
            {
                AppIdentityOrReader = null, // #WipAppIdentityOrReader must become not null
                LookUp = lookupResolveLazy.Value.GetLookUpEngine(0),
            }
            : typed.LookUp == null
                ? typed with { LookUp = lookupResolveLazy.Value.GetLookUpEngine(0) }
                : typed;

    #endregion
}
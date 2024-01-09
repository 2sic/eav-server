using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Services;

internal class DataSourcesService: ServiceBase, IDataSourcesService
{
    #region Constructor / DI

    private readonly IServiceProvider _serviceProvider;
    private readonly LazySvc<ILookUpEngineResolver> _lookupResolveLazy;
        
    public DataSourcesService(IServiceProvider serviceProvider,
        LazySvc<ILookUpEngineResolver> lookupResolveLazy) : base($"{DataSourceConstants.LogPrefix}.Factry")
    {
        ConnectServices(
            _serviceProvider = serviceProvider,
            _lookupResolveLazy = lookupResolveLazy
        );
    }

    #endregion

    #region GetDataSource

    /// <inheritdoc />
    public IDataSource Create(Type type, IDataSourceLinkable attach = default, IDataSourceOptions options = default) => Log.Func(() =>
    {
        var newDs = _serviceProvider.Build<IDataSource>(type, Log);
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
            throw new Exception("Unexpected source - stream without a real source. can't process; wip");
        var sourceDs = stream.Source;
        var ds = Create<TDataSource>(options: new DataSourceOptions.Converter().Create(new DataSourceOptions(appIdentity: sourceDs, lookUp: sourceDs.Configuration.LookUpEngine), options));
        ds.Attach(DataSourceConstants.StreamDefaultName, stream);
        return ds;
    }

    /// <inheritdoc />
    public TDataSource Create<TDataSource>(IDataSourceLinkable attach = default, IDataSourceOptions options = default) where TDataSource : IDataSource => Log.Func(() =>
    {
        var primarySource = attach?.Link?.DataSource;
        if (primarySource == null && options?.AppIdentity == null)
            throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(attach)} and configuration.AppIdentity no not be null.");
        if (primarySource == null && options?.LookUp == null)
            options = OptionsWithLookUp(options);
        // throw new Exception($"{nameof(Create)}<{nameof(TDataSource)}> requires one or both of {nameof(attach)} and configuration.LookUp no not be null.");

        var newDs = _serviceProvider.Build<TDataSource>(Log);
        newDs.Setup(options, attach);
        return newDs;
    });

    #endregion

    #region Get Root Data Source with Publishing
        
    /// <inheritdoc />
    public IDataSource CreateDefault(IDataSourceOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        var appIdentity = options.AppIdentity
                          ?? throw new ArgumentNullException(nameof(IDataSourceOptions.AppIdentity));
        var l = Log.Fn<IDataSource>($"#{appIdentity.Show()}, draft:{options.ShowDrafts}, lookUp:{options.LookUp != null}");

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
            : new DataSourceOptions(optionsOrNull, lookUp: _lookupResolveLazy.Value.GetLookUpEngine(0));
    }

    #endregion
}
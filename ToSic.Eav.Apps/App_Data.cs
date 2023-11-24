using System;
using ToSic.Eav.DataSource;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Apps;

partial class App
{
    [PrivateApi]
    public ILookUpEngine ConfigurationProvider => _configurationProvider.Get(() => AppDataConfig.Configuration);
    private readonly GetOnce<ILookUpEngine> _configurationProvider = new();

    private IAppDataConfiguration AppDataConfig => _appDataConfigOnce.Get(() =>
    {
        // try deferred initialization of the configuration, 
        // this only works if on initialization a _dataConfigBuilder was provided
        if (_dataConfigurationBuilder == null) return null;

        var config = _dataConfigurationBuilder.Invoke(this);
        // needed to initialize data - must always happen a bit later because the show-draft info isn't available when creating the first App-object.
        // todo: later this should be moved to initialization of this object
        Log.A($"init data drafts:{config.ShowDrafts}, hasConf:{config.Configuration != null}");
        return config;

    });
    private readonly GetOnce<IAppDataConfiguration> _appDataConfigOnce = new();
    private Func<App, IAppDataConfiguration> _dataConfigurationBuilder;

    #region Data



    /// <inheritdoc />
    public IAppData Data => _data ??= BuildData();
    private IAppData _data;

    [PrivateApi]
    protected virtual IAppData BuildData() => Log.Func(l =>
    {
        if (ConfigurationProvider == null)
            throw new Exception("Cannot provide Data for the object App as crucial information is missing. " +
                                "Please call InitData first to provide this data.");

        // Note: ModulePermissionController does not work when indexing, return false for search
        var initialSource = Services.DataSourceFactory.CreateDefault(new DataSourceOptions(appIdentity: this, lookUp: ConfigurationProvider, showDrafts: AppDataConfig?.ShowDrafts));
        var appDataWithCreate = Services.DataSourceFactory.Create<AppData>(attach: initialSource);

        return appDataWithCreate;
    });

    #endregion
}
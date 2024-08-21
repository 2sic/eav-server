﻿using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Services;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources;

/// <summary>
/// All the data inside an App. <br/>
/// For example, it has a variable amount of Out-streams, one for each content-type in the app.
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "App",
    UiHint = "All data in an app with streams for type",
    Icon = DataSourceIcons.TableChart,
    Type = DataSourceType.Source,
    NameId = "ToSic.Eav.DataSources.App, ToSic.Eav.DataSources",
    DynamicOut = true,
    In = [DataSourceConstants.StreamDefaultName],
    ConfigurationType = "|Config ToSic.Eav.DataSources.App",
    HelpLink = "https://go.2sxc.org/DsApp")]
public partial class App : DataSourceBase
{
    #region Configuration-properties

    private const int NotConfigured = 0;

    /// <summary>
    /// Use this to re-target the app-source to another app. <br/>
    /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
    /// </summary>
    [Configuration(Fallback = NotConfigured)]
    public int AppSwitch
    {
        get => Configuration.GetThis(NotConfigured);
        set
        {
            Configuration.SetThisObsolete(value);
            AppId = value;
            Reset();
        }
    }

    /// <summary>
    /// Use this to re-target the app-source to another zone. <br/>
    /// Note that this can only be done before ever accessing the app - once the object has started reading data, switching has no more effect.
    /// </summary>
    [Configuration(Fallback = NotConfigured)]
    public int ZoneSwitch
    {
        get => Configuration.GetThis(NotConfigured);
        set
        {
            Configuration.SetThisObsolete(value);
            ZoneId = value;
            Reset();
        }
    }

    /// <summary>
    /// This is a very internal setting, not to be used publicly for now.
    /// It will cause the App to not just return its data, but also data from its ancestors.
    /// EG global data.
    /// We're still evaluating impact on performance, confusion of developers etc.
    /// </summary>
    /// <remarks>
    /// * Added in v15.04
    /// * Uses the[immutable convention](xref:NetCode.Conventions.Immutable).
    /// </remarks>
    [PrivateApi("WIP and not sure if this should ever become public")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Configuration(Fallback = false)]
    public bool WithAncestors => Configuration.GetThis(false);

    #endregion

    #region Constructor / DI


    public new class MyServices: MyServicesBase<Eav.DataSource.DataSourceBase.MyServices>
    {
        public IContextResolverUserPermissions UserPermissions { get; }
        public IDataSourcesService DataSourceFactory { get; }
        public IAppReaders AppReaders { get; }

        public MyServices(Eav.DataSource.DataSourceBase.MyServices parentServices,
            IAppReaders appReaders,
            IDataSourcesService dataSourceFactory,
            IContextResolverUserPermissions userPermissions) : base(parentServices)
        {
            ConnectLogs([
                AppReaders = appReaders,
                DataSourceFactory = dataSourceFactory,
                UserPermissions = userPermissions
            ]);
        }
    }

    /// <summary>
    /// Constructs a new App DataSource
    /// </summary>
    [PrivateApi]
    public App(MyServices services): base(services, $"{DataSourceConstants.LogPrefix}.EavApp")
    {
        _services = services;
        // this one is unusual, so don't pre-attach a default data stream to out
        _out = new(this, null);
    }

    private readonly MyServices _services;

    #endregion


    /// <summary>
    /// Attach a different data source than is currently attached...
    /// this is needed when a zone/app change
    /// </summary>
    private void AttachOtherDataSource()
    {
        // If something is done badly, we can easily get recursions
        if (_attachOtherDataSourceRunning) throw new("We have an unexpected recursion!");
        _attachOtherDataSourceRunning = true;
        // If we have zone/app switch, set not (they don't get updated if only the config is modified)
        // All not-set properties will use defaults 
        if (ZoneSwitch != 0) ZoneId = ZoneSwitch;
        if (AppSwitch != 0) AppId = AppSwitch;

        IDataSource appDs;

            
        // WIP / new
        if (WithAncestors)
        {
            Log.A("Will use Ancestors accessor with all ancestors");
            // Important: only pass the identity in, never pass this source in, or you'll get infinite recursions
            var appStack = _services.DataSourceFactory.Create<AppWithParents>(options: new DataSourceOptions(appIdentity: new AppIdentity(this), lookUp: Configuration.LookUpEngine));
            ((IAppIdentitySync)appStack).UpdateAppIdentity(this);
            appDs = appStack;
        }
        else
            appDs = _services.DataSourceFactory.CreateDefault(new DataSourceOptions(appIdentity: this, lookUp: Configuration.LookUpEngine));

        Attach(DataSourceConstants.StreamDefaultName, appDs);
        _attachOtherDataSourceRunning = false;
    }

    private bool _attachOtherDataSourceRunning = false;

    // 2024-01-09 2dm Removed for v17.01 - should have been removed a long time ago
    //[PrivateApi]
    //[Obsolete("Will probably be removed in v14")]
    //// TODO: cause obsolete warning when used! #Deprecated
    //public IMetadataSource Metadata => AppState;

    protected IAppState AppState => _appState.Get(() => _services.AppReaders.GetReader(this));
    private readonly GetOnce<IAppState> _appState = new();
}
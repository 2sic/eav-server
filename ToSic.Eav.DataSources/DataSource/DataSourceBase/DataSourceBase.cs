using ToSic.Eav.Apps;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSource;

/// <summary>
/// The base class for all DataSources, especially custom DataSources.
/// It must always be inherited.
/// It provides a lot of core functionality to get configurations, ensure caching and more.
///
/// Important: in most cases you will inherit the <see cref="CustomDataSource"/> DataSource for custom data sources.
/// </summary>
/// <remarks>
/// Had a major, breaking update in v15.
/// Consult the guide to upgrade your custom data sources.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice("Just FYI for people who need to know more about the internal APIs")]
public abstract partial class DataSourceBase : ServiceBase<DataSourceBase.MyServices>, IDataSource, IAppIdentitySync
{

    /// <summary>
    /// Default Constructor, _protected_.
    /// To inherit this, make sure your new class also gets the `MyServices` in it's constructor and passes it to here.
    /// </summary>
    /// <param name="services">All the needed services - see [](xref:NetCode.Conventions.MyServices)</param>
    /// <param name="logName">Your own log name, such as `My.CsvDs`</param>
    [PrivateApi]
    protected DataSourceBase(MyServices services, string logName, object[] connect = default) : base(services, logName, connect: connect)
    {
        AutoLoadAllConfigMasks(GetType());
    }
    [PrivateApi]
    protected DataSourceBase(MyServicesBase<MyServices> extendedServices, string logName) : base(extendedServices, logName)
    {
        AutoLoadAllConfigMasks(GetType());
    }

    /// <summary>
    /// Load all [Configuration] attributes and ensure we have the config masks.
    /// </summary>
    internal void AutoLoadAllConfigMasks(Type type)
    {
        // Load all config masks which are defined on attributes
        var configMasks = Services.ConfigDataLoader.GetTokens(type);
        configMasks.ForEach(cm => ConfigMask(cm.Key, cm.Token, cm.CacheRelevant));
    }


    /// <inheritdoc />
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public string Name => GetType().Name;

    [PrivateApi("internal use only - for labeling data sources in queries to show in debugging")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public string Label { get; private set; }

    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public virtual bool Immutable { get; private set; }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void DoWhileOverrideImmutable(Action action)
    {
        _overrideImmutable = true;
        action();
        _overrideImmutable = false;
    }

    private bool _overrideImmutable;

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void AddDebugInfo(Guid? guid, string label)
    {
        Guid = guid ?? Guid;
        Label = label ?? Label;
    }

    /// <inheritdoc />
    public virtual int AppId { get; protected set; }

    /// <inheritdoc />
    public virtual int ZoneId { get; protected set; }

    /// <inheritdoc />
    public Guid Guid { get; private set; }


    #region Error Handling

    [PublicApi]
    public DataSourceErrorHelper Error => _errorHandler.Get(() => Services.ErrorHandler.Value.Link(this));
    private readonly GetOnce<DataSourceErrorHelper> _errorHandler = new();

    #endregion

    void IAppIdentitySync.UpdateAppIdentity(IAppIdentity appIdentity)
    {
        AppId = appIdentity.AppId;
        ZoneId = appIdentity.ZoneId;
    }

}
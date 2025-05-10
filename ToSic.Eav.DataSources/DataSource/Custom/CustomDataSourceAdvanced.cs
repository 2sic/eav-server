using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSource.Internal.Configuration;

namespace ToSic.Eav.DataSource;

/// <inheritdoc />
/// <summary>
/// Base DataSource class for providing data from external sources.
/// This is the advanced base class which is more complex.
/// You will usually want to use the <see cref="CustomDataSource"/>.
/// </summary>
/// <remarks>
/// This has changed a lot in v15 (breaking change).
/// Read about it in the docs.
/// </remarks>
[PublicApi]
public abstract class CustomDataSourceAdvanced: DataSourceBase
{
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public new class MyServices: DataSourceBase.MyServices
    {
        [PrivateApi]
        public IDataFactory DataFactory { get; }

        [PrivateApi]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public MyServices(
            IDataSourceConfiguration configuration,
            LazySvc<DataSourceErrorHelper> errorHandler,
            ConfigurationDataLoader configDataLoader,
            LazySvc<IDataSourceCacheService> cacheService,
            IDataFactory dataFactory)
            : base(configuration, errorHandler, configDataLoader, cacheService)
        {
            DataFactory = ConnectService(dataFactory);
        }
    }

    /// <summary>
    /// Initializes an DataSource which will usually provide/generate external data.
    /// </summary>
    /// <param name="services">Dependencies needed by this data source and/or the parent</param>
    /// <param name="logName">
    /// The log name/identifier for insights logging.
    /// Optional, but makes debugging a bit easier when provided.
    /// </param>
    /// <remarks>
    /// set the cache creation date to the moment the object is constructed
    /// this is important, because the date should stay fixed throughout the lifetime of this object
    /// but renew when it is updates
    /// </remarks>
    protected CustomDataSourceAdvanced(MyServices services, string logName = null, object[] connect = null)
        : base(services, logName ?? $"{DataSourceConstantsInternal.LogPrefix}.Extern", connect: connect)
    {
        DataFactory = services.DataFactory;
    }

    protected CustomDataSourceAdvanced(MyServicesBase<MyServices> services, string logName = null)
        : base(services.ParentServices, logName ?? $"{DataSourceConstantsInternal.LogPrefix}.Extern", connect: [services])
    {
        DataFactory = services.ParentServices.DataFactory;
    }

    /// <inheritdoc />
    public override long CacheTimestamp { get; } = DateTime.Now.Ticks;  // Initialize with moment the object was created

    [PublicApi]
    protected IDataFactory DataFactory { get; }
}
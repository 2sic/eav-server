using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSource.Internal.Configuration;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    /// <summary>
    /// Services used by the <see cref="DataSourceBase"/>.
    /// This ensures that it's easy to inherit DataSources, while giving it all the services it needs even if the needs change with time.
    /// </summary>
    /// <remarks>
    /// * Added in v15.0x
    /// * Important: The internals of this class are not documented, as they will change with time.
    /// </remarks>
    [PrivateApi]
    public class MyServices(
        IDataSourceConfiguration configuration,
        LazySvc<DataSourceErrorHelper> errorHandler,
        ConfigurationDataLoader configDataLoader,
        LazySvc<IDataSourceCacheService> cacheService)
        : MyServicesBase(connect: [configuration, errorHandler, configDataLoader, cacheService])
    {
        public LazySvc<IDataSourceCacheService> CacheService { get; } = cacheService;
        public IDataSourceConfiguration Configuration { get; } = configuration;
        public ConfigurationDataLoader ConfigDataLoader { get; } = configDataLoader;
        public LazySvc<DataSourceErrorHelper> ErrorHandler { get; } = errorHandler;
    }
}
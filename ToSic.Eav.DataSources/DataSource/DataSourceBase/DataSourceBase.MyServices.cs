﻿using ToSic.Eav.DataSource.Internal.Caching;
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
    public class MyServices : MyServicesBase
    {
        public LazySvc<IDataSourceCacheService> CacheService { get; }
        public IDataSourceConfiguration Configuration { get; }
        public ConfigurationDataLoader ConfigDataLoader { get; }
        public LazySvc<DataSourceErrorHelper> ErrorHandler { get; }

        /// <summary>
        /// Note that we will use Generators for safety, because in rare cases the dependencies could be re-used to create a sub-data-source
        /// </summary>
        [PrivateApi]
        public MyServices(
            IDataSourceConfiguration configuration,
            LazySvc<DataSourceErrorHelper> errorHandler,
            ConfigurationDataLoader configDataLoader,
            LazySvc<IDataSourceCacheService> cacheService
        )
        {
            ConnectLogs([
                Configuration = configuration,
                ErrorHandler = errorHandler,
                ConfigDataLoader = configDataLoader,
                CacheService = cacheService
            ]);
        }
    }
}
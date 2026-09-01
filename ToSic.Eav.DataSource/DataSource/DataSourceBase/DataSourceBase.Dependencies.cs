using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Configuration;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    /// <summary>
    /// Services used by the <see cref="DataSourceBase"/>.
    /// This ensures that it's easy to inherit DataSources, while giving it all the services it needs even if the needs change with time.
    /// </summary>
    /// <remarks>
    /// * Added in v15.0x
    /// * The internals of this class are not documented, as they will change with time.
    /// * Up to v21 we believed that this must be a `class`, not a `record`,
    ///     because it needs to be inherited and extended by inheriting DataSources, and some platforms like DNN would not support records.
    /// * But in v22 we realized that derived classes would never need to inherit this,
    ///     since they can just add their own dependencies to their constructor, and the base class will still work.
    ///     So it was changed to be a `record` - which could be a breaking change.
    /// </remarks>
    [PrivateApi]
    public record Dependencies(
        IDataSourceConfiguration Configuration,
        LazySvc<DataSourceErrorHelper> ErrorHandler,
        ConfigurationDataLoader ConfigDataLoader,
        LazySvc<IDataSourceCacheService> CacheService)
        : DependenciesBase(connect: [Configuration, ErrorHandler, ConfigDataLoader, CacheService]);
}
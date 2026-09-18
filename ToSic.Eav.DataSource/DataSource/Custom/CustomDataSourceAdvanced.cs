using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Configuration;

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
[PrivateApi("Made private in v22, before was public. As of now, doesn't really serve a purpose any more...")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class CustomDataSourceAdvanced(CustomDataSourceAdvanced.Dependencies services, string? logName = null, object[]? connect = null)
    : DataSourceBase(services, logName ?? $"{DataSourceConstantsInternal.LogPrefix}.Extern", connect: connect)
{
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public new record Dependencies(
        IDataSourceConfiguration Configuration,
        LazySvc<DataSourceErrorHelper> ErrorHandler,
        ConfigurationDataLoader ConfigDataLoader,
        LazySvc<IDataSourceCacheService> CacheService)
        : DataSourceBase.Dependencies(Configuration, ErrorHandler, ConfigDataLoader, CacheService);


    /// <inheritdoc />
    /// <remarks>
    /// Set the cache creation date to the **moment the object is constructed**.
    /// This is important, because the date should stay fixed throughout the lifetime of this object
    /// but renew when it is updated
    /// </remarks>
    public override long CacheTimestamp { get; } = DateTime.Now.Ticks;  // Initialize with moment the object was created
}
﻿using ToSic.Eav.Data.Build;
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
[PublicApi]
public abstract class CustomDataSourceAdvanced: DataSourceBase
{
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public new class Dependencies: DataSourceBase.Dependencies
    {
        [PrivateApi]
        public IDataFactory DataFactory { get; }

        [PrivateApi]
        [ShowApiWhenReleased(ShowApiMode.Never)]
        public Dependencies(
            IDataSourceConfiguration configuration,
            LazySvc<DataSourceErrorHelper> errorHandler,
            ConfigurationDataLoader configDataLoader,
            LazySvc<IDataSourceCacheService> cacheService,
            IDataFactory dataFactory)
            : base(configuration, errorHandler, configDataLoader, cacheService)
        {
            ConnectLogs([dataFactory]);
            DataFactory = dataFactory;
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
    protected CustomDataSourceAdvanced(Dependencies services, string? logName = null, object[]? connect = null)
        : base(services, logName ?? $"{DataSourceConstantsInternal.LogPrefix}.Extern", connect: connect)
    {
        DataFactory = services.DataFactory;
    }

    /// <inheritdoc />
    public override long CacheTimestamp { get; } = DateTime.Now.Ticks;  // Initialize with moment the object was created

    [PublicApi]
    protected IDataFactory DataFactory { get; }
}
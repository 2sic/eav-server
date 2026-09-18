using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Eav.DataSource.Sys.Configuration;
using ToSic.Eav.DataSource.Sys.Errors;

namespace ToSic.Eav.DataSource;

/// <summary>
/// DataSource base for lightweight data sources which are quite simple and convention based.
/// </summary>
[PublicApi]
public class CustomDataSource: DataSourceBase
{
    /// <summary>
    /// The Services of <see cref="CustomDataSource"/> - explicitly implemented for API stability.
    /// </summary>
    /// <remarks>
    /// Note that it is the same as the base MyServices,
    /// but it's still important to have an own class.
    /// This is in case some day it will need more dependencies.
    /// Otherwise, compiled code would break when we need additional dependencies just for the CustomDataSource.
    /// </remarks>
    [PrivateApi]
    public new record Dependencies(
        IDataSourceConfiguration Configuration,
        LazySvc<DataSourceErrorHelper> ErrorHandler,
        ConfigurationDataLoader ConfigDataLoader,
        LazySvc<IDataSourceCacheService> CacheService,
        //IDataFactory DataFactory,
        // #DropSpawnNew
        // Note: This is not connected to the logs, could be an issue if we don't finish switching to ILogger
        // In that case, we would have to make a property an initialize, as was previously done in the base Dependencies record
        Generator<IDataFactory, DataFactoryOptions> DataFactoryGenerator
    )
        : DataSourceBase.Dependencies(Configuration, ErrorHandler, ConfigDataLoader, CacheService);//, DataFactory);

    /// <summary>
    /// Constructor for creating a Custom DataSource.
    /// </summary>
    /// <param name="services">All the needed services - see [](xref:NetCode.Conventions.Dependencies)</param>
    /// <param name="logName">Optional name for logging such as `My.JsonDS`</param>
    /// <param name="connect"></param>
    protected internal CustomDataSource(Dependencies services, string? logName = null, object[]? connect = null)
        : base(services, logName ?? "Ds.CustLt", connect: connect)
    {
        // Provide a default out, in case the overriding class doesn't
        ProvideOut(() => ConvertRaw(GetDefault, null));
    }

    /// <summary>
    /// Every new DataSource based on this is [immutable](xref:NetCode.Conventions.Immutable).
    /// </summary>
    public override bool Immutable => true;

    /// <summary>
    /// Default method called to return data.
    /// If the inheriting class overrides this, it can go without having a full constructor.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<IRawData> GetDefault() => [];

    [PrivateApi]
    protected internal void ProvideOutRaw<T>(
        Func<IEnumerable<T>> data,
        NoParamOrder npo = default,
        string name = DataSourceConstants.StreamDefaultName,
        Func<DataFactoryOptions>? options = default
    ) where T : class, IRawData
        => ProvideOut(() => ConvertRaw(data, options), name);

    /// <summary>
    /// Provide raw data which may instead contain an already prepared error stream.
    /// </summary>
    [PrivateApi]
    protected internal void ProvideOutRaw<T>(
        Func<ResultOrError<IEnumerable<T>>> data,
        NoParamOrder npo = default,
        string name = DataSourceConstants.StreamDefaultName,
        Func<DataFactoryOptions>? options = default
    ) where T : class, IRawData =>
        ProvideOut(() =>
            {
                var result = data();
                return result.IsOk
                    ? ConvertRaw(() => result.Result, options)
                    : result.ErrorsSafe();
            },
            name
        );

    internal IImmutableList<IEntity> ConvertRaw<T>(Func<IEnumerable<T>>? source, Func<DataFactoryOptions>? options)
        where T : class, IRawData
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // Get raw entities - from _source or from override method
        var raw = source?.Invoke()?.ToList();

        // If we didn't get anything, return empty
        if (raw.SafeNone())
            return l.Return([], "no items returned");

        // Transform result to IEntity
        var result = ((Dependencies)Services).DataFactoryGenerator
            .New(options: options?.Invoke() ?? new())
            .Create(raw);
        return l.Return(result, $"Got {result.Count} items");
    }
}
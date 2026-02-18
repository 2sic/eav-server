using ToSic.Eav.DataSource.Sys.Streams;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    #region Various provide-out commands - all public

    /// <summary>
    /// Provide a function to get the data which this DataSource offers.
    ///
    /// This is the more generic `IEnumerable` implementation.
    /// We recommend using the IImmutableList overload as it allows the system to optimize more.
    /// </summary>
    /// <param name="data">The function which will get the list.</param>
    /// <param name="name">_(optional)_ stream name, defaults to `Default`</param>
    [PublicApi]
    protected internal void ProvideOut(Func<IEnumerable<IEntity>> data, string name = DataSourceConstants.StreamDefaultName)
        => OutWritable.Add(name, new DataStream(Services.CacheService, this, name, data));

    /// <summary>
    /// Provide a function to get the data which this DataSource offers.
    ///
    /// This is the `ImmutableList` implementation, which is recommended.
    /// </summary>
    /// <param name="data">The function which will get the list.</param>
    /// <param name="name">_(optional)_ stream name, defaults to `Default`</param>
    [PublicApi]
    protected internal void ProvideOut(Func<IImmutableList<IEntity>> data, string name = DataSourceConstants.StreamDefaultName)
        => OutWritable.Add(name, new DataStream(Services.CacheService, this, name, data));

    #endregion


}
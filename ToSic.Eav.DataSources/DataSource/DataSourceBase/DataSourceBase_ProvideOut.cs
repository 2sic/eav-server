using ToSic.Eav.DataSource.Streams;
using ToSic.Eav.DataSource.Streams.Internal;

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
    /// <param name="getList">The function which will get the list.</param>
    /// <param name="name">_(optional)_ stream name, defaults to `Default`</param>
    [PublicApi]
    protected internal void ProvideOut(Func<IEnumerable<IEntity>> getList, string name = DataSourceConstants.StreamDefaultName)
        => OutWritable.Add(name, new DataStream(Services.CacheService, this, name, getList));

    /// <summary>
    /// Provide a function to get the data which this DataSource offers.
    ///
    /// This is the `ImmutableList` implementation, which is recommended.
    /// </summary>
    /// <param name="getList">The function which will get the list.</param>
    /// <param name="name">_(optional)_ stream name, defaults to `Default`</param>
    [PublicApi]
    protected internal void ProvideOut(Func<IImmutableList<IEntity>> getList, string name = DataSourceConstants.StreamDefaultName)
        => OutWritable.Add(name, new DataStream(Services.CacheService, this, name, getList));

    #endregion


}
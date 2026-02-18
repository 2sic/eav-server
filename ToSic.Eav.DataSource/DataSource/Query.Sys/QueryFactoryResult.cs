namespace ToSic.Eav.DataSource.Query.Sys;


/// <summary>
/// Structure to hold the result of building a query, containing the main data source and any other sources involved.
/// </summary>
/// <param name="Main">The primary data source resulting from the query. This is typically the main or exit point accessed by consumers.</param>
/// <param name="DataSources">A collection of all data sources produced by the query, keyed by their names.</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record QueryFactoryResult(IDataSource Main, Dictionary<string, IDataSource> DataSources);
namespace ToSic.Eav.DataSource.Query.Sys;


/// <summary>
/// Represents the result of a query, including the main data source and all associated data sources produced by the
/// query.
/// </summary>
/// <param name="Main">The primary data source resulting from the query. This is typically the main or exit point accessed by consumers.</param>
/// <param name="DataSources">A collection of all data sources produced by the query, keyed by their names.</param>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record QueryResult(IDataSource Main, Dictionary<string, IDataSource> DataSources);
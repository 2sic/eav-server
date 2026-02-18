namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Non-Generic Helpers to work with common Data Queries (not Typed Queries).
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryManager(QueryDefinitionService queryDefSvc, Generator<Query> queryGenerator)
    : QueryManager<Query>(queryDefSvc, queryGenerator);
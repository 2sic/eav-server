using ToSic.Eav.Apps;

namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// Non-Generic Helpers to work with common Data Queries (not Typed Queries).
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryManager(
    Generator<Query> queryGenerator,
    LazySvc<IAppReaderFactory> appReaders,
    LazySvc<QueryDefinitionBuilder> queryDefBuilder)
    : QueryManager<Query>(queryGenerator, appReaders, queryDefBuilder);
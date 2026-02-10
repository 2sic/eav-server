using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Tests;

internal static class ODataQueryEngineTestAccessors
{

    public static QueryExecutionResult ExecuteTac(this ODataQueryEngine engine, IDataSource root, Query query)
        => engine.Execute(root, query);
}

using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Eav.ImportExport.Tests.Persistence.File;


namespace ToSic.Eav.DataSourceTests.Query;

[TestClass]
[DeploymentItem("..\\..\\" + Eav.DataSourceTests.TestConfig.GlobalQueriesData, Eav.DataSourceTests.TestConfig.TestingPath)]
public class QueryGlobalTest: TestBaseDiEavFullAndDb
{
    private QueryBuilder QueryBuilder => field ??= GetService<QueryBuilder>();
    private QueryManager QueryManager => field ??= GetService<QueryManager>();
    private QueryDefinitionBuilder QueryDefinitionBuilder => field ??= GetService<QueryDefinitionBuilder>();


    private const int GlobalQueryCount = 15; // count in v15.03

    [ClassInitialize]
    public static void Config(TestContext context)
    {
        TestGlobalFolderRepository.PathToUse = Eav.DataSourceTests.TestConfig.TestingPath;
    }

    [TestMethod]
    public void FindGlobalQueries()
    {
        var queries = QueryManager.AllQueryItems(Constants.PresetIdentity);
        var count = queries.Count;
        IsTrue(count is >= GlobalQueryCount and <= GlobalQueryCount + 5, $"should find {GlobalQueryCount} +/-5 query definitions, found {queries.Count}");
    }


    [TestMethod]
    public void ReviewGlobalZonesQuery()
    {
        var queryName = $"{DataSourceConstantsInternal.SystemQueryPrefix}Zones";
        var queryEnt = QueryManager.FindQuery(Constants.PresetIdentity, queryName);
        AreEqual(queryName, queryEnt.GetTac<string>("Name"), "should find zones");

        var qdef = QueryDefinitionBuilder.Create(queryEnt, queryEnt.AppId);
        AreEqual(2, qdef.Parts.Count, "counting parts of the query definition, should have the zone + sort = 2 parts");
    }

    [TestMethod]
    public void UseGlobalZonesQuery()
    {
        var queryEnt = QueryManager.FindQuery(Constants.PresetIdentity, $"{DataSourceConstantsInternal.SystemQueryPrefixPreV15}Zones");

        var qDef = QueryDefinitionBuilder.Create(queryEnt, Eav.DataSourceTests.TestConfig.AppForQueryTests);

        var fac = QueryBuilder;
        var query = fac.GetDataSourceForTesting(qDef).Main;

        var list = query.ListTac();
        IsTrue(list.Count() > 1, "should find a few portals in the eav-testing-DB");
    }


}
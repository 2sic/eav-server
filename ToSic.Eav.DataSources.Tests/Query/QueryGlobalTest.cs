using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Eav.DataSources;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Data;

namespace ToSic.Eav.DataSourceTests.Query;

[TestClass]
[DeploymentItem("..\\..\\" + TestConfig.GlobalQueriesData, TestConfig.TestingPath)]
public class QueryGlobalTest: TestBaseDiEavFullAndDb
{
    public QueryGlobalTest()
    {
        _queryBuilder = GetService<QueryBuilder>();
        _queryManager = GetService<QueryManager>();
        _queryDefinitionBuilder = GetService<QueryDefinitionBuilder>();
    }

    private readonly QueryBuilder _queryBuilder;
    private readonly QueryManager _queryManager;
    private readonly QueryDefinitionBuilder _queryDefinitionBuilder;


    private const int GlobalQueryCount = 15; // count in v15.03

    [ClassInitialize]
    public static void Config(TestContext context)
    {
        TestGlobalFolderRepository.PathToUse = TestConfig.TestingPath;
    }

    [TestMethod]
    public void FindGlobalQueries()
    {
        var queries = _queryManager.AllQueryItems(Constants.PresetIdentity);
        var count = queries.Count;
        Assert.IsTrue(GlobalQueryCount <= count && count <= GlobalQueryCount + 5, $"should find {GlobalQueryCount} +/-5 query definitions, found {queries.Count}");
    }


    [TestMethod]
    public void ReviewGlobalZonesQuery()
    {
        var queryName = $"{DataSourceConstantsInternal.SystemQueryPrefix}Zones";
        var queryEnt = _queryManager.FindQuery(Constants.PresetIdentity, queryName);
        Assert.AreEqual(queryName, queryEnt.TacGet<string>("Name"), "should find zones");

        var qdef = _queryDefinitionBuilder.Create(queryEnt, queryEnt.AppId);
        Assert.AreEqual(2, qdef.Parts.Count, "counting parts of the qdef, should have the zone + sort = 2 parts");
    }

    [TestMethod]
    public void UseGlobalZonesQuery()
    {
        var queryEnt = _queryManager.FindQuery(Constants.PresetIdentity, $"{DataSourceConstantsInternal.SystemQueryPrefixPreV15}Zones");

        var qDef = _queryDefinitionBuilder.Create(queryEnt, TestConfig.AppForQueryTests);

        var fac = _queryBuilder;
        var query = fac.GetDataSourceForTesting(qDef).Main;

        var list = query.ListForTests();
        Assert.IsTrue(list.Count() > 1, "should find a few portals in the eav-testing-DB");
    }


}
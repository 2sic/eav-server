using ToSic.Eav.Apps.Sys;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSource.Sys;

namespace ToSic.Eav.DataSource.DbTests.Query;

[Startup(typeof(StartupTestFullWithDb))]
public class QueryGlobalTestJsonSerializer(
    QueryManager queryManager,
    QueryFactory queryFactory,
    QueryDefinitionBuilder queryDefinitionBuilder)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    public static int AppForQueryTests = 4;

    private const int GlobalQueryCount = 15; // count in v15.03

    [Fact]
    public void FindGlobalQueries()
    {
        var queries = queryManager.AllQueryEntities(KnownAppsConstants.PresetIdentity);
        var count = queries.Count;
        True(count is >= GlobalQueryCount and <= GlobalQueryCount + 5, $"should find {GlobalQueryCount} +/-5 query definitions, found {queries.Count}");
    }


    [Fact]
    public void ReviewGlobalZonesQuery()
    {
        var queryName = $"{DataSourceConstantsInternal.SystemQueryPrefix}Zones";
        var queryEnt = queryManager.TryGetQueryEntity(KnownAppsConstants.PresetIdentity, queryName);
        Equal(queryName, queryEnt.GetTac<string>("Name"));//, "should find zones");

        var qdef = queryDefinitionBuilder.Create(queryEnt.AppId, queryEnt);
        Equal(2, qdef.Parts.Count);//, "counting parts of the query definition, should have the zone + sort = 2 parts");
    }

    [Fact]
    public void UseGlobalZonesQuery()
    {
        var queryEnt = queryManager.TryGetQueryEntity(KnownAppsConstants.PresetIdentity, $"{DataSourceConstantsInternal.SystemQueryPrefix}Zones");

        var qDef = queryDefinitionBuilder.Create(AppForQueryTests, queryEnt);

        var fac = queryFactory;
        var query = fac.BuildWithTestParams(qDef).Main;

        var list = query.ListTac();
        True(list.Count() > 1, "should find a few portals in the eav-testing-DB");
    }

    [Fact]
    public void UseOldPrefixThrows()
    {
        Throws<Exception>(() => queryManager.TryGetQueryEntity(KnownAppsConstants.PresetIdentity, $"{DataSourceConstantsInternal.SystemQueryPrefixPreV15}Zones"));
    }

}
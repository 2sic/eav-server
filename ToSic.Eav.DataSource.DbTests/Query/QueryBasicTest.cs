using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.ImportExport.Json.Sys;

namespace ToSic.Eav.DataSource.DbTests.Query;

[Startup(typeof(StartupTestFullWithDb))]
public class QueryBasicTest(JsonSerializer jsonSerializer,
    QueryManager queryManager,
    QueryFactory queryFactory,
    QueryDefinitionBuilder queryDefinitionBuilder,
    IAppReaderFactory appReaderFactory)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    private static int AppForQueryTests = 4; //Eav.DataSourceTests.TestConfig.AppForQueryTests;

    private const int BasicId = 765;
    private const int BasicCount = 8;

    [Fact]
    public void LookForQuery_DeepApi()
    {
        var qdef = LoadQueryDef(AppForQueryTests, BasicId);
        var entity = (qdef as ICanBeEntity).Entity;
        NotNull(entity);

        var metadata = entity.Metadata.ToList();
        True(metadata.Count > 0);

    }

    private QueryDefinition LoadQueryDef(int appId, int queryId)
    {
        var appState = appReaderFactory.GetTac(appId);
        var pipelineEntity = queryManager.GetDefinition(appState, queryId);

        return pipelineEntity; // queryDefinitionBuilder.Create(appId, pipelineEntity);
    }



    [Fact]
    public void Query_To_Json()
    {
        var qdef = LoadQueryDef(AppForQueryTests, BasicId);
        var ser = jsonSerializer;
        var entity = (qdef as ICanBeEntity).Entity;
        var justHeader = ser.Serialize(entity, 0);
        var full = ser.Serialize(entity, 10);
        True(full.Length > justHeader.Length *2, "full serialized should be much longer");
        Trace.WriteLine("basic");
        Trace.WriteLine(justHeader);
        Trace.WriteLine("full");
        Trace.Write(full);
    }

    [Fact]
    public void Query_to_Json_and_back()
    {
        var qdef = LoadQueryDef(AppForQueryTests, BasicId);
        var ser = Serializer();
        var entity = (qdef as ICanBeEntity).Entity;
        var strHead = ser.Serialize(entity, 0);
        var full = ser.Serialize(entity, 10);

        var eHead2 = ser.Deserialize(strHead, true);
        True(eHead2.Metadata.Count() == 0, "header without metadata should also have non after restoring");

        var strHead2 = ser.Serialize(eHead2);
        Equal(strHead2, strHead2); //, "header without metadata serialized and back should be the same");

        var fullBack = ser.Deserialize(full, true);
        Equal(fullBack.Metadata.Count(), entity.Metadata.Count()); //, "full with metadata should also have after restoring");

        var full2 = ser.Serialize(fullBack, 10);
        Equal(full, full2); //, "serialize, deserialize and serialize should get same thing");


    }

    private JsonSerializer Serializer()
    {
        var ser = jsonSerializer;
        ser.Initialize(AppForQueryTests, new List<IContentType>(), null);
        return ser;
    }

    [Fact]
    public void Query_Run_And_Run_Materialized()
    {
        var qdef = LoadQueryDef(AppForQueryTests, BasicId);
        var query = queryFactory.GetDataSourceForTesting(qdef).Main;
        var countDef = query.ListTac().Count();
        True(countDef > 0, "result > 0");
        Equal(BasicCount, countDef);

        var ser = Serializer();
        var strQuery = ser.Serialize((qdef as ICanBeEntity).Entity, 10);
        var eDef2 = ser.Deserialize(strQuery, true);
        // TODO: #42
        var qdef2 = queryDefinitionBuilder.Create(0, eDef2);
        var query2 = queryFactory.GetDataSourceForTesting(qdef2).Main;
        var countDef2 = query2.ListTac().Count();
        Equal(countDef2, countDef); //, "countdefs should be same");
    }

}
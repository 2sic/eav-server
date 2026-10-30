using System.Diagnostics;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.DataSource.DbTests.Query;

[Startup(typeof(StartupTestFullWithDb))]
public class QueryBasicTest(
    IJsonDataWriter jsonSerializer,
    IJsonDataReader jsonReader,
    QueryDefinitionService queryDefSvc,
    QueryFactory queryFactory,
    QueryDefinitionFactory queryDefFactory,
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
        var pipelineEntity = queryDefSvc.GetDefinition(appState, queryId);

        return pipelineEntity; // queryDefFactory.Create(appId, pipelineEntity);
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
        var ser = DeAndSerializer();
        var entity = (qdef as ICanBeEntity).Entity;
        var strHead = ser.Writer.Serialize(entity, 0);
        var full = ser.Writer.Serialize(entity, 10);

        var eHead2 = ser.Reader.Deserialize(strHead, true);
        True(eHead2.Metadata.Count() == 0, "header without metadata should also have non after restoring");

        var strHead2 = ser.Writer.Serialize(eHead2);
        Equal(strHead2, strHead2); //, "header without metadata serialized and back should be the same");

        var fullBack = ser.Reader.Deserialize(full, true);
        Equal(fullBack.Metadata.Count(), entity.Metadata.Count()); //, "full with metadata should also have after restoring");

        var full2 = ser.Writer.Serialize(fullBack, 10);
        Equal(full, full2); //, "serialize, deserialize and serialize should get same thing");


    }

    private (IJsonDataWriter Writer, IJsonDataReader Reader) DeAndSerializer()
    {
        jsonReader.Initialize(new(AppForQueryTests, new List<IContentType>()));
        return (jsonSerializer, jsonReader);
    }

    [Fact]
    public void Query_Run_And_Run_Materialized()
    {
        var qdef = LoadQueryDef(AppForQueryTests, BasicId);
        var query = queryFactory.CreateWithTestParams(qdef).Main;
        var countDef = query.ListTac().Count();
        True(countDef > 0, "result > 0");
        Equal(BasicCount, countDef);

        var ser = DeAndSerializer();
        var strQuery = ser.Writer.Serialize((qdef as ICanBeEntity).Entity, 10);
        var eDef2 = ser.Reader.Deserialize(strQuery, true);
        // TODO: #42
        var qdef2 = queryDefFactory.Create(0, eDef2);
        var query2 = queryFactory.CreateWithTestParams(qdef2).Main;
        var countDef2 = query2.ListTac().Count();
        Equal(countDef2, countDef); //, "countdefs should be same");
    }

}
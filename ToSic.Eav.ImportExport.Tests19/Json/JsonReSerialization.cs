using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests19.Json;

public class JsonReSerialization(JsonSerializer jsonSerializer, IRepositoryLoader loader, JsonTestHelpers jsonTestHelpers) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{

    [Fact]
    public void JsonReExportHome()
    {
        var test = new SpecsTestExportSerialize();
        Test_DoubleExportEntity(test.AppId, test.TestItemToSerialize);
    }

    [Fact]
    public void JsonReExportContentGroup()
    {
        var test = new SpecsTestExportSerialize();
        Test_DoubleExportEntity(test.AppId, test.ContentBlockWithALotOfItems);
    }

    private void Test_DoubleExportEntity(int appId, int eid, JsonSerializer? serializer = null)
    {
        serializer ??= jsonTestHelpers.SerializerOfApp(appId);
        var json = GetJsonOfEntity(appId, eid, serializer);

        var ent = serializer.Deserialize(json);
        var json2 = serializer.Serialize(ent);
        //Trace.Write($"{{ \"First\": {json}, \"Second\": {json2}}}");
        Equal(json, json2);//, "serialize, de-serialize, and serialize again should be the same!");
    }


    private string GetJsonOfEntity(int appId, int eId, JsonSerializer? ser = null)
    {
        ser ??= jsonTestHelpers.SerializerOfApp(appId);
        var xmlEnt = ser.Serialize(eId);
        return xmlEnt;
    }


    [Fact]
    public void Json_ReExportHundredsOfAnApp()
    {
        var test = new SpecsTestExportSerialize();
        var appId = test.AppId;

        Test_DoubleExportAllOfAnApp(appId);
    }


    private void Test_DoubleExportAllOfAnApp(int appId)
    {
        //var loader = _loader; 
        var app = loader.AppStateReaderRawTac(appId);
        var exBuilder = jsonSerializer.SetApp(app);

        var maxCount = 1000;
        var skip = 0;
        var count = 0;
        try
        {
            foreach (var appEntity in app.List)
            {
                // maybe skip some
                if (count++ < skip) continue;

                Test_DoubleExportEntity(appId, appEntity.EntityId, exBuilder);

                // stop if we ran enough tests
                if (count >= maxCount)
                    return;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"had issue after count{count}", ex);
        }
    }
}
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;
using ToSic.Eav.Serialization.Sys;

namespace ToSic.Eav.ImportExport.Tests.Json;

public class JsonReSerialization(IJsonDataWriter jsonSerializer, IAppsAndZonesLoaderWithRaw loader, JsonTestHelpers jsonTestHelpers) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
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

    private void Test_DoubleExportEntity(int appId, int eid, (IAppReader Reader, IJsonDataWriter Serializer)? tuple = null)
    {
        tuple ??= jsonTestHelpers.ReaderAndSerializerOfApp(appId);
        var json = tuple.Value.Serializer.Serialize(tuple.Value.Reader.List.FindRepoId(eid) ?? throw new NullReferenceException($"Entity {eid} not found, can't serialize"));

        var deser = jsonTestHelpers.JsonReaderOfApp(appId);
        var ent = deser.Deserialize(json);
        var json2 = tuple.Value.Serializer.Serialize(ent);
        //Trace.Write($"{{ \"First\": {json}, \"Second\": {json2}}}");
        Equal(json, json2);//, "serialize, de-serialize, and serialize again should be the same!");
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

        var maxCount = 1000;
        var skip = 0;
        var count = 0;
        try
        {
            foreach (var appEntity in app.List)
            {
                // maybe skip some
                if (count++ < skip) continue;

                Test_DoubleExportEntity(appId, appEntity.EntityId, (app, jsonSerializer));

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
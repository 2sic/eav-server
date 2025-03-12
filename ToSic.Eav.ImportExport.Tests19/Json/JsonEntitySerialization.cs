using ToSic.Eav.ImportExport.Tests;
using ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;
using Xunit.Abstractions;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests19.Json;


public class JsonEntitySerialization(JsonSerializer jsonSerializer, IRepositoryLoader loader, ITestOutputHelper output, JsonTestHelpers jsonTestHelpers) : IClassFixture<DoFixtureStartup<ScenarioDotData>>
{
    [Fact]
    public void Json_ExportItemOnHome()
    {
        var test = new SpecsTestExportSerialize();
        var json = jsonTestHelpers.GetJsonOfEntity(test.AppId, test.TestItemToSerialize);
        output.WriteLine(json);
        True(json.Length > 200, "should get a long json string");
    }
        

    [Fact]
    public void Json_ExportCBWithRelationships()
    {
        var test = new SpecsTestExportSerialize();
        var json = jsonTestHelpers.GetJsonOfEntity(test.AppId, test.ContentBlockWithALotOfItems);
        output.WriteLine(json);
        True(json.Length > 200, "should get a long json string");
    }


        


    [Fact]
    public void Json_ExportHundredsOfAnApp()
    {
        var test = new SpecsTestExportSerialize();
        var appId = test.AppId;

        Test_ExportAllOfAnApp(appId);
    }




    private void Test_ExportAllOfAnApp(int appId)
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

                exBuilder.Serialize(appEntity.EntityId);

                // stop if we ran enough tests
                if (count >= maxCount)
                    return;
            }
        }
        catch (Exception ex)
        {
            throw new($"had issue after count{count}", ex);
        }
    }

        
}
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Sys;
using Xunit.Abstractions;
using JsonSerializer = ToSic.Eav.ImportExport.Json.Sys.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.Json;


public class JsonEntitySerialization(JsonSerializer jsonSerializer, IAppsAndZonesLoaderWithRaw loader, ITestOutputHelper output, JsonTestHelpers jsonTestHelpers)
    : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    private static SpecsTestExportSerialize Specs => new();

    [Fact]
    public void Json_ExportItemOnHome()
    {
        var json = jsonTestHelpers.GetJsonOfEntity(Specs.AppId, Specs.TestItemToSerialize);
        output.WriteLine(json);
        True(json.Length > 200, "should get a long json string");
    }
        

    [Fact]
    public void Json_ExportCBWithRelationships()
    {
        var json = jsonTestHelpers.GetJsonOfEntity(Specs.AppId, Specs.ContentBlockWithALotOfItems);
        output.WriteLine(json);
        True(json.Length > 200, "should get a long json string");
    }


    [Fact]
    public void Json_ExportHundredsOfAnApp() =>
        Test_ExportAllOfAnApp(Specs.AppId, 1000);


    private void Test_ExportAllOfAnApp(int appId, int maxCount)
    {
        var app = loader.AppStateReaderRawTac(appId);
        var exBuilder = jsonSerializer.SetApp(app);

        var skip = 0;
        var count = 0;
        try
        {
            foreach (var appEntity in app.List)
            {
                // maybe skip some
                if (count++ < skip) continue;

                var json = exBuilder.Serialize(appEntity.EntityId);
                True(json.Length > 25, "should get a long json string");

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
using ToSic.Eav.ImportExport.Tests;
using Xunit.Abstractions;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests19.Json;

// TODO: update stored file to match current serialization

public class JsonCtDeSerialization(ITestOutputHelper output, JsonTestHelpers jsonTestHelper) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    //[DeploymentItem("..\\..\\" + typesPath, testTypesPath)]
    //[Ignore("Disabled for now, should update raw template file for comparison so it would match again")]
    public void Json_LoadFile_SqlDataSource()
    {
        var test = new SpecsTestExportSerialize();
        var json = LoadJson("System.Config ToSic.Eav.DataSources.SqlDataSource.json");
        var ser = jsonTestHelper.SerializerOfApp(test.AppId);
        var contentType = ContentType(ser, json);
        var reSer = JsonTestHelpers.JsonOfContentType(ser, contentType);
        Equal(json, reSer);//, "original and re-serialized should be the same");
            
    }


    internal IContentType ContentType(JsonSerializer ser, string json)
    {
        var type = ser.DeserializeContentType(json);
        return type;
    }

    private string LoadJson(string path)
    {
        var root = TestFiles.GetTestPath($"Json\\Scenarios\\ContentTypes\\");
        return File.ReadAllText(root + path);
    }
}
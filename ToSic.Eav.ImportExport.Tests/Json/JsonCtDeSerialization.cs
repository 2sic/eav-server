using ToSic.Eav.ImportExport.Tests.Persistence.File;
using Xunit.Abstractions;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.Json;

public class JsonCtDeSerialization(ITestOutputHelper output, JsonTestHelpers jsonTestHelper) : IClassFixture<DoFixtureStartup<ScenarioBasicAndMini>>
{
    private const string ExpectedTestFileLocation = $"Json\\Scenarios\\ExpectedContentType\\";
    private const string ExpectedTestFileName = "System.DataSources.c76901b5-0345-4866-9fa3-6208de7f8543.2025-03-13.json";
    [Fact]
    public void Json_LoadFile_SqlDataSource()
    {
        // Load the file as exported a while ago to compare results
        var path = TestFiles.GetTestPath(ExpectedTestFileLocation + ExpectedTestFileName);
        var json = File.ReadAllText(path );


        // Just convert the json to a ContentType and back - the AppId isn't really relevant here
        var ser = jsonTestHelper.SerializerOfApp(Constants.PresetAppId);
        var contentType = ContentType(ser, json);
        var reSer = JsonTestHelpers.JsonOfContentType(ser, contentType);
        output.WriteLine(reSer);

        // Special: The ContentTypes which we serialize again will be missing related Entities (such as formulas)
        // This is because in this test, the ContentType is not part of an App, so it didn't have a place to store the related Entities
        // Because of this, we only want to test the 12'000 chars or so before the Entities start
        var jsonOriginalBeforeEntities = json.Substring(0, json.IndexOf("Entities\":[", StringComparison.Ordinal));
        var jsonReSerializedBeforeEntities = reSer.Substring(0, reSer.IndexOf("Entities\":[", StringComparison.Ordinal));

        // Verify that cut-off is where we expect it to be as of 2025-03-13. If the export changes, we may need to update this number.
        InRange(jsonOriginalBeforeEntities.Length, 12000, 13000);
        Equal(jsonOriginalBeforeEntities, jsonReSerializedBeforeEntities);
            
    }


    internal IContentType ContentType(JsonSerializer ser, string json)
    {
        var type = ser.DeserializeContentType(json);
        return type;
    }

}
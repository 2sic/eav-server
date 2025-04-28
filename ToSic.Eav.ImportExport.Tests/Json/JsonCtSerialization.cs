using ToSic.Eav.ImportExport.Tests.Persistence.File;
using Xunit.Abstractions;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.Json;

public class JsonCtSerialization(ITestOutputHelper output, JsonTestHelpers jsonTestHelper) : IClassFixture<DoFixtureStartup<ScenarioMini>>
{

    [Fact]
    public void Json_ExportCTOfItemOnHome()
    {
        var test = new SpecsTestExportSerialize();
        var json = GetJsonOfContentTypeOfItem(test.AppId, test.TestItemToSerialize);
        output.WriteLine(json);
        True(json.Length > 200, "should get a long json string");
    }

    [Fact]
    public void Json_ExportCTOfBlog()
    {
        var test = new SpecsTestExportSerialize();
        var json = JsonOfContentType(test.AppId, test.TestItemTypeName);
        output.WriteLine(json);
        True(json.Length > 200, "should get a long json string");
    }

    //[Ignore("can't test as currently the text-file for this isn't in the test setup")]
    [Fact]
    public void Json_Export_OfType_ConfigSqlDataSource()
    {
        var test = new SpecsTestExportSerialize();
        var json = JsonOfContentType(test.AppId, "|Config ToSic.Eav.DataSources.SqlDataSource");
        output.WriteLine(json);
    }

    [Fact]
    public void Json_Export_OfType()
    {
        var test = new SpecsTestExportSerialize();
        var json = JsonOfContentType(test.AppId, test.TestItemStaticTypeId);
        output.WriteLine(json);
    }

    private string GetJsonOfContentTypeOfItem(int appId, int eId)
        => GetJsonOfContentTypeOfItem(eId, jsonTestHelper.SerializerOfApp(appId));

    internal static string GetJsonOfContentTypeOfItem(int eId, JsonSerializer ser)
    {
        var x = ser.AppReaderOrError.List.One(eId);
        var xmlEnt = ser.Serialize(x.Type);
        return xmlEnt;
    }

    private string JsonOfContentType(int appId, string typeName)
        => JsonOfContentType(jsonTestHelper.SerializerOfApp(appId), typeName);

    internal static string JsonOfContentType(JsonSerializer ser, string typeName)
        => JsonTestHelpers.JsonOfContentType(ser, ser.AppReaderOrError.GetContentType(typeName));
}
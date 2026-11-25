using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Serialization;
using Xunit.Abstractions;

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
        => GetJsonOfContentTypeOfItem(eId, jsonTestHelper.ReaderAndSerializerOfApp(appId));

    internal static string GetJsonOfContentTypeOfItem(int eId, (IAppReader Reader, IJsonDataWriter Serializer) tuple)
    {
        var (appReader, ser) = tuple;
        var x = appReader.List.GetOne(eId);
        var xmlEnt = ser.Serialize(x.Type);
        return xmlEnt;
    }

    private string JsonOfContentType(int appId, string typeName)
        => JsonOfContentType(jsonTestHelper.ReaderAndSerializerOfApp(appId), typeName);

    internal static string JsonOfContentType((IAppReader Reader, IJsonDataWriter Serializer) tuple, string typeName)
        => JsonTestHelpers.JsonOfContentType(tuple.Serializer, tuple.Reader.GetContentTypeTac(typeName));
}
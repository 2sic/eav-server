using ToSic.Eav.ImportExport.Tests;

namespace ToSic.Eav.ImportExport.Tests19.Json;

public class JsonDynamic(JsonTestHelpers jsonTestHelpers) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void DeserializeDynamicByDefaultMustFail()
    {
        var test = new SpecsTestExportSerialize();
        var serializer = jsonTestHelpers.SerializerOfApp(test.AppId);
        var json = jsonTestHelpers.GetJsonOfEntity(test.AppId, test.TestItemToSerialize, serializer);

        serializer.Deserialize(json); // should work

        var jsonDynamic = ChangeTypeOfJson(test, json, "something-dynamic");
        Throws<FormatException>(() => serializer.Deserialize(jsonDynamic));
    }


    [Fact]
    public void DeserializeDynamic()
    {
        var specs = new SpecsTestExportSerialize();
        var serializer = jsonTestHelpers.SerializerOfApp(specs.AppId);
        var json = jsonTestHelpers.GetJsonOfEntity(specs.AppId, specs.TestItemToSerialize, serializer);

        var ent = serializer.Deserialize(json); // should work
        Equal(specs.TestItemAttributeCount, ent.Attributes.Count);//, "orig has 4 attribs");

        var jsonDynamic = ChangeTypeOfJson(specs, json, "something-dynamic");
        ent = serializer.Deserialize(jsonDynamic, true); // should work too
        True(ent.Type.IsDynamic, "should be dynamic");
        Equal("something-dynamic", ent.Type.NameId);//, "name should be dynamic");
        Equal(specs.TestItemAttributeCount, ent.Attributes.Count);//, "dynamic entity should also have 4 attribs");

        jsonDynamic = Add2FieldsToJson(jsonDynamic);
        ent = serializer.Deserialize(jsonDynamic, true); // should work too
        Equal(6, ent.Attributes.Count);//, "second dynamic entity should also have 6 attribs");

        Equal("v1", ent.GetTac("f1"));//, "added field f1 should be v1");
        Equal(specs.TestItemLinkValue, ent.GetTac(specs.TestItemLinkField));//, "original fields should still work");
        Null(ent.GetBestTitle());//, "shouldn't have a real title");
    }


    private static string ChangeTypeOfJson(SpecsTestExportSerialize specs, string json, string newType)
        => json.Replace("\"Id\":\"" + specs.TestItemStaticTypeId + "\"", "\"Id\":\"" + newType + "\"");



    private static string Add2FieldsToJson(string json)
        => json.Replace("\"Title\":{", "\"f1\":{\"en-us\": \"v1\"},\"f2\":{\"en-us\": \"v2\"},\"Title\":{");
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Tests.Json;
using ToSic.Eav.Repository.Efc.Tests;


namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonDynamic : JsonTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void DeserializeDynamicByDefaultMustFail()
        {
            var test = new SpecsTestExportSerialize();
            var serializer = SerializerOfApp(test.AppId);
            var json = GetJsonOfEntity(test.AppId, test.TestItemToSerialize, serializer);
            //Trace.Write(json);

            serializer.Deserialize(json); // should work

            var jsonDynamic = ChangeTypeOfJson(test, json, "something-dynamic");
            serializer.Deserialize(jsonDynamic); // should fail
        }


        [TestMethod]
        public void DeserializeDynamic()
        {
            var specs = new SpecsTestExportSerialize();
            var serializer = SerializerOfApp(specs.AppId);
            var json = GetJsonOfEntity(specs.AppId, specs.TestItemToSerialize, serializer);

            var ent = serializer.Deserialize(json); // should work
            Assert.AreEqual(specs.TestItemAttributeCount, ent.Attributes.Count, "orig has 4 attribs");

            var jsonDynamic = ChangeTypeOfJson(specs, json, "something-dynamic");
            ent = serializer.Deserialize(jsonDynamic, true); // should work too
            Assert.IsTrue(ent.Type.IsDynamic, "should be dynamic");
            Assert.AreEqual("something-dynamic", ent.Type.NameId, "name should be dynamic");
            Assert.AreEqual(specs.TestItemAttributeCount, ent.Attributes.Count, "dynamic entity should also have 4 attribs");

            jsonDynamic = Add2FieldsToJson(jsonDynamic);
            ent = serializer.Deserialize(jsonDynamic, true); // should work too
            Assert.AreEqual(6, ent.Attributes.Count, "second dynamic entity should also have 6 attribs");

            Assert.AreEqual("v1", ent.GetTac("f1"), "added field f1 should be v1");
            Assert.AreEqual(specs.TestItemLinkValue, ent.GetTac(specs.TestItemLinkField), "original fields should still work");
            Assert.AreEqual(null, ent.GetBestTitle(), "shouldn't have a real title");
        }


        private static string ChangeTypeOfJson(SpecsTestExportSerialize specs, string json, string newType)
            => json.Replace("\"Id\":\"" + specs.TestItemStaticTypeId + "\"", "\"Id\":\"" + newType + "\"");



        private static string Add2FieldsToJson(string json)
            => json.Replace("\"Title\":{", "\"f1\":{\"en-us\": \"v1\"},\"f2\":{\"en-us\": \"v2\"},\"Title\":{");
    }
}

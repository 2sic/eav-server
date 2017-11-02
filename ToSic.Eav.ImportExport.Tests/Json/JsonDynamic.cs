using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var test = new TestValuesOnPc2Dm();
            var serializer = SerializerOfApp(test.AppId);
            var json = GetJsonOfEntity(test.AppId, test.ItemOnHomeId, serializer);
            //Trace.Write(json);

            serializer.Deserialize(json); // should work

            var jsonDynamic = ChangeTypeOfJson(json, "something-dynamic");
            serializer.Deserialize(jsonDynamic); // should fail
        }


        [TestMethod]
        public void DeserializeDynamic()
        {
            var test = new TestValuesOnPc2Dm();
            var serializer = SerializerOfApp(test.AppId);
            var json = GetJsonOfEntity(test.AppId, test.ItemOnHomeId, serializer);

            var ent = serializer.Deserialize(json); // should work
            Assert.AreEqual(4, ent.Attributes.Count, "orig has 4 attribs");

            var jsonDynamic = ChangeTypeOfJson(json, "something-dynamic");
            ent = serializer.Deserialize(jsonDynamic, true); // should work too
            Assert.IsTrue(ent.Type.IsDynamic, "should be dynamic");
            Assert.AreEqual("something-dynamic", ent.Type.Name, "name should be dynamic");
            Assert.AreEqual(4, ent.Attributes.Count, "dynamic entity should also have 4 attribs");

            jsonDynamic = Add2FieldsToJson(jsonDynamic);
            ent = serializer.Deserialize(jsonDynamic, true); // should work too
            Assert.AreEqual(6, ent.Attributes.Count, "second dynamic entity should also have 6 attribs");

            Assert.AreEqual("v1", ent.GetBestValue("f1"), "added field f1 should be v1");
            Assert.AreEqual("File:1075", ent.GetBestValue("Image"), "original fields should still work");
            Assert.AreEqual(null, ent.GetBestTitle(), "shouldn't have a real title");
        }


        private static string ChangeTypeOfJson(string json, string newType)
            => json.Replace("\"Id\":\"7eae404c-0ba9-48d3-8cc8-2cba9d48ca0d\"", "\"Id\":\"" + newType + "\"");



        private static string Add2FieldsToJson(string json)
            => json.Replace("\"Title\":{", "\"f1\":{\"en-us\": \"v1\"},\"f2\":{\"en-us\": \"v2\"},\"Title\":{");
    }
}

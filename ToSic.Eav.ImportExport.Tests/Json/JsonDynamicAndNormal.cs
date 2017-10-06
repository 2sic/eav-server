﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Tests.Json;
using ToSic.Eav.Repository.Efc.Tests;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonDynamicAndNormal : JsonTestBase
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

            var jsonDynamic = ChangeTypeOfJson(json);
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

            var jsonDynamic = ChangeTypeOfJson(json);
            ent = serializer.Deserialize(jsonDynamic, true); // should work too
            Assert.AreEqual(Constants.DynamicType, ent.Type.Name, "name should be dynamic");
            Assert.AreEqual(4, ent.Attributes.Count, "dynamic entity should also have 4 attribs");

            jsonDynamic = Add2FieldsToJson(jsonDynamic);
            ent = serializer.Deserialize(jsonDynamic, true); // should work too
            Assert.AreEqual(6, ent.Attributes.Count, "second dynamic entity should also have 6 attribs");
        }


        private static string ChangeTypeOfJson(string json)
            => json.Replace("\"Id\":\"7eae404c-0ba9-48d3-8cc8-2cba9d48ca0d\"", "\"Id\":\"something-dynamic\"");



        private static string Add2FieldsToJson(string json)
            => json.Replace("\"Title\":{", "\"f1\":{\"en-us\": \"v1\"},\"f2\":{\"en-us\": \"v2\"},\"Title\":{");
    }
}

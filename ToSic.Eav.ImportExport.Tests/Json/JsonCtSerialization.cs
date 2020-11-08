using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repository.Efc.Tests;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonCtSerialization: Eav.Persistence.Efc.Tests.Efc11TestBase
    {
        public JsonCtSerialization() : base() { }

        [TestMethod]
        public void Json_ExportCTOfItemOnHome()
        {
            var test = new TestValuesOnPc2Dm();
            var json = GetJsonOfContentTypeOfItem(test.AppId, test.ItemOnHomeId);
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }

        [TestMethod]
        public void Json_ExportCTOfBlog()
        {
            var test = new TestValuesOnPc2Dm();
            var json = JsonOfContentType(test.BlogAppId, "BlogPost");
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }

        [Ignore("can't test as currently the text-file for this isn't in the test setup")]
        [TestMethod]
        public void Json_Export_OfType_ConfigSqlDataSource()
        {
            var test = new TestValuesOnPc2Dm();
            var json = JsonOfContentType(test.BlogAppId, "|Config ToSic.Eav.DataSources.SqlDataSource");
            Trace.Write(json);
        }

        [TestMethod]
        public void Json_Export_OfType()
        {
            var test = new TestValuesOnPc2Dm();
            var json = JsonOfContentType(test.BlogAppId, "48d849d6-b83d-4001-96e5-79da0833e84e");
            Trace.Write(json);
        }

        private string GetJsonOfContentTypeOfItem(int appId, int eId)
            => GetJsonOfContentTypeOfItem(eId, SerializerOfApp(appId));

        internal static string GetJsonOfContentTypeOfItem(int eId, JsonSerializer ser)
        {
            var x = ser.App.List.One(eId);
            var xmlEnt = ser.Serialize(x.Type);
            return xmlEnt;
        }

        private string JsonOfContentType(int appId, string typeName)
            => JsonOfContentType(SerializerOfApp(appId), typeName);

        internal static string JsonOfContentType(JsonSerializer ser, string typeName)
            => JsonOfContentType(ser, ser.App.GetContentType(typeName));

        internal static string JsonOfContentType(JsonSerializer ser, IContentType type) 
            => ser.Serialize(type);
    }
}

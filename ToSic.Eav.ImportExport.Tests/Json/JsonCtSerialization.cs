using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Repository.Efc.Tests;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonCtSerialization: Persistence.Efc.Tests.Efc11TestBase
    {

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
            var json = GetJsonOfContentType(test.BlogAppId, "BlogPost");
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }

        

        private string GetJsonOfContentTypeOfItem(int appId, int eId, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var x = exBuilder.App.Entities[eId];
            var xmlEnt = exBuilder.Serialize(x.Type);
            return xmlEnt;
        }

        private string GetJsonOfContentType(int appId, string typeName, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var type = exBuilder.App.GetContentType(typeName);
            var xmlEnt = exBuilder.Serialize(type);
            return xmlEnt;
        }


        


    }
}

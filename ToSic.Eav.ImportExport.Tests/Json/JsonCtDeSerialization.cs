using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Repository.Efc.Tests;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonCtDeSerialization: Eav.Persistence.Efc.Tests.Efc11TestBase
    {
        public TestContext TestContext { get; set; }

        private const string typesPath = "\\Json\\ContentTypes\\";
        private const string testTypesPath = "types";

        [TestMethod]
        [DeploymentItem("..\\..\\" + typesPath, testTypesPath)]
        public void Json_LoadFile_SqlDataSource()
        {
            var test = new TestValuesOnPc2Dm();
            var json = LoadJson("System.Config ToSic.Eav.DataSources.SqlDataSource.json");
            var ser = SerializerOfApp(test.BlogAppId);
            var contentType = ContentType(ser, json);
            var reSer = JsonCtSerialization.JsonOfContentType(ser, contentType);
            Assert.AreEqual(json, reSer, "original and re-serialized should be the same");
            
        }

        private IContentType ContentType(int appId, string json)
            => ContentType(SerializerOfApp(appId), json);


        internal IContentType ContentType(JsonSerializer ser, string json)
        {
            var type = ser.DeserializeContentType(json);
            return type;
        }

        private string LoadJson(string path)
        {
            var root = TestContext.DeploymentDirectory + "\\" + testTypesPath + "\\";
            return File.ReadAllText(root + path);
        }
    }
}

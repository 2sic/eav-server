using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Tests;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    public class JsonSerializationTests: Persistence.Efc.Tests.Efc11TestBase
    {
        public static Log Log { get; set; } = new Log("TstJsn");

        [TestMethod]
        public void Json_ExportItemOnHome()
        {
            var test = new TestValuesOnPc2Dm();
            var xmlEnt = GetJsonOfEntity(test.AppId, test.ItemOnHomeId);
            Trace.Write(xmlEnt);
            Assert.IsTrue(xmlEnt.Length > 200, "should get a long json string");
        }

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

        [TestMethod]
        public void Json_DoubleExportHome()
        {
            var test = new TestValuesOnPc2Dm();
            Test_DoubleExportEntity(test.AppId, test.ItemOnHomeId);
        }

         [TestMethod]
        public void Json_DoubleExportCG()
        {
            var test = new TestValuesOnPc2Dm();
            Test_DoubleExportEntity(test.AppId, test.ContentBlockItemWith9Items);
        }
       private static void Test_DoubleExportEntity(int appId, int eid, JsonSerializer serializer = null)
        {
            serializer = serializer ?? SerializerOfApp(appId);
            var json = GetJsonOfEntity(appId, eid, serializer);

            var ent = serializer.Deserialize(json);
            var json2 = serializer.Serialize(ent);
            //Trace.Write($"{{ \"First\": {json}, \"Second\": {json2}}}");
            Assert.AreEqual(json, json2, "serialize, de-serialize, and serialize again should be the same!");
        }

        [TestMethod]
        public void Json_ExportCBWithRelationships()
        {
            var test = new TestValuesOnPc2Dm();
            var xmlEnt = GetJsonOfEntity(test.AppId, test.ContentBlockItemWith9Items);
            Trace.Write(xmlEnt);
            Assert.IsTrue(xmlEnt.Length > 200, "should get a long json string");
        }

        private static string GetJsonOfEntity(int appId, int eId, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var xmlEnt = exBuilder.Serialize(eId);
            return xmlEnt;
        }

        private static string GetJsonOfContentTypeOfItem(int appId, int eId, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var x = exBuilder.App.Entities[eId];
            var xmlEnt = exBuilder.Serialize(x.Type);
            return xmlEnt;
        }
        private static string GetJsonOfContentType(int appId, string typeName, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var type = exBuilder.App.ContentTypes.Values.Single(x => x.StaticName == typeName || x.Name == typeName);
            var xmlEnt = exBuilder.Serialize(type);
            return xmlEnt;
        }

        private static JsonSerializer SerializerOfApp(int appId)
        {
            var dbc = DbDataController.Instance(null, appId, Log);
            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(appId);
            //var exBuilder = new JsonSerializer();
            //exBuilder.Initialize(app);
            //return exBuilder;
            return SerializerOfApp(app);
        }


        [TestMethod]
        public void Json_ExportHundredsOfAnApp()
        {
            var test = new TestValuesOnPc2Dm();
            var appId = test.AppId;

            Test_ExportAllOfAnApp(appId);
        }

        [TestMethod]
        public void Json_DoubleExportHundredsOfAnApp()
        {
            var test = new TestValuesOnPc2Dm();
            var appId = test.AppId;

            Test_DoubleExportAllOfAnApp(appId);
        }


        private static void Test_ExportAllOfAnApp(int appId)
        {
            var dbc = DbDataController.Instance(null, appId, Log);

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(appId);
            var exBuilder = new JsonSerializer();
            exBuilder.Initialize(app);

            var maxCount = 1000;
            var skip = 0;
            var count = 0;
            try
            {
                foreach (var appEntity in app.Entities.Values)
                {
                    // maybe skip some
                    if (count++ < skip) continue;

                    exBuilder.Serialize(appEntity.EntityId);

                    // stop if we ran enough tests
                    if (count >= maxCount)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"had issue after count{count}", ex);
            }
        }


        private static void Test_DoubleExportAllOfAnApp(int appId)
        {
            var dbc = DbDataController.Instance(null, appId, Log);

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(appId);
            var exBuilder = new JsonSerializer();
            exBuilder.Initialize(app);

            var maxCount = 1000;
            var skip = 0;
            var count = 0;
            try
            {
                foreach (var appEntity in app.Entities.Values)
                {
                    // maybe skip some
                    if (count++ < skip) continue;

                    Test_DoubleExportEntity(appId, appEntity.EntityId, exBuilder);

                    // stop if we ran enough tests
                    if (count >= maxCount)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"had issue after count{count}", ex);
            }
        }
    }
}

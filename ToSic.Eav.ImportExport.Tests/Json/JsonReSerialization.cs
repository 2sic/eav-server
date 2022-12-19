using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Tests;
using ToSic.Lib.Logging;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonReSerialization: Eav.Persistence.Efc.Tests.Efc11TestBase
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly IRepositoryLoader _loader;

        public JsonReSerialization()
        {
            _jsonSerializer = Build<JsonSerializer>();
            _loader = Build<IRepositoryLoader>();
        }

        [TestMethod]
        public void JsonReExportHome()
        {
            var test = new SpecsTestExportSerialize();
            Test_DoubleExportEntity(test.AppId, test.TestItemToSerialize);
        }

        [TestMethod]
        public void JsonReExportContentGroup()
        {
            var test = new SpecsTestExportSerialize();
            Test_DoubleExportEntity(test.AppId, test.ContentBlockWithALotOfItems);
        }

        private void Test_DoubleExportEntity(int appId, int eid, JsonSerializer serializer = null)
        {
            serializer = serializer ?? SerializerOfApp(appId);
            var json = GetJsonOfEntity(appId, eid, serializer);

            var ent = serializer.Deserialize(json);
            var json2 = serializer.Serialize(ent);
            //Trace.Write($"{{ \"First\": {json}, \"Second\": {json2}}}");
            Assert.AreEqual(json, json2, "serialize, de-serialize, and serialize again should be the same!");
        }


        private string GetJsonOfEntity(int appId, int eId, JsonSerializer ser = null)
        {
            var exBuilder = ser ?? SerializerOfApp(appId);
            var xmlEnt = exBuilder.Serialize(eId);
            return xmlEnt;
        }


        [TestMethod]
        public void Json_ReExportHundredsOfAnApp()
        {
            var test = new SpecsTestExportSerialize();
            var appId = test.AppId;

            Test_DoubleExportAllOfAnApp(appId);
        }


        private void Test_DoubleExportAllOfAnApp(int appId)
        {
            var loader = _loader; 
            var app = loader.AppState(appId, false);
            var exBuilder = _jsonSerializer.Init(Log).SetApp(app);

            var maxCount = 1000;
            var skip = 0;
            var count = 0;
            try
            {
                foreach (var appEntity in app.List)
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

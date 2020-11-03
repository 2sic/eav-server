using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ImportExport.Tests.Json;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Tests;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonEntitySerialization: JsonTestBase
    {
        [TestMethod]
        public void Json_ExportItemOnHome()
        {
            var test = new TestValuesOnPc2Dm();
            var json = GetJsonOfEntity(test.AppId, test.ItemOnHomeId);
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }
        

        [TestMethod]
        public void Json_ExportCBWithRelationships()
        {
            var test = new TestValuesOnPc2Dm();
            var json = GetJsonOfEntity(test.AppId, test.ContentBlockItemWith9Items);
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }


        


        [TestMethod]
        public void Json_ExportHundredsOfAnApp()
        {
            var test = new TestValuesOnPc2Dm();
            var appId = test.AppId;

            Test_ExportAllOfAnApp(appId);
        }




        private void Test_ExportAllOfAnApp(int appId)
        {
            var dbc = Factory.Resolve<DbDataController>().Init(null, appId, Log);

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppState(appId);
            var exBuilder = new JsonSerializer(app, Log);

            var maxCount = 1000;
            var skip = 0;
            var count = 0;
            try
            {
                foreach (var appEntity in app.List)
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

        
    }
}

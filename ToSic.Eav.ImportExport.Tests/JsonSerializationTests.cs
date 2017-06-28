using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Tests;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    public class JsonSerializationTests
    {
        [TestMethod]
        public void Json_ExportItemOnHome()
        {
            var test = new TestValuesOnPc2Dm();
            var appId = test.AppId;
            var dbc = DbDataController.Instance(null, appId);

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(test.AppId);
            var exBuilder = new JsonSerializer();
            exBuilder.Initialize(app);
            var xmlEnt = exBuilder.Serialize(test.ItemOnHomeId);
            Trace.Write(xmlEnt);
            Assert.IsTrue(xmlEnt.Length > 200, "should get a long json string");


        }
        [TestMethod]
        public void Json_ExportHundredsOfAnApp()
        {
            var test = new TestValuesOnPc2Dm();
            var appId = test.AppId;
            var dbc = DbDataController.Instance(null, appId);

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppPackage(test.AppId);
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

                    var xml = exBuilder.Serialize(appEntity.EntityId);

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

﻿using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.ImportExport.Tests.Json;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Tests;
using ToSic.Lib.Logging;
using ToSic.Testing.Shared;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.ImportExport.Tests.json
{
    [TestClass]
    public class JsonEntitySerialization: JsonTestBase
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly IRepositoryLoader _loader;

        public JsonEntitySerialization()
        {
            _jsonSerializer = GetService<JsonSerializer>();
            _loader = GetService<IRepositoryLoader>();
        }

        [TestMethod]
        public void Json_ExportItemOnHome()
        {
            var test = new SpecsTestExportSerialize();
            var json = GetJsonOfEntity(test.AppId, test.TestItemToSerialize);
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }
        

        [TestMethod]
        public void Json_ExportCBWithRelationships()
        {
            var test = new SpecsTestExportSerialize();
            var json = GetJsonOfEntity(test.AppId, test.ContentBlockWithALotOfItems);
            Trace.Write(json);
            Assert.IsTrue(json.Length > 200, "should get a long json string");
        }


        


        [TestMethod]
        public void Json_ExportHundredsOfAnApp()
        {
            var test = new SpecsTestExportSerialize();
            var appId = test.AppId;

            Test_ExportAllOfAnApp(appId);
        }




        private void Test_ExportAllOfAnApp(int appId)
        {
            var loader = _loader;
            var app = loader.AppStateRawTA(appId);
            var exBuilder = _jsonSerializer.SetApp(app.ToInterface(Log));

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

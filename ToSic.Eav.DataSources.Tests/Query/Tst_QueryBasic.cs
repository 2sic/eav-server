using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources.Tests.Query
{
    [TestClass]
    public class Tst_QueryBasic
    {
        private const int queryId = 765;

        [TestMethod]
        public void LookForQuery_DeepApi()
        {
            //var source = DataSource.GetInitialDataSource(appId: TestConfig.AppForQueryTests);
            //var pipelineEntity = DataPipeline.GetPipelineEntity(queryId, source);
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, queryId);
            Assert.IsNotNull(qdef.Header);

            var metadata = qdef.Header.Metadata.ToList();
            Assert.IsTrue(metadata.Count > 0);

        }

        private QueryDefinition LoadQueryDef(int appId, int queryId)
        {
            var source = DataSource.GetInitialDataSource(appId: appId);
            var pipelineEntity = DataPipeline.GetPipelineEntity(queryId, source);
            return new QueryDefinition(pipelineEntity);
        }



        [TestMethod]
        public void Query_To_Json()
        {
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, queryId);
            var ser = new JsonSerializer();
            var justHeader = ser.Serialize(qdef.Header, 0);
            var full = ser.Serialize(qdef.Header, 10);
            Assert.IsTrue(full.Length > justHeader.Length *2, "full serialized should be much longer");
            Trace.WriteLine("basic");
            Trace.WriteLine(justHeader);
            Trace.WriteLine("full");
            Trace.Write(full);
        }

        [TestMethod]
        public void Query_to_Json_and_back()
        {
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, queryId);
            var ser = new JsonSerializer();
            var strHead = ser.Serialize(qdef.Header, 0);
            var full = ser.Serialize(qdef.Header, 10);

            ser.Initialize(TestConfig.AppForQueryTests, new List<IContentType>(), null, null);
            var eHead2 = ser.Deserialize(strHead, true);
            Assert.IsTrue(eHead2.Metadata.Count() == 0, "header without metadata should also have non after restoring");

            var strHead2 = ser.Serialize(eHead2);
            Assert.AreEqual(strHead2, strHead2, "header without metadata serialized and back should be the same");

            var fullBack = ser.Deserialize(full, true);
            Assert.AreEqual(fullBack.Metadata.Count(), qdef.Header.Metadata.Count(),
                "full with metadata should also have after restoring");

            var full2 = ser.Serialize(fullBack, 10);
            Assert.AreEqual(full, full2, "serialize, deserialize and serialize should get same thing");


        }

    }
}

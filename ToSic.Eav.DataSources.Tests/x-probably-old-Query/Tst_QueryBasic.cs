using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSourceTests;
using ToSic.Eav.ImportExport.Json;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSources.Tests.Query
{
    [TestClass]
    public class Tst_QueryBasic: EavTestBase
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly QueryManager _queryManager;
        private readonly QueryBuilder _queryBuilder;

        public Tst_QueryBasic()
        {
            _jsonSerializer = Resolve<JsonSerializer>();
            _queryManager = Resolve<QueryManager>();
            _queryBuilder = Resolve<QueryBuilder>();
        }

        private const int basicId = 765;
        private const int basicCount = 8;

        [TestMethod]
        public void LookForQuery_DeepApi()
        {
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, basicId);
            Assert.IsNotNull(qdef.Entity);

            var metadata = qdef.Entity.Metadata.ToList();
            Assert.IsTrue(metadata.Count > 0);

        }

        private QueryDefinition LoadQueryDef(int appId, int queryId)
        {
            var appState = Apps.State.Get(appId);
            var pipelineEntity = _queryManager.Init(null).GetQueryEntity(queryId, appState);
            return new QueryDefinition(pipelineEntity, appId, null);
        }



        [TestMethod]
        public void Query_To_Json()
        {
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, basicId);
            var ser = _jsonSerializer;
            var justHeader = ser.Serialize(qdef.Entity, 0);
            var full = ser.Serialize(qdef.Entity, 10);
            Assert.IsTrue(full.Length > justHeader.Length *2, "full serialized should be much longer");
            Trace.WriteLine("basic");
            Trace.WriteLine(justHeader);
            Trace.WriteLine("full");
            Trace.Write(full);
        }

        [TestMethod]
        public void Query_to_Json_and_back()
        {
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, basicId);
            var ser = Serializer();
            var strHead = ser.Serialize(qdef.Entity, 0);
            var full = ser.Serialize(qdef.Entity, 10);

            var eHead2 = ser.Deserialize(strHead, true);
            Assert.IsTrue(eHead2.Metadata.Count() == 0, "header without metadata should also have non after restoring");

            var strHead2 = ser.Serialize(eHead2);
            Assert.AreEqual(strHead2, strHead2, "header without metadata serialized and back should be the same");

            var fullBack = ser.Deserialize(full, true);
            Assert.AreEqual(fullBack.Metadata.Count(), qdef.Entity.Metadata.Count(),
                "full with metadata should also have after restoring");

            var full2 = ser.Serialize(fullBack, 10);
            Assert.AreEqual(full, full2, "serialize, deserialize and serialize should get same thing");


        }

        private JsonSerializer Serializer()
        {
            var ser = _jsonSerializer;
            ser.Initialize(TestConfig.AppForQueryTests, new List<IContentType>(), null, null);
            return ser;
        }

        [TestMethod]
        public void Query_Run_And_Run_Materialized()
        {
            var qdef = LoadQueryDef(TestConfig.AppForQueryTests, basicId);
            var query = _queryBuilder.GetDataSourceForTesting(qdef, false);
            var countDef = query.List.Count();
            Assert.IsTrue(countDef > 0, "result > 0");
            Assert.AreEqual(basicCount, countDef);

            var ser = Serializer();
            var strQuery = ser.Serialize(qdef.Entity, 10);
            var eDef2 = ser.Deserialize(strQuery, true);

            var qdef2 = new QueryDefinition(eDef2, 0, null);
            var query2 = _queryBuilder.GetDataSourceForTesting(qdef2, false);
            var countDef2 = query2.List.Count();
            Assert.AreEqual(countDef2, countDef, "countdefs should be same");
        }

    }
}

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.DataSources.Tests.Query
{
    [TestClass]
    public class Tst_QueryBasic
    {
        [TestMethod]
        public void LookForQuery()
        {
            var appId = TestConfig.AppForQueryTests;
            var queryId = 765;

            var source = DataSource.GetInitialDataSource(appId: appId);
            var pipelineEntity = DataPipeline.GetPipelineEntity(queryId, source);

            var metadata = pipelineEntity.Metadata.ToList();

            Assert.IsNotNull(pipelineEntity);

            Assert.IsTrue(metadata.Count > 0);

            var ser = new JsonSerializer();

            var lightser = ser.Serialize(pipelineEntity, 0);
            var serialized = ser.Serialize(pipelineEntity, 10);
            Trace.WriteLine("basic");
            Trace.WriteLine(lightser);
            Trace.WriteLine("full");
            Trace.Write(serialized);
        }
    }
}

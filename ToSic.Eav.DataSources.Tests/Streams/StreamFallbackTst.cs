using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Streams
{
    [TestClass]
    public class StreamFallbackTst: TestBaseEavDataSource
    {
        [TestMethod]
        public void StreamWhereDefaultIsReturned()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.InForTests()[DataSourceConstants.DefaultStreamName] = stmf.InForTests()["1"];

            Assert.AreEqual(1, stmf.ListForTests().Count(), "should have found 1");
        }

        [TestMethod]
        public void StreamsWhereFirstFallbackIsReturned()
        {
            var stmf = AssembleTestFallbackStream();
            Assert.AreEqual(1, stmf.ListForTests().Count(), "should be 1 - the fallback");
        }

        [TestMethod]
        public void DoManyFoldFallback()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.InForTests().Remove("1");
            stmf.InForTests().Add("Fallback1", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("Fallback2", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("Fallback3", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("Fallback4", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("Fallback5", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            Assert.AreEqual(45, stmf.ListForTests().Count(), "Should have looped through many and found the 45");
            Assert.AreEqual("ZMany", stmf.ReturnedStreamName);
        }

        [TestMethod]
        public void DoManyFallbacks2()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.InForTests().Add("ZZZ", stmf.InForTests()["1"]); // should be after the "ZMany" so it should not return anything
            stmf.InForTests().Remove("1");
            stmf.InForTests().Add("1", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("2", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("3", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("Fallback5", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            Assert.AreEqual(45, stmf.ListForTests().Count(), "Should have looped through many and found the 45");
            Assert.AreEqual("ZMany", stmf.ReturnedStreamName);
        }

        [TestMethod]
        public void FindNothing()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.InForTests().Remove("1");
            stmf.InForTests().Add("1", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("2", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Add("3", stmf.InForTests()[DataSourceConstants.DefaultStreamName]);
            stmf.InForTests().Remove("ZMany");
            Assert.AreEqual(0, stmf.ListForTests().Count(), "Should find none");
        }

        public StreamFallback AssembleTestFallbackStream()
        {
            var emptyDs = new DataTablePerson(this).Generate(0, 1001);
            var streams = CreateDataSource<StreamFallback>(emptyDs);

            var dsWith1 = new DataTablePerson(this).Generate(1, 2000);
            var dsWithmany = new DataTablePerson(this).Generate(45, 4000);
            streams.AttachForTests("1", dsWith1);
            streams.AttachForTests("ZMany", dsWithmany);
            return streams;

        }
    }
}

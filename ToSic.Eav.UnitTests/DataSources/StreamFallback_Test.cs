using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class StreamFallback_Test
    {
        [TestMethod]
        public void StreamWhereDefaultIsReturned()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.In[Constants.DefaultStreamName] = stmf.In["1"];

            Assert.AreEqual(1, stmf.List.Count, "should have found 1");
        }

        [TestMethod]
        public void StreamsWhereFirstFallbackIsReturned()
        {
            var stmf = AssembleTestFallbackStream();
            Assert.AreEqual(1, stmf.List.Count, "should be 1 - the fallback");
        }

        [TestMethod]
        public void DoManyFoldFallback()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.In.Remove("1");
            stmf.In.Add("Fallback1", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("Fallback2", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("Fallback3", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("Fallback4", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("Fallback5", stmf.In[Constants.DefaultStreamName]);
            Assert.AreEqual(45, stmf.List.Count, "Should have looped through many and found the 45");
            Assert.AreEqual("ZMany", stmf.ReturnedStreamName);
        }

        [TestMethod]
        public void DoManyFallbacks2()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.In.Add("ZZZ", stmf.In["1"]); // should be after the "ZMany" so it should not return anything
            stmf.In.Remove("1");
            stmf.In.Add("1", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("2", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("3", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("Fallback5", stmf.In[Constants.DefaultStreamName]);
            Assert.AreEqual(45, stmf.List.Count, "Should have looped through many and found the 45");
            Assert.AreEqual("ZMany", stmf.ReturnedStreamName);
        }

        [TestMethod]
        public void FindNothing()
        {
            var stmf = AssembleTestFallbackStream();
            stmf.In.Remove("1");
            stmf.In.Add("1", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("2", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Add("3", stmf.In[Constants.DefaultStreamName]);
            stmf.In.Remove("ZMany");
            Assert.AreEqual(0, stmf.List.Count, "Should find none");
        }

        public StreamFallback AssembleTestFallbackStream()
        {
            var emptyDs = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(0, 1001);
            var streams = DataSource.GetDataSource<StreamFallback>(1, 1, emptyDs);

            var dsWith1 = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(1, 2000);
            var dsWithmany = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(45, 4000);
            streams.Attach("1", dsWith1);
            streams.Attach("ZMany", dsWithmany);
            return streams;

        }
    }
}

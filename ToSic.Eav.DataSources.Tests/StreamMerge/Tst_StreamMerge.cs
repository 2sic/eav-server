using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.LookUp;
using ToSic.Eav.UnitTests.DataSources;



namespace ToSic.Eav.DataSources.Tests.StreamMerge
{
    [TestClass]
    public class Tst_StreamMerge
    {

        [TestMethod]
        public void StreamMerge_In0()
        {
            var desiredFinds = 0;
            var sf = DataSource.GetDataSource<DataSources.StreamMerge>(0, 0, configLookUp: new LookUpEngine());
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");
        }

        [TestMethod]
        public void StreamMerge_In1()
        {
            var desiredFinds = 100;
            var sf = GenerateMergeDs(desiredFinds);
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");
        }

        [TestMethod]
        public void StreamMerge_In2()
        {
            // fi
            var items = 100;
            var desiredFinds = items * 2;
            var sf = GenerateMergeDs(items);
            sf.In.Add("another", sf.In.FirstOrDefault().Value);
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

        }

        [TestMethod]
        public void StreamMerge_In2Null1()
        {
            // fi
            var items = 100;
            var desiredFinds = items * 3;
            var sf = GenerateMergeDs(items);
            sf.In.Add("another", sf.In.FirstOrDefault().Value);
            sf.In.Add("middle", null);
            sf.In.Add("xFinal", sf.In.FirstOrDefault().Value);
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

        }

        private static DataSources.StreamMerge GenerateMergeDs(int desiredFinds)
        {
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(desiredFinds, 1001, true);
            var sf = DataSource.GetDataSource<DataSources.StreamMerge>(0, 0, ds);
            return sf;
        }
    }
}

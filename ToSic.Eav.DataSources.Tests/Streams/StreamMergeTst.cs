using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Streams
{
    [TestClass]
    public class StreamMergeTst: EavTestBase
    {

        [TestMethod]
        public void StreamMerge_In0()
        {
            var desiredFinds = 0;
            var sf = Resolve<DataSourceFactory>().GetDataSource<StreamMerge>(
                new AppIdentity(0, 0), null, 
                new LookUpEngine(null as ILog));
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
            sf.InForTests().Add("another", sf.InForTests().FirstOrDefault().Value);
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
            sf.InForTests().Add("another", sf.InForTests().FirstOrDefault().Value);
            sf.InForTests().Add("middle", null);
            sf.InForTests().Add("xFinal", sf.InForTests().FirstOrDefault().Value);
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

        }

        private static StreamMerge GenerateMergeDs(int desiredFinds)
        {
            var ds = DataTablePerson.Generate(desiredFinds, 1001, true);
            var sf = Resolve<DataSourceFactory>().GetDataSource<StreamMerge>(new AppIdentity(0, 0), ds);
            return sf;
        }
    }
}
